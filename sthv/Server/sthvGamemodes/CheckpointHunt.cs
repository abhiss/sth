using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

using Shared;

namespace sthvServer.sthvGamemodes
{
	internal class CheckpointHunt : BaseGamemodeSthv
	{
		SthvPlayer runner = null;
		string runnerServerId = null;
		Shared.sthvMapModel map = Shared.sthvMaps.Maps[5];
		const string TRunner = "runner";
		const string THunter = "hunter";
		readonly float check_radius = 15;
		Random rand = new Random();

		int currentmapid;
		readonly List<Vector3> UncapturedCheckpoints = new List<Vector3>();
		internal CheckpointHunt() : base(GamemodeId: Gamemode.CheckpointHunt, gameLengthInSeconds: GamemodeConfig.huntLengthSeconds, minimumNumberOfPlayers: 2, numberOfTeams: 2){}

		public override void CreateEvents()
		{
			AddTimeEvent(0, new Action(async () =>
			{
				Tick += CheckpointHandler;

				UncapturedCheckpoints.AddRange(checkpointPositions);
				var cpoint_index = rand.Next(0, UncapturedCheckpoints.Count);
				var pos = UncapturedCheckpoints[cpoint_index];
				TriggerClientEvent("createcheckpoint", cpoint_index, check_radius, pos);

				//Assigning teams.
				//Framework assures all dead players are ready for next hunt. Non-ready players could be loading, not authenticated, etc.
				var readyPlayers = sthvLobbyManager.GetPlayersOfState(playerState.ready);
				log(readyPlayers.Count + " ready players in this hunt.");

				//picking and assigning runner
				int runnerindex = rand.Next(0, readyPlayers.Count);
				Debug.WriteLine("Choosing runner index: " + runnerindex, "readyPlayers size: " + readyPlayers.Count);
				readyPlayers[runnerindex].teamname = TRunner;
				//assigning everyone else hunter team
				foreach (var p in readyPlayers)
				{
					if (p.teamname != TRunner)
					{
						p.teamname = THunter;
					}
				}

				//BaseGamemode picks runner already.
				runner = sthvLobbyManager.GetPlayersInTeam(TRunner)[0];

				if (sthvLobbyManager.GetPlayersInTeam("runner").Count != 1) throw new Exception("Unexpected number of runners were assigned in ClassicHunt");
				runnerServerId = runner.player.Handle;

				log($"^2runner handle is now: {runnerServerId}^7");
				TriggerClientEvent("sth:updateRunnerHandle", int.Parse(runnerServerId));
				Server.SendChatMessage("^CheckpointHunt", $"Runner is:{runner.Name}", 255, 255, 255);
				Server.SendToastNotif($"CheckpointHunt starting with runner: {runner.Name}", 3000);

				await Delay(100);

				runner.Spawn(map.RunnerSpawn, true, playerState.alive);

				//offer hunters to opt into runner ?
				TriggerClientEvent("removeveh");
				await Delay(500);

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
			}));
			AddTimeEvent(30, new Action(() =>
			{
				log("Giving weapons after 30 seconds");

				runner.player.TriggerEvent("sth:updateRunnerHandle", runnerServerId); //incase runner has wrong clothes

				TriggerClientEvent("sth:setguns", true);
				TriggerClientEvent("sth:setcops", GamemodeConfig.isPoliceEnabled);

				Server.SendChatMessage("^5HUNT", "You now have guns");
				Server.SendToastNotif("You now have weapons!");
				Server.SendToastNotif("You now have weapons!");
			}));

			AddFinalizerEvent(new Action(() =>
			{
				TriggerClientEvent("sth:updateRunnerHandle", -1);
				foreach (var p in sthvLobbyManager.GetPlayersOfState(playerState.alive, playerState.ready))
				{
					p.Spawn(map.HunterSpawn, true, playerState.ready);
				}
				UncapturedCheckpoints.Clear();

				TriggerClientEvent("removeveh");
				Tick -= CheckpointHandler;
			}));
		}

		[EventHandler("gamemode::player_killed")]
		void playerKilledHandler(string killerLicense, string killedLicense)
		{
			//killerLicense is null when there's no killer. Player died from suicide or natural causes.
			if (killerLicense == null)
			{
				return;
			}
			var killer = sthvLobbyManager.getPlayerByLicense(killerLicense);
			var killed = sthvLobbyManager.getPlayerByLicense(killedLicense);

			log($"{killer.player.Name} ({killer.teamname}) killed {killed.player.Name} ({killed.teamname})");

			//friendly fire
			if (killer.teamname == killed.teamname)
			{
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
					killed = killer;
				}
			}

