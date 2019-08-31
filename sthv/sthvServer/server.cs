using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace sthvServer
{

	class server : BaseScript
	{

		int runnerHandle = 0;
		bool isRunnerKilled = false;
		int totalTime;
		public Player runner;
		bool hasHuntStarted = false; //
		List<Player> NextRunnerQueue = new List<Player>();
		List<Player> AlivePlayerList = new List<Player>();
		bool isHuntOver = true;
		bool isEveryoneInvincible = true;

		public server()
		{
			var stuffythings = new Identifiers();
			//test 
			EventHandlers["sth:sendServerDebug"] += new Action<string>((string info) => { Debug.WriteLine(info); });

			API.RegisterCommand("spawn", new Action<int, List<object>, string>((src, args, raw) =>
			{
				try
				{
					int playerHandle = int.Parse(args[0].ToString());
					Player _playerToSpawn = GetPlayerFromHandle(playerHandle);
					_playerToSpawn.TriggerEvent("sth:spawnall", true);

				}
				catch(Exception ex)
				{
					Debug.WriteLine($"^1Error (probably passed invalid arguments): {ex}^7");
				}
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

			API.RegisterCommand("resetall", new Action<int, List<object>, string>((src, args, raw) =>
			{
				TriggerClientEvent("onClientMapStart");
			}), true);
			API.RegisterCommand("hunt", new Action<int, List<object>, string>((src, args, raw) =>
			{
				StartHunt(25);
			}), true);

			API.RegisterCommand("spawnall", new Action<int, List<object>, string>((src, args, raw) =>
			{
				TriggerClientEvent("sth:spawnall");

			}), true);

			API.RegisterCommand("starthunt", new Action<int, List<object>, string>(async (src, args, raw) =>
			{
				try
				{
					var argsList = args.Select((object o) => o.ToString()).ToList();
					if (argsList.Any())
					{

						totalTime = int.Parse(argsList[1]);
						int totalTimeInSeconds = totalTime * 60;
						TriggerClientEvent("sth:starttimer", totalTimeInSeconds);

						int playArea = int.Parse(argsList[2]);
						TriggerClientEvent("sth:setPlayArea", playArea);

						#region assignrunner
						runnerHandle = int.Parse(argsList[0]);

						Debug.WriteLine($"runner handle is now: {runnerHandle}");
						TriggerClientEvent("sth:updateRunnerHandle", runnerHandle);

						foreach (Player player in Players)
						{
							if (int.Parse(player.Handle) == runnerHandle)
							{
								runner = player;
							}
						}
						#endregion
						TriggerClientEvent("sth:spawnall");
						Debug.WriteLine($"hunt total time in minutes: {totalTime}");
						Debug.WriteLine("freeze hunters");

						await Delay(1000);

						Debug.WriteLine($"Hunt will start in 1 minute! Runner: {runner.Name}");
						TriggerClientEvent("sth:freezePlayer", true); //hunt start
						for (int i = 0; i < totalTime; i++)

						{
							int timeLeft = totalTime - i;

							if (isRunnerKilled )
							{
								TriggerClientEvent("sendChatMessageToAll", "^5HUNT", $"Runner {runner.Name} lost with {timeLeft} minutes left. Hunters win!");
								break;
							}
							if (!isRunnerKilled)
							{
								Debug.WriteLine($"{timeLeft} minutes remaining"); //just the timer
								if ((timeLeft % 10 == 0) || (timeLeft == totalTime) || (timeLeft == 5) || (timeLeft == 3) || (timeLeft == 2) || (timeLeft == 1))
								{
									TriggerClientEvent("sendChatMessageToAll", "^5HUNT", $"^4Hunt Started! ^7Time Left: {timeLeft}^7"); //sendchatmessagetoall doesnt work anymore, replaced with local TriggerChatEvent

								}
								if ((timeLeft < totalTime) && (hasHuntStarted == false)) //after hunters can go
								{
									TriggerClientEvent("sth:freezePlayer", false);
									TriggerClientEvent("sendChatMessageToAll", "^5HUNT", $"^4Hunt Started! Time left: {timeLeft}, Runner: {runner.Name}^7");
									hasHuntStarted = true;
								}
								await BaseScript.Delay(60000);
							}
							else if (isRunnerKilled)
							{
								TriggerClientEvent("sendChatMessageToAll", "^5HUNT", $"Runner {runner.Name} lost with {timeLeft} minutes left");
								TriggerClientEvent("sth:starttimer", 0); //end timer
							}
							else if (!isRunnerKilled)
							{
								TriggerClientEvent("sendChatMessageToAll", "^5HUNT", $"Runner {runner.Name} won with {timeLeft} minutes left");
								TriggerClientEvent("sth:starttimer", 0); //end  timer
							}

						}
						TriggerClientEvent("sth:freezePlayer", true); //after the for loop, so after hunt is over
					}
					else { Debug.WriteLine("need valid arguments for starthunt"); }
				}
				catch (Exception ex) { Debug.WriteLine($"^5ERROR: {ex}"); }
			}), true);

			#region obsolete
			API.RegisterCommand("assignrunner", new Action<int, List<object>, string>((source, args, raw) =>
			{
				try
				{
					isRunnerKilled = false;

					var argsList = args.Select((object o) => o.ToString()).ToList();
					if (argsList.Any())
					{
						runnerHandle = int.Parse(argsList[0]);
						Debug.WriteLine($"runner handle is now: {runnerHandle}");
						TriggerClientEvent("sth:updateRunnerHandle", runnerHandle);
						foreach (Player player in Players)
						{
							if (int.Parse(player.Handle) == runnerHandle)
							{
								runner = player;
							}
						}
					}
					else
					{
						Debug.WriteLine("an error occured in assignhunter");
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"^4assignhunter broke with exception: {ex}^7");
				}

			}), true);
			#endregion //assignhunter

			//new hunt related
			EventHandlers["sthv:playerIsDead"] += new Action<Player>(OnPlayerDead);

			EventHandlers["sth:NeedLicense"] += new Action<Player>(OnRequestedLicense);
			EventHandlers["sth:sendserverkillerserverindex"] += new Action<Player, int>(KillfeedStuff);
			////EventHandlers["sth:testevent"] += new Action<Player>(OnTestEvent);
			//EventHandlers["sth:showMeOnMap"] += new Action<float, float, float>((float x, float y, float z) => { TriggerClientEvent("sth:sendShowOnMap", x, y, z); });
			EventHandlers["sthv:opttorun"] += new Action<Player>(addToRunnerList);
		}
		void addToRunnerList([FromSource]Player source) {
			NextRunnerQueue.Add(source);
		}
		void OnPlayerDead([FromSource]Player source)
		{
			if (true)//AlivePlayerList.Contains(source))
			{
				AlivePlayerList.Remove(source);
				Debug.WriteLine($"^2{source.Name} removed from alive players list");
				Debug.WriteLine($"{AlivePlayerList.Count} alive players remain");
				
				if (runner != null && source.Handle == runner.Handle)
				{
					SendChatMessage("^5HUNT", $"Runner {runner.Name} died, hunt over");

					isHuntOver = true;
				}
				else
				{
					SendChatMessage("^5HUNT", $"{source.Name} is dead, {AlivePlayerList.Count} people remain");
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
			
			//NextRunnerQueue = new List<Player>(); doesnt reset till runner is chosen
			#endregion
		}
		/// <summary>
		/// playerid defaults to -1 which randomly chooses a player to run
		/// </summary>
		/// <param name="timeInMinutes"></param>
		/// <param name="playarea"></param>
		/// <param name="playerId"></param>
		async void StartHunt(int timeInMinutes, int playarea = 0, int runnerID = -1)
		{
			if (isHuntOver)
			{
				try
				{

					resetVars();

					int totalTimeSecs = timeInMinutes * 60;
					TriggerClientEvent("sth:starttimer", totalTimeSecs);
					if (runnerID < 0) //picks random player from queue
					{

						foreach (Player p in NextRunnerQueue)
						{
							Debug.WriteLine($"^3players in list: {p.Name}^7");
						}
						Random rand = new Random();
						if ((NextRunnerQueue.Count > 0))
						{
							foreach(Player p in NextRunnerQueue)
							{
								if (!Players.Contains(p))
								{
									NextRunnerQueue.Remove(p);
								}
							}
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

					NextRunnerQueue = new List<Player>(); //resets the list after runner spawns while hunters weight
														  //freezehunters, remveh, 
					foreach (Player p in Players) //hunters can spawn after opting
					{
						if (int.Parse(p.Handle) != runnerHandle)
						{
							p.TriggerEvent("AskRunnerOpt");
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

					foreach (Player p in Players)
					{
						AlivePlayerList.Add(p);
					}

					for (int timeleft = totalTimeSecs; timeleft > 0; --timeleft) //game event loop
					{
						if (!isHuntOver)
						{
							if (!hasHuntStarted && (totalTimeSecs - timeleft > 30))
							{
								SendChatMessage("^5HUNT", "Hunt started!", 255, 255, 255);
								//spawn hunter cars, spawn hunters
								//TriggerClientEvent("sthv:spawnhuntercars");
								TriggerClientEvent("sthv:nuifocus", false);
								TriggerClientEvent("sth:freezePlayer", false);
								hasHuntStarted = true;
							}
							if (isEveryoneInvincible && (totalTimeSecs - timeleft > 60))
							{
								//give guns, invincible false
								TriggerClientEvent("sth:invincible", false);
								TriggerClientEvent("sth:giveguns", true);
								SendChatMessage("^5HUNT", "You now have guns");
								isEveryoneInvincible = false;
							}
							if ((timeleft % 10) == 0)
							{
								Debug.WriteLine($"timeleft: {timeleft}");
								TriggerClientEvent("sth:starttimer", timeleft);
								Debug.WriteLine($"{AlivePlayerList.Count} people still alive");

							}
							await Delay(1000);
						}
						else
						{
							TriggerClientEvent("sth:giveguns", false);
							SendChatMessage("^5HUNT", $"Runner {runner.Name} lost with {timeleft} seconds remaining");
							TriggerClientEvent("sth:starttimer", 0);
							break;
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
		void onHuntOver()
		{
			TriggerClientEvent("sth:spawnall");
			TriggerClientEvent("removeveh");
			TriggerClientEvent("sth:freezePlayer", true);
			isEveryoneInvincible = true;
			isHuntOver = true;
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
		}
		void KillfeedStuff([FromSource]Player killed, int KillerIndex)
		{
			Debug.WriteLine($"Log:killfeedstuff killed.Name: {killed.Name} KillerIndex: {KillerIndex}");
			var Killer = API.GetPlayerFromIndex(KillerIndex);
			Debug.WriteLine($"^2Killer: {Killer}^7 KillerIndex: {KillerIndex}");
			foreach (Player i in Players)
			{

				Debug.WriteLine($"playerhandles = {i.Handle}");
				if (int.Parse(i.Handle) == KillerIndex)
				{
					Debug.WriteLine($"killerhandle ={i.Handle}\nkillername = {i.Name}");
					Debug.WriteLine($"{i.Name} killed {killed.Name}");

					if (runner != null)
					{
						if (killed.Handle == runner.Handle) //if runner gets killed 
						{

							isRunnerKilled = true;
							isHuntOver = true;
							SendChatMessage("^5HUNT", $"Runner {runner.Name} was killed by hunter {i.Name}");
						}
						if (i.Handle == runner.Handle) //if runner gets a kill
						{

							SendChatMessage("^1HUNT", $"Runner {i.Name} killed hunter {killed.Name}");
						}
					}
					else
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