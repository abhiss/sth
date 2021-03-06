﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.IO;
using Newtonsoft.Json;
using System.Web.Routing;
using System.ComponentModel;

namespace sthvServer
{

	class server : BaseScript
	{
		int runnerHandle = -1;
		bool isRunnerKilled = false;
		int totalTime;
		static public Player runner;
		public static bool hasHuntStarted = false; //
		List<Player> NextRunnerQueue = new List<Player>();
		List<Player> AlivePlayerList = new List<Player>();
		static public bool isHuntOver = true;
		bool isEveryoneInvincible = true;
		public bool HavePlayersGottenGuns { get; set; }
		public bool TestMode { get; set; } = false;
		int numberOfAvailableMaps = 1;
		public int currentplayarea { get; set; }


		public bool AutoHunt { get; set; } = true;
		public server()
		{

			var stuffythings = new Identifiers();
			//test 

			EventHandlers["sth:sendServerDebug"] += new Action<string>((string info) => { Debug.WriteLine(info); });

			//API.RegisterCommand("test", new Action<int, List<object>, string>((src, args, raw) =>
			//{
			//	var arguments = new Dictionary<string, object> {
			//		{"url", "http://localhost:3000"},
			//		{"method", "POST"},
			//		{"data", "YAAA WE PASSED THE DATA"},
			//		{"headers", new Dictionary<string, string> {
			//		{"Content-Type", "application/json"}}}};

			//	string json = JsonConvert.SerializeObject(arguments);
			//	var i = API.PerformHttpRequestInternal(json, json.Length);
			//	Debug.WriteLine(i.ToString());
			//}), true);

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
			API.RegisterCommand("freeze", new Action<int, List<object>, string>((src, args, raw) =>
			{
				TriggerClientEvent("sth:freezePlayer", true);
			}), true);
			API.RegisterCommand("unfreeze", new Action<int, List<object>, string>((src, args, raw) =>
			{
				TriggerClientEvent("sth:freezePlayer", false);
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
					Debug.WriteLine("Error in hunt command. Probably invalid parameters.");
				}
				
			}), true);