			//respawn player after 2 mins
			var timer = GamemodeConfig.respawnTimeSeconds;
			if (TimeLeft > timer + 10)
			{
				AddTimeEvent(timer + TimeSinceRoundStart, new Action(() =>
				{
					if (killed.teamname == THunter) killed.Spawn(map.HunterSpawn, false, playerState.alive);
				}));
			}
		}

		[EventHandler("gamemode::player_join_late")]
		async Task player_joined_late_hander(SthvPlayer player)
		{
			if(base.TimeSinceRoundStart > 30)
			{
				await Delay(10000);
				TriggerClientEvent("sth:setguns", true);
				TriggerClientEvent("sth:setcops", GamemodeConfig.isPoliceEnabled);
				player.Spawn(map.HunterSpawn, false, playerState.alive);
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
		
		[EventHandler("tookcheckpoint")]
		void takenCheckpointHandler(int _checkpointId)
		{

			log("event recieved: tookcheckpoint for checkpointid: " + _checkpointId);
			if(_checkpointId >= UncapturedCheckpoints.Count)
			{
				log("Error in takenCheckpointHandler, _checkpointId >= UncapturedCheckpoints.Count");
				
				//todo: remove exception after testing and handle more gracefully. Can be abused cheaters. 
				throw new Exception("Error in takenCheckpointHandler, _checkpointId >= UncapturedCheckpoints.Count");
			}
			UncapturedCheckpoints.RemoveAt(_checkpointId);
			TriggerClientEvent("removecheckpoint", _checkpointId);

			var cpoint_index = rand.Next(0, UncapturedCheckpoints.Count);
			var pos = UncapturedCheckpoints[cpoint_index];
			TriggerClientEvent("createcheckpoint", cpoint_index, check_radius, pos);

		}

		async Task CheckpointHandler()
		{
			await Delay(10000);
		}
		private readonly Vector3[] checkpointPositions = {
			new Vector3(637.8491f, -1437.132f, 29.70673f),
			new Vector3(466.6298f, -1175.656f, 40.5019f),
			new Vector3(-110.492f, -1044.861f, 26.27357f),
			new Vector3(68.3438f, -673.9508f, 43.3111f),
			new Vector3(148.1946f, -568.7192f, 42.88696f),
			new Vector3(145.5018f, -355.9124f, 42.30743f),
			new Vector3(-233.0494f, -212.1216f, 48.1279f),
			new Vector3(-777.076f, -85.77178f, 36.79874f),
			new Vector3(-1951.711f, -338.5465f, 44.9702f),
			new Vector3(-1482.905f, -807.0694f, 22.80004f),
			new Vector3(-1335.343f, -1469.028f, 3.313878f),
			new Vector3(-724.3319f, -1411.443f, 4.000523f),
			new Vector3(-610.2518f, -2028.66f, 16.21686f),
			new Vector3(-601.7064f, -2011.402f, 28.27418f),
			new Vector3(-617.6996f, -2052.146f, 26.32173f),
			new Vector3(-796.9703f, -2500.618f, 12.75971f),
			new Vector3(201.8326f, -2577.078f, 5.20483f),
			new Vector3(1380.117f, -2341.818f, 60.47023f),
			new Vector3(1082.611f, -1188.037f, 45.92384f),
			new Vector3(-217.6544f, 261.0845f, 91.08981f),
			new Vector3(565.9612f, -1218.734f, 8.899653f),
			new Vector3(1021.131f, -352.4419f, 47.23037f),
			new Vector3(1882.359f, -1878.276f, 191.0228f),
			new Vector3(1400.738f, -1522.626f, 57.19899f),
			new Vector3(899.2529f, -2453.76f, 27.40315f),
			new Vector3(731.9427f, -2642.001f, 14.45242f),
			new Vector3(719.0999f, -3063.99f, 12.31112f),
			new Vector3(1020.065f, -3155.32f, 4.900775f),
			new Vector3(441.5827f, -1399.592f, 28.31816f),
			new Vector3(-528.8695f, 663.2872f, 140.4203f),
			new Vector3(-632.6037f, -354.9757f, 33.82269f)};
	}

}
