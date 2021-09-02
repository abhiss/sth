using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
namespace sthvServer.sthvGamemodes
{
	class InverseTag : BaseGamemodeSthv
	{
		internal InverseTag() : base(GamemodeId: Shared.Gamemode.InverseTag, gameLengthInSeconds: GamemodeConfig.huntLengthSeconds, minimumNumberOfPlayers: 2, numberOfTeams: 2){}

		//"runner" is tagged
		SthvPlayer runner = sthvLobbyManager.GetAllPlayers()[0];
		readonly string TRunner = "runner";
		readonly string THunter = "hunter";
		int currentmapid = 0;
		bool isRecentlyTagged = false;

		Shared.sthvMapModel map = Shared.sthvMaps.Maps[5];


		override public void CreateEvents()
		{
			AddTimeEvent(0, new Action(() =>
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
				log(readyPlayers.Count + " ready players in this game.");

				//picking and assigning runner
				int runnerindex = rand.Next(0, readyPlayers.Count - 1);
				runner = readyPlayers[runnerindex];
				runner.teamname = TRunner;

				//assigning everyone else hunter team
				foreach (var p in readyPlayers)
				{
					if (p.teamname != TRunner)
					{
						p.teamname = THunter;
					}
				}

				//announce teams to clients
				log($"^2runner handle is now: {runner.player.Handle}^7");
				TriggerClientEvent("sth:updateRunnerHandle", int.Parse(runner.player.Handle));
				Server.SendChatMessage("^2HUNT", $"Runner is:{runner.Name}", 255, 255, 255);
				Server.SendToastNotif($"InverseTag starting with runner: {runner.Name}", 3000);

				//spawn runner
				runner.Spawn(map.RunnerSpawn, true, playerState.alive);

				runner.player.TriggerEvent("sthv:spawnhuntercars", currentmapid);
				runner.player.TriggerEvent("sthv:spectate", false);
				Server.refreshscoreboard();
			}));

			AddTimeEvent(5, new Action(() =>
			{
				log("spawning hunters after 5 seconds.");
				var hunters = sthvLobbyManager.GetPlayersInTeam(THunter);
				foreach (var h in hunters)
				{
					h.Spawn(map.HunterSpawn, false, playerState.alive);
				}
				TriggerClientEvent("sth:new_runner", runner.player.Handle);
			}));

			AddFinalizerEvent(new Action(() =>
			{
				TriggerClientEvent("sth:updateRunnerHandle", -1);

				//TriggerClientEvent("removeveh");
				Tick -= runnerHintHandler;
			}));
		}

		[EventHandler("gamemode::player_join_late")]
		async void player_join_late_handler(string playerLicense)
		{
			await Delay(5000);
			TriggerClientEvent("sthv:sendChosenMap", currentmapid);
			TriggerClientEvent("sth:updateRunnerHandle", int.Parse(runner.player.Handle));
		}

		[EventHandler("sth:tagged_runner")]
		async void tagged_runner_handler([FromSource]Player source, int runnerServerId){
			if(isRecentlyTagged) return;

			if(runnerServerId < 1) {
				log($"^4Got invalid runnerServerId: {runnerServerId} in tagged_runner_handler from player {source.Name}.");
				
			}

			isRecentlyTagged = true;
			log("new runner serverid: " + runnerServerId);
			this.runner = sthvLobbyManager.getPlayerByLicense(Players[runnerServerId].getLicense());
			log("new runner: " + runner.Name);
			TriggerClientEvent("sth:new_runner", runnerServerId);
			await Delay(5000);
			
			isRecentlyTagged = false;
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
			Vector3 pos = runner.player.Character.Position;
			TriggerClientEvent("sthv:showRunnerOnMap", pos);
			await Delay((int)GamemodeConfig.secondsBetweenHints * 1000);
		}
	}
}