			API.RegisterCommand("spawnall", new Action<int, List<object>, string>((src, args, raw) =>
			{
				TriggerClientEvent("sth:spawnall");
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

							foreach (var i in coordlist)
							{
								file.WriteLine($"new Vector4({i.X}f, {i.Y}f, {i.Z}f, {i.W}f),");
								Console.WriteLine($"new Vector4({i.X}f, {i.Y}f, {i.Z}f, {i.W}f),");

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
			EventHandlers["sthv:playerJustDead"] += new Action<Player>(OnPlayerDead);

			EventHandlers["sth:NeedLicense"] += new Action<Player>(OnRequestedLicense); //kind of means when they load in
			EventHandlers["sth:sendserverkillerserverindex"] += new Action<Player, int>(KillfeedStuff);
			////EventHandlers["sth:testevent"] += new Action<Player>(OnTestEvent);
			//EventHandlers["sth:showMeOnMap"] += new Action<float, float, float>((float x, float y, float z) => { TriggerClientEvent("sth:sendShowOnMap", x, y, z); });
			EventHandlers["sthv:opttorun"] += new Action<Player>(addToRunnerList);

			EventHandlers["NumberOfAvailableMaps"] += new Action<int>(i => numberOfAvailableMaps = i);


			API.SetHttpHandler(new Action<dynamic, dynamic>(async (req, res) =>
			{
		
			}));
		}
#if d

#endif
		private async Task<Stream> GetBodyStream(dynamic req)
		{
			var tcs = new TaskCompletionSource<byte[]>();

			req.setDataHandler(new Action<byte[]>(data =>
			{
				tcs.SetResult(data);
			}), "binary");

			var bytes = await tcs.Task;
			return new MemoryStream(bytes);
		}

		void addToRunnerList([FromSource]Player source) {
			NextRunnerQueue.Add(source);
		}
		void OnPlayerDead([FromSource]Player source)	//not good
		{
			if (true)//AlivePlayerList.Contains(source))
			{
				if (runner != null && source.Handle == runner.Handle)
				{
					SendChatMessage("^5HUNT", $"Runner {runner.Name} died, hunt over^7");
					isHuntOver = true;
				}
			}
		}
		void resetVars()
		{
#region resetVariables
			runnerHandle = 0;
			isRunnerKilled = false;
			totalTime = 0;
			runner = null;
			hasHuntStarted = false; //
			AlivePlayerList = new List<Player>();
			isHuntOver = false;
			isEveryoneInvincible = true;
			sthvLobbyManager.DeadPlayers = new List<Player>();
			//NextRunnerQueue = new List<Player>(); doesnt reset till runner is chosen
#endregion
		}
		/// <summary>
		/// playerid defaults to -1 which randomly chooses a player to run
		/// </summary>
		/// <param name="timeInMinutes"></param>
		/// <param name="playarea"></param>
		/// <param name="playerId"></param>
		async void StartHunt(int timeInMinutes, int playarea = -1, int runnerID = -1)
		{
			if (isHuntOver)
			{
				try
				{
					if (Players.Count() < 2 && !TestMode)
					{
						Console.WriteLine("not enough players to start hunt, waiting till more join");
						SendChatMessage("hunt-error", "not enough players to start hunt, waiting till more join");
						while ((Players.Count() < 2) && !TestMode)
						{
							SendChatMessage("hunt", "waiting for 2 people before we start", 105, 0, 225);
							Debug.WriteLine("^8 Not enough players to start^7");
							await Delay(20000);
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
					if (runnerID < 0) //picks random player from queue
					{
						foreach (Player p in NextRunnerQueue)
						{
							Debug.WriteLine($"^3players in list: {p.Name}^7");
						}
						Random rand = new Random();
						NextRunnerQueue.RemoveAll(item => !Players.Contains(item)); //needs to be tested
						if ((NextRunnerQueue.Count > 0))
						{						
							int randIndex = rand.Next(0, NextRunnerQueue.Count());
							runnerID = runnerHandle = int.Parse(NextRunnerQueue.ToArray()[randIndex].Handle); //runnerid and runnerhandle should be the same
							
							runner = GetPlayerFromHandle(runnerHandle);
							
							Debug.WriteLine($"^4 playerchosen {randIndex} out of {NextRunnerQueue.Count() } options, handle: {runnerHandle}^7");
						}
						else {
							int randIndex = rand.Next(0, Players.Count());
							runnerID = runnerHandle = int.Parse(Players.ToArray()[randIndex].Handle);
							runner = GetPlayerFromHandle(runnerHandle);
							Debug.WriteLine($"^3Noone wanted to be runner so a random one was chosen. new runnerid: {runnerID}");
							SendChatMessage($"^1HUNT", $"Noone wanted to be runner so {runner.Name} was randomly chosen");
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
					Debug.WriteLine($"^2runner handle is now: {runnerHandle}^7");
					TriggerClientEvent("sth:updateRunnerHandle", runnerHandle);

					//place blip

					SendChatMessage("^2HUNT", $"Hunt starting in 30 seconds with runner:{runner.Name}", 255, 255, 255);
					await Delay(100);
					runner.TriggerEvent("sth:spawn", 1);
					//runner.TriggerEvent("sthv:nuifocus", false);
					//Debug.WriteLine("spawned runner and nuifocus false");
					TriggerClientEvent("sth:invincible", true);
					NextRunnerQueue = new List<Player>(); //resets the list after runner spawns while hunters wait
														  //freezehunters, remveh, 
					foreach (Player p in Players) //hunters can spawn after opting
					{
						if (int.Parse(p.Handle) != runnerHandle)
						{
							p.TriggerEvent("AskRunnerOpt");
							p.TriggerEvent("sth:invincible", true);

						}
					}
					//offer hunters to opt into runner 
					TriggerClientEvent("removeveh");
					await Delay(500);
					Players.First().TriggerEvent("sthv:spawnhuntercars");
					foreach(Player p in Players)
					{
						if(p != runner)
						{
							p.TriggerEvent("sth:freezePlayer", true);
						}
					}
					runner.TriggerEvent("sthv:spectate", false);
					TriggerClientEvent("sthv:refreshsb");

					foreach (Player p in Players)
					{
						AlivePlayerList.Add(p);
					}

					for (int timeleft = totalTimeSecs; timeleft > 0; --timeleft) //hunt event loop
					{
						if (!isHuntOver)
						{
							if (!hasHuntStarted && (totalTimeSecs - timeleft > 30))
							{
								SendChatMessage("^5HUNT", "Hunt started!", 255, 255, 255);
							
								//TriggerClientEvent("sthv:nuifocus", false);
								TriggerClientEvent("sth:freezePlayer", false);
								hasHuntStarted = true;
							}
							if (isEveryoneInvincible && (totalTimeSecs - timeleft > 60))
							{
								TriggerClientEvent("sth:invincible", false);
								TriggerClientEvent("sth:setguns", true);
								HavePlayersGottenGuns = true;
								SendChatMessage("^5HUNT", "You now have guns");
								isEveryoneInvincible = false;
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
						}
					}
					onHuntOver();
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"^2ERROR in StartHunt: {ex}^7");
					isHuntOver = true;
				}
			}
			else
			{
				Debug.WriteLine("^1ERORR: A HUNT IS ALREADY IN PROGRESS");
			}
		}
		async Task OntickCheckPlayers()
		{ 
			foreach(Player p in AlivePlayerList)
			{
				Debug.WriteLine($"alive players: {p.Name}");
			}
			await BaseScript.Delay(5000);
		}
		async void onHuntOver()
		{
			TriggerClientEvent("sth:starttimer", 0); //end timer
			TriggerClientEvent("sthv:spectate", false);
			TriggerClientEvent("sth:spawnall");
			TriggerClientEvent("removeveh");
			TriggerClientEvent("sth:freezePlayer", true);
			runnerHandle = -1;
			TriggerClientEvent("sth:updateRunnerHandle", runnerHandle);

			TriggerClientEvent("sth:invincible", true);
			isEveryoneInvincible = true;
			isHuntOver = true;

			await Delay(10000);
			if (AutoHunt)
			{
				StartHunt(25);
			}
			else
			{
				SendChatMessage("hunt", "autohunt is off");
			}

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
				foreach(Player p in Players)
				{
					if (int.Parse(p.Handle) == playerId)
					{
						return p;
					}
				}
				return null;
			}
			catch(Exception ex)
			{
				Debug.Write($"^3ERROR THROWN IN GetPlayerFromId: {ex}");
				return null;
			}
		}


		void OnTestEvent([FromSource]Player source)
		{
			Debug.WriteLine($"Test Event Triggered by {source.Name}");
		}
		void OnRequestedLicense([FromSource] Player source)        //send client their license 
		{
			Debug.WriteLine($"^3triggered:onRequestedLicense from: {source.Name}^7");
			string licenseId = source.Handle;
			TriggerClientEvent(source, "sth:returnlicense", licenseId, runnerHandle);
			TriggerClientEvent("sthv:refreshsb");

			source.TriggerEvent("sth:updateRunnerHandle", runnerHandle);
			source.TriggerEvent("sthv:sendChosenMap", currentplayarea);

			Debug.WriteLine("discord check happens here");
			VerifyJustJoinedPlayer(source);
		}
		void VerifyJustJoinedPlayer(Player player)
		{
			sthvDiscordController.VerifyDiscord(player.Identifiers["discord"]);
		}
		void KillfeedStuff([FromSource]Player killed, int KillerIndex)
		{
			Debug.WriteLine($"Log:killfeedstuff killed.Name: {killed.Name} KillerIndex: {KillerIndex}");
			var Killer = API.GetPlayerFromIndex(KillerIndex);
			foreach (Player i in Players)
			{
				Debug.WriteLine($"playerhandles = {i.Handle}");
				if (int.Parse(i.Handle) == KillerIndex)
				{
					Debug.WriteLine($"killerhandle ={i.Handle}\nkillername = {i.Name}");
					Debug.WriteLine($"{i.Name} killed {killed.Name}");
					if (runner != null)
					{
						Debug.WriteLine("runner: " + runner.Name);
						if (killed.Handle == runner.Handle) //if runner gets killed 
						{
							isRunnerKilled = true;
							isHuntOver = true;
							SendChatMessage("^5HUNT", $"{i.Name} killed runner: {runner.Name}");
						}
						else if (i.Handle == runner.Handle) //if runner gets a kill
						{

							SendChatMessage("^1HUNT", $"Runner {i.Name} killed hunter {killed.Name}");
						}
						else
						{
							SendChatMessage("^1KILLFEED", $"{i.Name} teamkilled {killed.Name}");
							i.TriggerEvent("sthv:kill");
							SendChatMessage("", $"^5{i.Name} was killed by Karma");
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
		}


		public static void SendChatMessage(string title, string message, int r = 255, int g = 255, int b = 255)
		{
			var msg = new Dictionary<string, object>
			{
				["color"] = new[] { r, g, b },
				["args"] = new[] { title, message }
			};
			TriggerClientEvent("chat:addMessage", msg);
		}
	}

}