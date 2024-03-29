﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace sthvServer.sthvGamemodes
{
    internal class ClassicHunt : BaseGamemodeSthv
    {
        SthvPlayer runner = null;
        string runnerServerId = null;
        Shared.sthvMapModel map = Shared.sthvMaps.Maps[2];
        const string TRunner = "runner";
        const string THunter = "hunter";

        int currentmapid = 0;

        internal ClassicHunt() : base(GamemodeId: Shared.Gamemode.ClassicHunt, gameLengthInSeconds: GamemodeConfig.huntLengthSeconds, minimumNumberOfPlayers: 1, numberOfTeams: 2)
        {
            //Empty constructor so gamemode manager can cheaply loop over gamemodes to get metadata.
        }
        public override void CreateEvents()
        {
            AddTimeEvent(0, new Action(async () =>
            {
                Tick += runnerHintHandler;
                Random rand = new Random();
                if (GamemodeConfig.huntNextMapIndex > 0)
                {
                    currentmapid = GamemodeConfig.huntNextMapIndex;
                }
                else
                {
                    //pick random playarea 
                    currentmapid = rand.Next(0, Shared.sthvMaps.Maps.Length);
                }
                Server.currentMap = Shared.sthvMaps.Maps[currentmapid];
                map = Shared.sthvMaps.Maps[currentmapid];
                TriggerClientEvent("sthv:sendChosenMap", currentmapid);
                log("current map index: " + currentmapid);

                //Assigning teams.
                //Framework assures all dead players are ready for next hunt. Non-ready players could be loading, not authenticated, etc.
                var readyPlayers = sthvLobbyManager.GetPlayersOfState(playerState.ready);
                log(readyPlayers.Count + " ready players in this hunt.");

                //picking and assigning runner
                int runnerindex = rand.Next(0, readyPlayers.Count);
                log("picked runner index: " + runnerindex);

                readyPlayers[runnerindex].teamname = TRunner;
                //assigning everyone else hunter team
                foreach (var p in readyPlayers)
                {
                    if (p.teamname != TRunner)
                    {
                        p.teamname = THunter;
                    }
                }

                runner = sthvLobbyManager.GetPlayersInTeam(TRunner)[0];

                if (sthvLobbyManager.GetPlayersInTeam("runner").Count != 1) throw new Exception("Unexpected number of runners were assigned in ClassicHunt");
                runnerServerId = runner.player.Handle;

                log($"^2runner handle is now: {runnerServerId}^7");
                TriggerClientEvent("sth:updateRunnerHandle", int.Parse(runnerServerId));
                Server.SendChatMessage("^2HUNT", $"Runner is:{runner.Name}", 255, 255, 255);
                Server.SendToastNotif($"Hunt starting with runner: {runner.Name}", 3000);

                await Delay(100);
#if DEBUG
                runner.Spawn(map.HunterSpawn, true, playerState.alive);

#else
				runner.Spawn(map.RunnerSpawn, true, playerState.alive);
#endif

                //offer hunters to opt into runner ?
                TriggerClientEvent("removeveh");
                await Delay(3000);

                runner.player.TriggerEvent("sthv:spectate", false);
                Server.refreshscoreboard();
            }));
            AddTimeEvent(5, new Action(async () =>
            {
                log("spawning hunters after 5 seconds.");
                var hunters = sthvLobbyManager.GetPlayersInTeam(THunter);
                foreach (var h in hunters)
                {
                    h.Spawn(map.HunterSpawn, false, playerState.alive);
                }
                await Delay(1000);
                if(hunters.Count > 0) hunters[0].player.TriggerEvent("sthv:spawnhuntercars", currentmapid);
            }));
            AddTimeEvent(30, new Action(() =>
            {
                log("Giving weapons after 30 seconds");

                runner.player.TriggerEvent("sth:updateRunnerHandle", runnerServerId); //incase runner has wrong clothes

                TriggerClientEvent("sth:setguns", true);
                TriggerClientEvent("sth:setcops", GamemodeConfig.isPoliceEnabled);

                Server.SendChatMessage("^5HUNT", "You now have guns");
                Server.SendToastNotif("You now have weapons!");
            }));

            AddFinalizerEvent(new Action(() =>
            {
                TriggerClientEvent("sth:updateRunnerHandle", -1);
                foreach (var p in sthvLobbyManager.GetPlayersOfState(playerState.alive, playerState.ready))
                {
                    p.Spawn(map.HunterSpawn, false, playerState.ready);
                }

                TriggerClientEvent("removeveh");
            }));
        }

      			[EventHandler("gamemode::player_killed")]
		void playerKilledHandler(string killerLicense, string killedLicense)
		{
			var killed = sthvLobbyManager.getPlayerByLicense(killedLicense);

			//killerLicense is null when there's no killer. Player died from suicide or natural causes.
			if (killerLicense != null)
			{
				var killer = sthvLobbyManager.getPlayerByLicense(killerLicense);
				var isTeamkill = killer.teamname == killed.teamname;

				log($"{killer.player.Name} ({killer.teamname}) killed {killed.player.Name} ({killed.teamname}. Teamkill: {isTeamkill})");
				
				//friendly fire
				if (isTeamkill)
				{
					log("Teamkill");
					if (GamemodeConfig.isFriendlyFireAllowed)
					{
						Server.SendChatMessage("", $"^5{killer.Name} teamkilled {killed.Name}.");
						Debug.WriteLine("^5{killer.Name} teamkilled {killed.Name} because friendly fire is enable in GamemodeConfig.");
					}
					else
					{
						//punish killer and respawn killed if FF is disallowed.

						killer.player.TriggerEvent("sthv:kill"); //kills the teamkiller
						Server.SendChatMessage("", $"^5{killer.Name} was killed by Karma and {killed.Name} respawned.");				

						if (killed.teamname == TRunner) killed.Spawn(map.RunnerSpawn, true, playerState.alive); //spawns killed player at spawn location
						else if (killed.teamname == THunter) killed.Spawn(map.HunterSpawn, false, playerState.alive);
						else log("Killed isn't a runner or hunter!?");
						killed = killer; //this lets the teamkiller respawn later. Killed is already respawned, so doesn't to set a future respawn. 

 
					}
				}
			}
			//respawn player after 2 mins
			var timer = GamemodeConfig.respawnTimeSeconds;
			if (TimeLeft > timer + 10)
			{
				AddTimeEvent(timer + TimeSinceRoundStart, new Action(() =>
				{
					killed.Spawn(map.HunterSpawn, false, playerState.alive);
					log("Spawning killed player after respawn time: " + killed.Name);
					Server.SendChatMessage("sthv", "Spawning " + killed.Name + " after respawn time.");
				}));
				AddTimeEvent(timer + TimeSinceRoundStart + 7, new Action(() =>
				{
					killed.player.TriggerEvent("sth:setguns", true);
				}));
			}
		}
        
        [EventHandler("admin_menu_save_request")]
        void adminMenuSaveHandler([FromSource] Player source, string jsonData)
        {
            Debug.WriteLine(jsonData);
            if (API.IsPlayerAceAllowed(source.Handle, "sthv.host"))
            {
                try
                {
                    Debug.WriteLine("jsondata: " + jsonData);
                    var data = JsonConvert.DeserializeObject<Shared.AdminMenuSave>(jsonData);
                    if (data.next_respawn_time != 0 &&
                        GamemodeConfig.respawnTimeSeconds != data.next_respawn_time)
                    {
                        GamemodeConfig.respawnTimeSeconds = data.next_respawn_time;
                    }
                    if (data.next_runner_serverid != "0") GamemodeConfig.huntNextRunnerServerId = data.next_runner_serverid;
                    if (data.next_hunt_length != 0)
                    {
                        GamemodeConfig.huntLengthSeconds = data.next_hunt_length * 60;
                    }

                    GamemodeConfig.huntNextMapIndex = data.next_map;
                    GamemodeConfig.isFriendlyFireAllowed = data.is_friendly_fire_allowed;

                    if (GamemodeConfig.secondsBetweenHints != data.seconds_between_hints)
                    {
                        GamemodeConfig.secondsBetweenHints = data.seconds_between_hints;
                        if (GamemodeConfig.secondsBetweenHints < 5)
                        {
                            GamemodeConfig.secondsBetweenHints = 5;
                            Server.SendChatMessage("Host Menu", "Time Between Hunts was set to the minimum value of 5 seconds.", 255, 255, 255, source.Handle);
                        }
                    }
                    if (GamemodeConfig.isPoliceEnabled != data.is_cops_enabled)
                    {
                        if (data.is_cops_enabled) Server.SendChatMessage("Host Menu", "Cops will be enabled next round.", 255, 255, 255, source.Handle);
                        else Server.SendChatMessage("Host Menu", "Cops will be disabled next round.", 255, 255, 255, source.Handle);
                        GamemodeConfig.isPoliceEnabled = data.is_cops_enabled;
                    }

                    if (data.end_hunt) sthvLobbyManager.winnerTeamAndReason = ("Nobody", "Host: " + source.Name + " ended the hunt with Host Menu.");
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Exception thrown in Classic Hunt becuase jsonconvert failed on admin_menu_save_request. \n" + e);
                }
            }
            else
            {
                Debug.WriteLine($"Player {source.Name} tried sending admin_menu_save_request without permission!");
                source.Drop("Permission denied: Host Menu. Contact server owner if you think this is a mistake.");
            }
        }
        async Task runnerHintHandler()
        {
            if(runner == null) 
            {
				log("^3runnerHintHandler ran when runner was null."); 
				await Delay(6000);
				return;
			}
			Vector3 pos = runner.player.Character.Position;
			TriggerClientEvent("sthv:showRunnerOnMap", pos);
            await Delay((int)GamemodeConfig.secondsBetweenHints * 1000);
        }
    }
}
