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
		bool runnerKilledSelfOrAi = false;
		Player runner;
		bool hasHuntStarted = false; 

		public server()
		{
			var stuffythings = new Identifiers();
			//test 
			EventHandlers["sth:sendServerDebug"] += new Action<string>((string info) => {Debug.WriteLine(info); });

			API.RegisterCommand("freeze", new Action<int, List<object>, string>((src, args, raw) =>
			{
				TriggerClientEvent("sth:freezePlayer", true);
			}), true);
			API.RegisterCommand("unfreeze", new Action<int, List<object>, string>((src, args, raw) =>
			{
				TriggerClientEvent("sth:freezePlayer", false);
			}), true);
			API.RegisterCommand("resetspawns", new Action<int, List<object>, string>((src, args, raw) =>
			{
				TriggerClientEvent("sth:resetrespawncounter");

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
						
						Debug.WriteLine($"hunt total time in minutes: {totalTime}");
						Debug.WriteLine("freeze hunters");
						TriggerClientEvent("sth:spawnall");

						await BaseScript.Delay(3);
						TriggerClientEvent("sth:freezePlayer", true);


						Debug.WriteLine($"Hunt Started with Runner: {runner.Name}");    //hunt start
						for (int i = 0; i < totalTime; i++)

						{
							int timeLeft = totalTime - i;

							if (isRunnerKilled || runnerKilledSelfOrAi)
							{
								TriggerClientEvent("sendChatMessageToAll", "^5HUNT", $"Runner {runner.Name} lost with {timeLeft} minutes left. Hunters win!");
								break;
							}
							if (!isRunnerKilled && !runnerKilledSelfOrAi)
							{
								Debug.WriteLine($"{timeLeft} minutes remaining");
								if ((timeLeft % 10 == 0) || (timeLeft == totalTime) || (timeLeft == 5) || (timeLeft == 3) || (timeLeft == 2) || (timeLeft == 1))
								{
									TriggerClientEvent("sendChatMessageToAll", "^5HUNT", $"Time Left: {timeLeft}^7");
								}
								if ((timeLeft < totalTime - 1)&&(hasHuntStarted == false))
								{
									TriggerClientEvent("sth:freezePlayer", false);
									TriggerClientEvent("sendChatMessageToAll", "^5HUNT", $"Hunt Started! Time left: {timeLeft}, runner: {runner.Name}^7");
									hasHuntStarted = true;
								}
								await BaseScript.Delay(10000);
							}
							else if (isRunnerKilled || runnerKilledSelfOrAi)
							{
								TriggerClientEvent("sendChatMessageToAll", "^5HUNT", $"Runner {runner.Name} lost with {timeLeft} minutes left");
							}
							else if ((!isRunnerKilled) && (!runnerKilledSelfOrAi))
							{
								TriggerClientEvent("sendChatMessageToAll", "^5HUNT", $"Runner {runner.Name} won with {timeLeft} minutes left");
							}
							
						}
					TriggerClientEvent("sth:freezePlayer", false, true); //after the for loop, so after hunt is over
					}
					else { Debug.WriteLine("need valid arguments for starthunt"); }
				}
				catch(Exception ex) { Debug.WriteLine($"^5ERROR: {ex}"); }
			}), true);
			API.RegisterCommand("assignrunner", new Action<int, List<object>, string>((source, args, raw) =>
			{
				try
				{
					isRunnerKilled = false;
					runnerKilledSelfOrAi = false;

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

			EventHandlers["sth:NeedLicense"] += new Action<Player>(OnRequestedLicense);
			EventHandlers["sth:sendserverkillerserverindex"] += new Action<Player, int>(KillfeedStuff);
			EventHandlers["sth:testevent"] += new Action<Player>(OnTestEvent);
			EventHandlers["sth:showMeOnMap"] += new Action<float, float, float>((float x, float y, float z) => { TriggerClientEvent("sth:sendShowOnMap", x, y, z); });
			EventHandlers["sth:killedSelfOrAi"] += new Action(KilledBySelfOrAi); //on suicide or killed by AI 
			EventHandlers["testevent"] += new Action<Player>((Player player) => { Debug.WriteLine($"player:{player.Name}"); });
		}
		void KilledBySelfOrAi()
		{
			if (!isRunnerKilled)
			{
				runnerKilledSelfOrAi = true;
				isRunnerKilled = true;

				//TriggerClientEvent("sendChatMessageToAll", "^6KILLFEED", $"{runner.Name} killed himself lol\n hunt is over");

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
							TriggerClientEvent("sendChatMessageToAll", "^5HUNT", $"Runner {runner.Name} was killed by hunter {i.Name}");
						}
						if (i.Handle == runner.Handle) //if runner gets a kill
						{
							TriggerClientEvent("sendChatMessageToAll", "^5HUNT", $"Runner {i.Name} killed hunter {killed.Name}");
						}
						else
						{
							TriggerClientEvent("sendChatMessageToAll", "^6KILLFEED", $"{i.Name} killed {killed.Name}");
						}
					}

				}
			}
		}
	}

}
