using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.IO;
using Newtonsoft.Json;

using Shared;

namespace sthvServer
{

	class Server : BaseScript
	{
		int runnerHandle { get; set; }
		static public Player runner;
		public static bool hasHuntStarted = false; //
		List<Player> NextRunnerQueue = new List<Player>();
		List<Player> AlivePlayerList = new List<Player>();
		static public bool isHuntOver = true;
		public static bool TestMode { get; set; } = false;
		public static sthvMapModel currentMap;
		
		/// <summary>
		/// Set by BaseGamemode when a new gamemode starts.
		/// </summary>
		public static Gamemode GamemodeId = Gamemode.None; 
		public static bool IsDiscordServerOnline { get; set; } = false;
		public bool AutoHunt { get; set; } = false;

		public sthvDiscordController discord { get; }

		BaseGamemodeSthv gamemode;

		public static List<int> playersInHeliServerid { get; set; } = new List<int>();

		[Command("test")]
		void t()
		{
		}
		public Server()
		{
			discord = new sthvDiscordController();

			currentMap = Shared.sthvMaps.Maps[2];
			FetchHandler fetchHandler = new FetchHandler();

			fetchHandler.addHandler<Shared.PlayerJoinInfo>(new Func<Player, Shared.BaseFetchClass>(source =>
			{

				Debug.WriteLine($"^3 player: {source.Name} Triggered PlayerJoinInfo handler.^7");
				string licenseId = source.Handle;
				source.TriggerEvent("sth:updateRunnerHandle", runnerHandle);
				sthvLobbyManager.getPlayerByLicense(source.getLicense()).Spawn(currentMap.HunterSpawn, false, playerState.ready); //defaults to hunter spawn
				refreshscoreboard();
				
				//retry bc sometimes nui is still not finished loading after player is ready.
				var send_sb_later = new Action(async () =>
				{
					await Delay(5000);
					refreshscoreboard();
					Debug.WriteLine("Trying scoreboard again_________________________________----------www");
				});
				send_sb_later();
				//todo isAllowedHostMenu should use database or something instead of ace perms.
				return (new Shared.PlayerJoinInfo
				{
					hasDiscord = false,
					isDiscordServerOnline = false,
					isInSTHGuild = false,
					isInVc = false,
					runnerServerId = runnerHandle,
					isAllowedHostMenu = API.IsPlayerAceAllowed(source.Handle, "sthv.host"),
					gamemodeId = Server.GamemodeId
				});

			}));
			fetchHandler.addHandler<Shared.Ping>(new Func<Player, Shared.BaseFetchClass>(source =>
			{
				return (new Shared.Ping { response = "pong!!" });
			}));

			EventHandlers["sth:sendServerDebug"] += new Action<string>((string info) => { Debug.WriteLine(info); });

			API.RegisterCommand("spawn", new Action<int, List<object>, string>((src, args, raw) =>
			{
				try
				{
					int playerHandle = int.Parse(args[0].ToString());
					Player _playerToSpawn = GetPlayerFromHandle(playerHandle);
					_playerToSpawn.TriggerEvent("sth:spawnall", true);
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"^1Error (probably passed invalid arguments): {ex}^7");
				}
			}), true);
			API.RegisterCommand("testmode", new Action<int, List<object>, string>((src, args, raw) =>
			{
				TestMode = !TestMode;
				Debug.WriteLine("testmode: " + TestMode.ToString());
			}), true);
			API.RegisterCommand("endhunt", new Action<int, List<object>, string>((src, args, raw) =>
			{
				isHuntOver = true;
			}), true);
			API.RegisterCommand("remveh", new Action<int, List<object>, string>((src, args, raw) =>
			{
				TriggerClientEvent("removeveh");
			}), true);
			API.RegisterCommand("spawnall", new Action<int, List<object>, string>((src, args, raw) =>
			{
				Debug.WriteLine();
				TriggerClientEvent("sth:spawnall");
				TriggerClientEvent("hudintrooff");
			}), true);
			//test 

