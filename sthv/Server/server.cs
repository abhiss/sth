using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.IO;
using Newtonsoft.Json;

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
		public static Shared.sthvMapModel currentMap;
		
		public static bool IsDiscordServerOnline { get; set; } = false;
		public bool AutoHunt { get; set; } = false;

		public sthvDiscordController discord { get; }

		BaseGamemodeSthv gamemode;

		public static List<int> playersInHeliServerid { get; set; } = new List<int>();

		public Server()
		{
			discord = new sthvDiscordController();

			currentMap = Shared.sthvMaps.Maps[2];
			FetchHandler fetchHandler = new FetchHandler();

			fetchHandler.addHandler<Shared.PlayerJoinInfo>(new Func<Player, Shared.BaseSharedClass>(source =>
			{
				//retry loop incase of connection issues

				Debug.WriteLine($"^3 player: {source.Name} Triggered PlayerJoinInfo handler.^7");
				string licenseId = source.Handle;
				var discordid = source.getDiscordId();
				refreshscoreboard();
				source.TriggerEvent("sth:updateRunnerHandle", runnerHandle);
				sthvLobbyManager.getPlayerByLicense(source.getLicense()).Spawn(currentMap.HunterSpawn, false, playerState.ready); //defaults to hunter spawn

				TriggerClientEvent("hudintrooff");

				return (new Shared.PlayerJoinInfo { hasDiscord = false, isDiscordServerOnline = false, isInSTHGuild = false, isInVc = false, runnerServerId = runnerHandle });

			}));
			fetchHandler.addHandler<Shared.Ping>(new Func<Player, Shared.BaseSharedClass>(source =>
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

			EventHandlers["logCoords"] += new Action<List<dynamic>>((coordlist) =>
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

							//foreach (var i in coordlist)
							//{
							//	file.WriteLine($"new Vector4({i.X}f, {i.Y}f, {i.Z}f, {i.W}f),");
							//	Console.WriteLine($"new Vector4({i.X}f, {i.Y}f, {i.Z}f, {i.W}f),");

							//}
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

			Debug.WriteLine("Starting server");
			await Delay(5000);

			//StartHunt(25);

			//wait for atleast 1 player
			//while (Players.Count() < 1) await Delay(1000);

			while (true)
			{
				if (Players.Count() < 1)
				{
					await Delay(5000);
					Debug.WriteLine("^8waiting for more players^7");
					continue;
				}

				//$ select gamemode based on player count
				gamemode = new sthvGamemodes.ClassicHunt();
				Debug.WriteLine("Instantiating gamemode " + gamemode.Name + ".");

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
		/// runner defaults to -1 which randomly chooses a player to run
		/// </summary>
		/// <param name="timeInMinutes"></param>
		/// <param name="playarea"></param>
		/// <param name="runnerID"></param>
		async void StartHunt(int timeInMinutes, int playarea = -1, int runnerID = -1)
		{
			throw new NotImplementedException("StartHunt shouldn't be used!");
			return;
			if (isHuntOver)
			{
				//try
				{
					if (Players.Count() < 2 && !(TestMode && Players.Count() == 1))
					{
						Console.WriteLine("not enough players to start hunt, waiting till more join");
						SendChatMessage("hunt-error", "not enough players to start hunt, waiting till more join");

						while ((Players.Count() < 2) && !(TestMode && Players.Count() == 1))
						{
							SendChatMessage("hunt", "waiting for 2 people before we start", 105, 0, 225);
							SendToastNotif("Waiting for 2 players before the hunt starts.");

							Debug.WriteLine("^1 Not enough players to start^7");
							await Delay(15000);
						}
					}
					resetVars();

					TriggerClientEvent("sthv:sendChosenMap", playarea);
					//currentplayarea = playarea;
					int totalTimeSecs = timeInMinutes * 60;
					TriggerClientEvent("sth:starttimer", totalTimeSecs);
					Debug.WriteLine("runner id: " + runnerID);
					if (runnerID < 0) //picks random player from queue
					{
						foreach (Player p in Players) //hunters can spawn after opting
						{
							if (int.Parse(p.Handle) != runnerHandle)
							{
								p.TriggerEvent("AskRunnerOpt");
							}
						}
						await Delay(8000);
						foreach (Player p in NextRunnerQueue)
						{
							Debug.WriteLine($"^3players in list: {p.Name}^7");
						}
						Random rand = new Random();
						NextRunnerQueue.RemoveAll(item => !Players.Contains(item)); //needs to be tested
						if (NextRunnerQueue.Count > 0)
						{
							int randIndex = rand.Next(0, NextRunnerQueue.Count());
							try
							{
								runnerID = runnerHandle = int.Parse(NextRunnerQueue.ToArray()[randIndex].Handle); //runnerid and runnerhandle should be the same
							}
							catch (Exception ex)
							{
								Debug.WriteLine("exception caughtsds: " + ex);
							}
							runner = GetPlayerFromHandle(runnerHandle);

							Debug.WriteLine($"^4 playerchosen {randIndex} out of {NextRunnerQueue.Count() } options, handle: {runnerHandle}^7");
						}
						else
						{
							try
							{
								int randIndex = rand.Next(0, Players.Count());
								Debug.WriteLine("randIndex: " + randIndex);
								runnerID = runnerHandle = int.Parse(Players.ToArray()[randIndex].Handle);
								runner = GetPlayerFromHandle(runnerHandle);
								Debug.WriteLine($"^3Noone wanted to be runner so a random one was chosen. new runnerid: {runnerID}");
								SendChatMessage($"^1HUNT", $"Noone wanted to be runner so {runner.Name} was randomly chosen");
								SendToastNotif($"Noone wanted to be runner so {runner.Name} was randomly chosen", 3000);
							}
							catch (Exception ex)
							{
								Debug.WriteLine("^1E394 exception caught: ^7" + ex);
							}
						}
					}
					else
					{
						try
						{
							runnerHandle = runnerID;
							runner = GetPlayerFromHandle(runnerHandle);
						}
						catch (Exception ex)
						{
							Debug.WriteLine($"^1ERROR at assigning runner: {ex}");
						}
					}
					Debug.WriteLine("0b0");


					//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
					sthvLobbyManager.setAllActiveToWaiting();
					Debug.WriteLine("0b1");

					Debug.WriteLine("^1Invariant: all players should be inactive or waiting. ");

					//#remove inactive from list and handle else
					//foreach (var p in sthvLobbyManager.GetPlayersOfState(SthvPlayer.stateEnum.alive, SthvPlayer.stateEnum.inactive, SthvPlayer.stateEnum.ready, SthvPlayer.stateEnum.dead))
					//{
					//	Debug.WriteLine(p.player.Name + " is in state " + p.State.ToString());
					//}
					//Debug.WriteLine("0b2");


					Debug.WriteLine("\n\n^7");

					Debug.WriteLine($"^2runner handle is now: {runnerHandle}^7");
					TriggerClientEvent("sth:updateRunnerHandle", runnerHandle);

					//place blip

					SendChatMessage("^2HUNT", $"Runner is:{runner.Name}", 255, 255, 255);
					SendToastNotif($"Hunt starting with runner: {runner.Name}", 3000);

					await Delay(100);

					var runnerSthvPlayer = sthvLobbyManager.getPlayerByLicense(runner.getLicense());
					//sthvLobbyManager.SetPlayerTeam(runner, "runner");
					//sthvLobbyManager.MarkPlayerAlive(runner);

					NextRunnerQueue = new List<Player>(); //resets the list after runner spawns while hunters wait
														  //freezehunters, remveh, 


					//offer hunters to opt into runner ?
					TriggerClientEvent("removeveh");
					await Delay(500);
					//Players.First().TriggerEvent("sthv:spawnhuntercars"); doesn't work now

					runner.TriggerEvent("sthv:spectate", false);
					refreshscoreboard();
					var HavePlayersGottenGuns = false;



					await Delay(1000);
					for (int timeleft = totalTimeSecs; timeleft > 0; --timeleft) //hunt event loop
					{
						if (!isHuntOver)
						{
							if (!hasHuntStarted && (totalTimeSecs - timeleft > 15))
							{
								SendChatMessage("^5HUNT", "Hunt started!", 255, 255, 255);
								SendToastNotif("Spawning hunters! Weapons are given 30 seconds from now.", 10000);
								hasHuntStarted = true;
								foreach (Player p in Players)
								{
									AlivePlayerList.Add(p);
									if (int.Parse(p.Handle) != runnerHandle)
									{
										sthvLobbyManager.SetPlayerTeam(p, "hunter");

										if (IsDiscordServerOnline) discord.MovePlayerToVc(p.getDiscordId(), discord.fivemHunters);

									}
									else //if runner
									{
										if (IsDiscordServerOnline) discord.MovePlayerToVc(p.getDiscordId(), discord.fivemRunner);
									}
								}
							}
							if (!HavePlayersGottenGuns && (totalTimeSecs - timeleft > 60))
							{
								runner.TriggerEvent("sth:updateRunnerHandle", runnerHandle);

								TriggerClientEvent("sth:setguns", true);
								HavePlayersGottenGuns = true;
								SendChatMessage("^5HUNT", "You now have guns");
								SendToastNotif("You now have weapons!");

							}
							if ((timeleft % 10) == 0)
							{
								Debug.WriteLine($"timeleft: {timeleft}");
								TriggerClientEvent("sth:starttimer", timeleft);
							}
							await Delay(1000);
						}
						else
						{
							TriggerClientEvent("sth:setguns", false);
							SendChatMessage("^5HUNT", $"Runner {runner.Name} lost with {timeleft} seconds remaining");
							TriggerClientEvent("sth:starttimer", 0);
							break;
						}
					}
					onHuntOver();
				}
				//catch (Exception ex)
				//{
				//	Debug.WriteLine($"^2ERROR in StartHunt: {ex}^7");
				//	isHuntOver = true;
				//	onHuntOver();
				//}
			}
			else
			{
				Debug.WriteLine("^1ERORR: A HUNT IS ALREADY IN PROGRESS");
			}
		}
		async Task OntickCheckPlayers()
		{
			foreach (Player p in AlivePlayerList)
			{
				Debug.WriteLine($"alive players: {p.Name}");
			}
			await BaseScript.Delay(5000);
		}
		async void onHuntOver() //happens once on hunt over
		{
			if (IsDiscordServerOnline)
			{
				foreach (Player p in Players)
				{
					discord.MovePlayerToVc(p.getDiscordId(), discord.pcVoice);
				}
			}
			TriggerClientEvent("sthv:spectate", false);
			playersInHeliServerid = new List<int>();
			TriggerClientEvent("removeveh");
			runnerHandle = -1;
			TriggerClientEvent("sth:updateRunnerHandle", -1);
			TriggerClientEvent("sth:starttimer", 0);
			isHuntOver = true;
			refreshscoreboard();
			if (AutoHunt) Debug.WriteLine("hunt over. Starting another in 10 seconds.");
			await Delay(10000);
			if (AutoHunt)
			{
				StartHunt(25);
			}
			else
			{
				SendChatMessage("hunt", "autohunt is off");
				Debug.WriteLine("Autohunt is off.");
			}

		}
		void resetVars()
		{
			#region resetVariables
			runnerHandle = -1;
			runner = null;
			hasHuntStarted = false; //
			AlivePlayerList = new List<Player>();
			isHuntOver = false;
			#endregion
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
			if (KillerIndex < 0)
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
			TriggerClientEvent("sthv:refreshsb", JsonConvert.SerializeObject(playersInHeliServerid.ToArray()));
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
			if(serverid != null)
			{
				var targetPlayer = sthvLobbyManager.GetAllPlayers().First(p => p.player.Handle == serverid);
				if(targetPlayer != null)
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
		public void RegisterTickHandler( Func<Task> tick ) {
			Tick += tick;
		}
		public void DeregisterTickHandler( Func<Task> tick ) {
			Tick -= tick;
		}
		public void RegisterExport( string exportName, Delegate callback ) {
			Exports.Add( exportName, callback );
		}
		public dynamic GetExport( string resourceName ) {
			return Exports[resourceName];
		}
	}

}