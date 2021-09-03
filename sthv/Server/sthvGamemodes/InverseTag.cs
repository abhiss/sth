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

		SthvPlayer _runner = sthvLobbyManager.GetAllPlayers()[0];
		public SthvPlayer runner
		{
			get { return _runner; }
			set { 
					TriggerClientEvent("sth:new_runner", value.player.Handle);
					//make old runner hunter since there will 
					//only be a single runner at a time.
					_runner.teamname = THunter;
					value.teamname = TRunner;
					sthvLobbyManager.getPlayerByLicense(value.player.getLicense()).teamname = TRunner;
					if(_runner != null)
					sthvLobbyManager.getPlayerByLicense(_runner.player.getLicense()).teamname = TRunner;

					_runner = value;
					Server.refreshscoreboard();
				}
		}
		
		readonly string TRunner = "runner";
		readonly string THunter = "hunter";
		int currentmapid = 0;
		bool isRecentlyTagged = false;
		Random rand = new Random();
		int noTagbackTimeMS = 10_000;

		Shared.sthvMapModel map = Shared.sthvMaps.Maps[5];

		override public void CreateEvents()
		{
			AddTimeEvent(0, new Action(() =>
			{
				Tick += runnerHintHandler;
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
				log("RUNNER SET TO " + runner.player.Handle + " and has teamname: " + runner.teamname);

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
				TriggerClientEvent("sth:new_runner", runner.player.Handle);
				log("setting runner to: " + runner.player.Handle);
			}));

			AddFinalizerEvent(new Action(() =>
			{
				TriggerClientEvent("sth:updateRunnerHandle", -1);
				TriggerClientEvent("removeveh");
			}));
		}

#if DEBUG
		[Command("bring")]
		void cmd_bring(){
			if(runner is null)
			{
				log("Runner is null.");
			}
			foreach(var p in sthvLobbyManager.GetAllPlayers()) {
				var pos =  runner.player.Character.Position;
				p.player.Character.Position = pos;
				
			}
		}
#endif

		[EventHandler("gamemode::player_join_late")]
		async void player_join_late_handler(string playerLicense)
		{
			await Delay(5000);
			TriggerClientEvent("sthv:sendChosenMap", currentmapid);
			TriggerClientEvent("sth:updateRunnerHandle", int.Parse(runner.player.Handle));
		}

		[EventHandler("sth:tagged_runner")]
		async void tagged_runner_handler([FromSource]Player source, int runnerServerId){
			log("message from source: " + source.Name + " handle: " + source.Handle + " tagged: " + runnerServerId + " runner: " + runner.Name);
			//avoid tag-backs			
			if(isRecentlyTagged) return;

			if(runnerServerId < 1) {
				TriggerClientEvent("sth:new_runner", runner.player.Handle);
				log($"^4Got invalid runnerServerId: {runnerServerId} in tagged_runner_handler from player {source.Name}.");
				return;
			}
			if(runnerServerId != int.Parse(runner.player.Handle)){
				TriggerClientEvent("sth:new_runner", runner.player.Handle);
				log($"^4Player {source.Name} sent tagged runner with serverid {runnerServerId} but current runner's server id is {runner.player.Handle}.");
				return;
			}

			isRecentlyTagged = true;

			//assign new runner
			log("new runner serverid: " + source + " " + source.Name);
			runner = new SthvPlayer(source);
			log("new runner: " + runner.Name);

			TriggerClientEvent("sthv:no_tagback_time_ms", noTagbackTimeMS);
			await Delay(noTagbackTimeMS);
			isRecentlyTagged = false;
			Server.refreshscoreboard();
		}

		[EventHandler("sth:player_not_in_car_too_long")]
		void runner_not_in_car_too_long([FromSource]Player source){
			//checks
			if(source.Handle != runner.player.Handle){
				log("Player " + source.Name + "  " + source.Handle + " triggered sth:player_not_in_car_too_long but isn't runner:" + runner.player.Handle+ ". Exceptional.");
				source.TriggerEvent("sth:new_runner", runner.player.Handle);
				return;
			}
			else //assign new runner
			{ 
				var potential_runners = sthvLobbyManager.GetPlayersInTeam(THunter);

				if(potential_runners.Count > 0){ //usually means everyones dead and gamemode is about to end.
					log("Runner should be removed for being out of car too long but are no hunters to make runners.");
					var new_runner_index = rand.Next(0, potential_runners.Count);
					runner = potential_runners[new_runner_index];
					
					Server.SendChatMessage("Inverse Tag", $"Runner " + source.Name + " was not in a car for too long. " + runner.Name + " was randomly chosen as next runner.");
				}
			}
		}

		[EventHandler("gamemode::player_killed")]
		void playerKilledHandler(string killerLicense, string killedLicense)
		{
			var player = sthvLobbyManager.getPlayerByLicense(killedLicense);
			if(player == null) return;
			player.Spawn(map.RunnerSpawn, false, playerState.alive);
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
			if(runner == null) {
				log("^3runnerHintHandler ran when runner was null."); 
				await Delay(6000);
				return;
			}
			Vector3 pos = runner.player.Character.Position;
			TriggerClientEvent("sthv:showRunnerOnMap", pos);
			await Delay((int)GamemodeConfig.secondsBetweenHints * 1000/2); //time /2 since tagged players should almost always be on map.
		}
	}
}