			EventHandlers["logCoords"] += new Action<List<Vector3>>((coordlist) =>
			{
				Debug.WriteLine("triggered log");
				if (coordlist.Count > 0)
				{
					string userpath = $"{API.GetResourcePath(API.GetCurrentResourceName())}/coordsData.txt";
					Debug.WriteLine($"{userpath}");
					var exists = File.Exists(userpath);

					File.Delete(userpath);
					if (true)
					{
						using (var file = File.CreateText(userpath))
						{

							foreach (var i in coordlist)
							{
								//file.WriteLine($"new Vector4({i.X}f, {i.Y}f, {i.Z}f),");
								//Debug.WriteLine($"new Vector4({i.X}f, {i.Y}f, {i.Z}f,");

							}
							file.Flush();
						}
					}
				}
				else
				{
					Debug.WriteLine("coordlist was empty");
				}
			});

			//new hunt related
			EventHandlers["sth:sendserverkillerserverindex"] += new Action<Player, int>(KillfeedEventHandler);

		}

		[Tick]
		private async Task firstTick()
		{
			Tick -= firstTick;

			Debug.WriteLine("Starting server!");
			await Delay(5000);

			//StartHunt(25);

			//wait for atleast 1 player
			//while (Players.Count() < 1) await Delay(1000);
			bool toggle = true;
			while (true)
			{
				if (Players.Count() < 2 && !TestMode)
				{
					await Delay(5000);
#if DEBUG
					Debug.WriteLine("^8waiting for more players!^7");
#endif
					continue;
				}

				//$ select gamemode based on player count
				if (toggle)
				{
					gamemode = new sthvGamemodes.CheckpointHunt();
				}
				else
				{
					gamemode = new sthvGamemodes.ClassicHunt();
				}
				toggle = !toggle;

				gamemode.CreateEvents();

				Debug.WriteLine("Instantiating gamemode " + gamemode.GamemodeId + ".");

				Debug.WriteLine("^4Registering gamemode script.^7");
				RegisterScript(gamemode);

				sthvLobbyManager.isGameActive = true;
				var (winner, reason) = await gamemode.Run();
				sthvLobbyManager.isGameActive = false;

				SendChatMessage("HUNT", "Team " + winner + "s win because " + reason + "!", 225, 225, 100);
				SendToastNotif("Team " + winner + "s win because " + reason + "!", 4000);
				await Delay(11000);

				Debug.WriteLine("^4Unregistering gamemode script.^7");
				UnregisterScript(gamemode);
			}
		}


		[EventHandler("sthv:opttorun")]
		void addToRunnerList([FromSource] Player source)
		{
			NextRunnerQueue.Add(source);
			Debug.WriteLine(source.Name + " opted to run");
		}

		/// <summary>
		/// check for null return
		/// </summary>
		/// <param name="playerId"></param>
		/// <returns></returns>
		Player GetPlayerFromHandle(int playerId)
		{
			try
			{
				foreach (Player p in Players)
				{
					if (int.Parse(p.Handle) == playerId)
					{
						return p;
					}
				}
				return null;
			}
			catch (Exception ex)
			{
				Debug.Write($"^3ERROR THROWN IN GetPlayerFromId: {ex}");
				return null;
			}
		}

