﻿using System;
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
		bool isRunnerKilled = false;
		int totalTime;
		static public Player runner;
		public static bool hasHuntStarted = false; //
		List<Player> NextRunnerQueue = new List<Player>();
		List<Player> AlivePlayerList = new List<Player>();
		static public bool isHuntOver = true;
		public bool TestMode { get; set; } = false;
		int numberOfAvailableMaps = 1;
		public int currentplayarea { get; set; } = 1;
		public sthvDiscordController discord { get; }
		public static bool IsDiscordServerOnline { get; set; } = false;
		public bool AutoHunt { get; set; } = false;
		public static bool isInDevmode { get; } = (API.GetConvarInt("sthv_devmode", 0) != 0);


		public static List<int> playersInHeliServerid { get; set; } = new List<int>();

		[Command("test")]
		void onTest()
		{
			Debug.WriteLine("ran test");
		}
		public Server()
		{
			discord = new sthvDiscordController();

			

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
			API.RegisterCommand("toggleautohunt", new Action<int, List<object>, string>((src, args, raw) =>
			{
				AutoHunt = !AutoHunt;
				Debug.WriteLine("autohunt now " + AutoHunt);
			}), true);
			API.RegisterCommand("hunt", new Action<int, List<object>, string>((src, args, raw) =>
			{
				//int time = int.Parse(args[0].ToString());
				try
				{
					int huntplayarea = int.Parse(args[0].ToString());
					int huntrunnerindex = int.Parse(args[1].ToString());
					StartHunt(25, huntplayarea, huntrunnerindex);
				}
				catch (Exception ex)
				{
					StartHunt(25);
					Debug.WriteLine("^5Error in hunt command. Probably invalid parameters.^7");
				}

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
			EventHandlers["sth:NeedLicense"] += new Action<Player>(OnRequestedLicense); //kind of means when they load in
			EventHandlers["sth:sendserverkillerserverindex"] += new Action<Player, int>(KillfeedEventHandler);
			////EventHandlers["sth:testevent"] += new Action<Player>(OnTestEvent);
			//EventHandlers["sth:showMeOnMap"] += new Action<float, float, float>((float x, float y, float z) => { TriggerClientEvent("sth:sendShowOnMap", x, y, z); });

			EventHandlers["NumberOfAvailableMaps"] += new Action<int>(i => numberOfAvailableMaps = i);

			//test

		}

		[Tick]
		private async Task firstTick()
		{
			Tick -= firstTick;

			Debug.WriteLine("Starting server");
			await Delay(7000);

			StartHunt(25);
			//wait for atleast 1 player
			//while (Players.Count() < 1) await Delay(1000);

			//while (true)
			//{
			//	//$ select gamemode based on player count
			//	BaseGamemodeSthv gamemode = new sthvGamemodes.ClassicHunt();
			//	Debug.WriteLine("Instantiating gamemode " + gamemode.Name + ".");

			//	Debug.WriteLine("^4Registering gamemode script.^7");
			//	RegisterScript(gamemode);

			//	Debug.WriteLine("^4Testing method^7");
			//	await gamemode.Run();

			//	Debug.WriteLine("^4Unregistering gamemode script.^7");
			//	UnregisterScript(gamemode);
			//}

			Debug.WriteLine("Starting hunt");
			await discord.GetPlayersInChannel(discord.pcVoice);
			IsDiscordServerOnline = true;
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

							Debug.WriteLine("^8 Not enough players to start^7");
							await Delay(15000);
						}
					}
					resetVars();
					if (playarea < 0)
					{
						Random r = new Random();
						playarea = r.Next(0, numberOfAvailableMaps);
						Debug.WriteLine($"{numberOfAvailableMaps} maps available, {playarea} chosen");
					}
					TriggerClientEvent("sthv:sendChosenMap", playarea);
					currentplayarea = playarea;
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

					sthvLobbyManager.setAllActiveToWaiting();
					Debug.WriteLine("0b1");

					Debug.WriteLine("^1Invariant: all players should be inactive or waiting. ");

					//#remove inactive from list and handle else
					foreach (var p in sthvLobbyManager.GetPlayersOfState(SthvPlayer.stateEnum.alive, SthvPlayer.stateEnum.inactive, SthvPlayer.stateEnum.waiting, SthvPlayer.stateEnum.dead))
					{
						Debug.WriteLine(p.player.Name + " is in state " + p.State.ToString());
					}
					Debug.WriteLine("0b2");


					Debug.WriteLine("\n\n^7");

					Debug.WriteLine($"^2runner handle is now: {runnerHandle}^7");
					TriggerClientEvent("sth:updateRunnerHandle", runnerHandle);

					//place blip

					SendChatMessage("^2HUNT", $"Runner is:{runner.Name}", 255, 255, 255);
					SendToastNotif($"Hunt starting with runner: {runner.Name}", 3000);

					await Delay(100);
					runner.TriggerEvent("sth:spawn", (int)spawnType.runner);

					var runnerSthvPlayer = sthvLobbyManager.getPlayerByLicense(runner.getLicense());
					sthvLobbyManager.SetPlayerTeam(runner, "runner");
					sthvLobbyManager.MarkPlayerAlive(runner);

					NextRunnerQueue = new List<Player>(); //resets the list after runner spawns while hunters wait
														  //freezehunters, remveh, 


					//offer hunters to opt into runner 
					TriggerClientEvent("removeveh");
					await Delay(500);
					Players.First().TriggerEvent("sthv:spawnhuntercars");

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
										p.TriggerEvent("sth:spawn", (int)spawnType.hunter);
										sthvLobbyManager.MarkPlayerAlive(p);
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
			//TriggerClientEvent("sth:spawnall");
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
			isRunnerKilled = false;
			totalTime = 0;
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
		[EventHandler("sthv:requestspawn")]
		async void requestedSpawnHandler([FromSource] Player source) //0 idle, 1 spawn in game as hunter, 2 spawn dead as spectator
		{
			Debug.WriteLine("requested spawn from " + source.Name);
			if (isHuntOver)
			{
				source.TriggerEvent("sth:spawn", (int)spawnType.hunter); //2 means spawning as a hunter
				Debug.WriteLine("1");
				await Delay(1000);
				source.TriggerEvent("sth:setguns", true);
			}
			//else if (sthvLobbyManager.DeadPlayers.Contains(source.Identifiers["license"])) //if player had died was already dead
			//{
			//	source.TriggerEvent("sth:spawn", (int)spawnType.spectator); //3 means going into spectator mode (spawning and dying)
			//	Debug.WriteLine("2");
			//}
			else
			{
				source.TriggerEvent("sth:spawn", (int)spawnType.hunter);
				Debug.WriteLine($"spawned player {source.Name} as a hunter as a late joining hunter.");
				sthvLobbyManager.SetPlayerTeam(source, "hunter");
				sthvLobbyManager.MarkPlayerAlive(source);
			}
			refreshscoreboard();
			sthvLobbyManager.CheckAlivePlayers();
		}
		async void OnRequestedLicense([FromSource] Player source)        //send client their license 
		{
			var i = 1; //waits up to 3x2 seconds. Waits for firstick to check if discord server is online. 
			if (isInDevmode)
			{
				i = 3;
			}

			//retry loop incase of connection issues
			while (!IsDiscordServerOnline && i < 3)
			{
				Debug.WriteLine(IsDiscordServerOnline.ToString());
				await Delay(2000);
				i += 1;
			}
			Debug.WriteLine($"^3triggered:onRequestedLicense from: {source.Name}^7");
			string licenseId = source.Handle;
			var discordid = source.Identifiers["discord"];
			Debug.WriteLine(i.ToString());
			if (i < 3)
			{
				if ((discordid != null && discordid.Length > 4) || source.Identifiers["license"] == "705d1d418885080ecfd8aabb8710e624b6dc469e")
				{
					Debug.WriteLine(discordid);
					var isInGuild = await this.discord.GetIsPlayerInGuild(discordid);
					var vcMemberIds = await this.discord.GetPlayersInChannel(this.discord.pcVoice);
					var isInVc = vcMemberIds.Contains(source.Identifiers["discord"]);
					source.TriggerEvent("sth:returnlicense", licenseId, runnerHandle, true, isInGuild, isInVc, IsDiscordServerOnline);
				}
				else
				{
					source.TriggerEvent("sth:returnlicense", licenseId, runnerHandle, false, false, false, IsDiscordServerOnline); //p4-7 are discord related, used in client.cs
					Debug.WriteLine("player " + source.Name + " doesnt have discord smh");
				}
			}
			else
			{
				source.TriggerEvent("sth:returnlicense", licenseId, runnerHandle, false, false, false, false); //p4-7 are discord related, used in client.cs
			}
			refreshscoreboard();
			source.TriggerEvent("sth:updateRunnerHandle", runnerHandle);
			source.TriggerEvent("sthv:sendChosenMap", currentplayarea);

			//Debug.WriteLine("discord check happens here");
			//VerifyJustJoinedPlayer(source);
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
			var Killer = API.GetPlayerFromIndex(KillerIndex);

			//sthvLobbyManager.DeadPlayers.Add(killed.Identifiers["license"]);



			TriggerClientEvent("sthv:updateAlive", killed.Handle, false);

			//if there is no killer
			if (KillerIndex < 0)
			{
				SendChatMessage("KILLFEED", killed.Name + " died.");
				if (int.Parse(killed.Handle) == runnerHandle && !isHuntOver)
				{
					SendChatMessage("HUNT", "Runner " + runner.Name + " died.");
					SendToastNotif("Runner " + runner.Name + " died.");
					sthvLobbyManager.MarkPlayerDead(killed, killed.Name, killed.getLicense());
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


						if (runner != null)
						{
							Debug.WriteLine("runner: " + runner.Name);
							if (killed.Handle == runner.Handle && !isHuntOver) //if runner gets killed 
							{
								isRunnerKilled = true;
								isHuntOver = true;
								SendChatMessage("^5HUNT", $"{i.Name} killed runner: {runner.Name}");
							}
							else if (i.Handle == runner.Handle) //if runner gets a kill
							{
								SendChatMessage("^1HUNT", $"Runner {i.Name} killed hunter {killed.Name}");
							}
							else //teamkill during hunt
							{
								SendChatMessage("^1KILLFEED", $"{i.Name} teamkilled {killed.Name}");
								i.TriggerEvent("sthv:kill");
								SendChatMessage("", $"^5{i.Name} was killed by Karma and {killed.Name} respawned.");
								killed.TriggerEvent("sth:spawn", 2);
								killed.TriggerEvent("sth:setguns", true);

								//sthvLobbyManager.DeadPlayers.RemoveAll(p => p == killed.getLicense());
								sthvLobbyManager.getPlayerByLicense(killed.getLicense()).State = SthvPlayer.stateEnum.alive;
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
		public void RegisterEventHandler(string eventName, Delegate action)
		{
			EventHandlers[eventName] += action;
		}
		public static void SendChatMessage(string title, string message, int r = 255, int g = 255, int b = 255)
		{
			var msg = new Dictionary<string, object>
			{
				["color"] = new[] { r, g, b },
				["args"] = new[] { title, message }
			};
			TriggerClientEvent("chat:addMessage", msg);
			if (title == "^1KILLFEED") TriggerClientEvent("sthv:showToastNotification", msg, 1000);

		}
		public static void SendToastNotif(string message, int displayTimeInSeconds = 2000)
		{
			TriggerClientEvent("sthv:showToastNotification", message, displayTimeInSeconds);

		}
		private enum spawnType
		{
			idle = 0,
			runner = 1,
			hunter = 2,
			spectator = 3
		}
	}

}