		[EventHandler("baseevents:onPlayerDied")]
		void onPlayerDead([FromSource] Player player)
		{
			KillfeedEventHandler(player, -1);
		}
		void KillfeedEventHandler([FromSource] Player killed, int KillerIndex)
		{
			Debug.WriteLine("Halnding killfeed event.");
			Debug.WriteLine($"Log: KillfeedEventHandler killed.Name: {killed.Name} KillerIndex: {KillerIndex}");

			TriggerClientEvent("sthv:updateAlive", killed.Handle, false);

			//if there is no killer
			if (KillerIndex < 1)
			{
				TriggerEvent("gamemode::player_killed", null, killed.getLicense());

				SendChatMessage("KILLFEED", killed.Name + " died.");
				sthvLobbyManager.MarkPlayerDead(killed, killed.Name, killed.getLicense());

				if (int.Parse(killed.Handle) == runnerHandle && !isHuntOver)
				{
					SendChatMessage("HUNT", "Runner " + runner.Name + " died.");
					SendToastNotif("Runner " + runner.Name + " died.");
				}
			}
			else foreach (Player i in Players)
				{
					Debug.WriteLine($"playerhandles = {i.Handle}");
					if (int.Parse(i.Handle) == KillerIndex)
					{
						Debug.WriteLine($"killerhandle ={i.Handle}\nkillername = {i.Name}");
						Debug.WriteLine($"{i.Name} killed {killed.Name}");
						sthvLobbyManager.MarkPlayerDead(killed, i.Name, i.getLicense());

						Player killer = i;
						TriggerEvent("gamemode::player_killed", i.getLicense(), killed.getLicense());

						if (runner != null)
						{
							Debug.WriteLine("runner: " + runner.Name);
							if (killed.Handle == runner.Handle && !isHuntOver) //if runner gets killed 
							{
								isHuntOver = true;
								SendChatMessage("^5HUNT", $"{i.Name} killed runner: {runner.Name}");
							}
							else if (i.Handle == runner.Handle) //if runner gets a kill
							{
								SendChatMessage("^1HUNT", $"Runner {i.Name} killed hunter {killed.Name}");
							}
							else //teamkills are handled by gamemodes
							{
								SendChatMessage("^1KILLFEED", $"{i.Name} teamkilled {killed.Name}");
								//i.TriggerEvent("sthv:kill");
								//SendChatMessage("", $"^5{i.Name} was killed by Karma and {killed.Name} respawned.");
								//killed.TriggerEvent("sth:spawn", 2);
								//killed.TriggerEvent("sth:setguns", true);

								//sthvLobbyManager.DeadPlayers.RemoveAll(p => p == killed.getLicense());
							}

						}
						else //means before hunt started
						{
							SendChatMessage("^1KILLFEED", $"{i.Name} killed {killed.Name}");
							i.TriggerEvent("sthv:kill");
							SendChatMessage("", $"^5{i.Name} was killed by Karma");
						}
					}
				}

			TriggerEvent("sthv:checkaliveplayers");
		}
		public static void refreshscoreboard()
		{
			Debug.WriteLine("REFRESHING SCOREBOARD");
			List<Shared.ScoreboardInfoPlayer> sb_playerlist = new List<Shared.ScoreboardInfoPlayer>();
			foreach (SthvPlayer player in sthvLobbyManager.GetAllPlayers())
			{
				sb_playerlist.Add(new Shared.ScoreboardInfoPlayer()
				{
					is_alive = (player.State == playerState.alive || player.State == playerState.ready),
					is_in_helicopter = playersInHeliServerid.Contains(int.Parse(player.player.Handle)),
					is_runner = (player.teamname == "runner"),
					serverid = player.player.Handle
				});
			}
			Debug.WriteLine(JsonConvert.SerializeObject(sb_playerlist));
			TriggerClientEvent("sthv:refreshsb", JsonConvert.SerializeObject(sb_playerlist));
		}
		[EventHandler("sthv:isinheli")]
		private void onPlayerJustHeli([FromSource] Player source, bool isJustInHeli) //true when just entered heli, false when just left heli
		{
			Debug.WriteLine(source.Name + " justinheli: " + isJustInHeli.ToString());
			if (isJustInHeli)
			{
				playersInHeliServerid.Add(int.Parse(source.Handle));
			}
			else
			{
				playersInHeliServerid.RemoveAll(p => p == int.Parse(source.Handle));
			}
			refreshscoreboard();
		}

		public static void SendChatMessage(string title, string message, int r = 255, int g = 255, int b = 255, string serverid = null)
		{
			var msg = new Dictionary<string, object>
			{
				["color"] = new[] { r, g, b },
				["args"] = new[] { title, message }
			};
			if (serverid != null)
			{
				var targetPlayer = sthvLobbyManager.GetAllPlayers().First(p => p.player.Handle == serverid);
				if (targetPlayer != null)
				{
					targetPlayer.player.TriggerEvent("chat:addMessage", msg);
				}
			}
			TriggerClientEvent("chat:addMessage", msg);
			if (title == "^1KILLFEED") TriggerClientEvent("sthv:showToastNotification", msg, 1000);

		}
		public static void SendToastNotif(string message, int displayTimeInSeconds = 2000)
		{
			TriggerClientEvent("sthv:showToastNotification", message, displayTimeInSeconds);

		}
		public enum spawnType
		{
			idle = 0,
			runner = 1,
			hunter = 2,
			spectator = 3
		}

		public void RegisterEventHandler(string eventName, Delegate action)
		{
			EventHandlers[eventName] += action;
		}
		public void RegisterTickHandler(Func<Task> tick)
		{
			Tick += tick;
		}
		public void DeregisterTickHandler(Func<Task> tick)
		{
			Tick -= tick;
		}
		public void RegisterExport(string exportName, Delegate callback)
		{
			Exports.Add(exportName, callback);
		}
		public dynamic GetExport(string resourceName)
		{
			return Exports[resourceName];
		}
	}

}