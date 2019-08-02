using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX;
using CitizenFX.Core;
using CitizenFX.Core.Native;



namespace sthvClient
{
	public class client : BaseScript
	{
		bool isRunner { get; set; }
		int License { get; set; }
		int RunnerLicense;
		int respawnCount = 0;
		
		public client()
		{
			//test 

			var playArea = new sthvClient.sthvPlayArea();
			var rules = new sthvClient.sthvRules();

			Tick += rules.AutoBrakeLight;
			Tick += playArea.OnTickPlayArea;
			Tick += rules.isZPressed; //for big map toggle


			TriggerServerEvent("sth:NeedLicense");

			EventHandlers["onClientMapStart"] += new Action<string>(onPlayerLoaded); // event from mapmanager_cliend.lua line 47
			EventHandlers["sth:spawnall"] += new Action(Respawn);
			EventHandlers["sth:resetrespawncounter"] += new Action(ResetRespawnCounter);
			EventHandlers["sth:returnlicense"] += new Action<int,int>(ReceivedLicense); //gets license from server
			EventHandlers["sth:updateRunnerHandle"] += new Action<int>(RunnerHandleUpdate);
			//EventHandlers["sth:licenseStored"] += new Action(Respawn); //when client knows license because client must know license before spawning
			EventHandlers["playerSpawned"] += new Action(onPlayerSpawned); //basically just give guns 
			//EventHandlers["basescript:"]
			#region commands
			API.RegisterCommand("license", new Action<int, List<object>, string>((src, args, raw) =>
			{
				Debug.WriteLine(License.ToString());
			}), false);
			API.RegisterCommand("spawn", new Action<int, List<object>, string>((src, args, raw) =>
			{
				Respawn();
				
			}), false);
			API.RegisterCommand("checkrunner", new Action<int, List<object>, string>((src, args, raw) =>
			{
				TriggerServerEvent("NeedLicense");
				if (RunnerLicense.Equals(License))
				{
					isRunner = true;
				}
				SendChatMessage("runner:", $"{isRunner}", 255, 255, 200);
				Debug.WriteLine($"RUNNER:^2{isRunner}\nyou:{License}\nrunner: {RunnerLicense}");
				
			}), false);
			API.RegisterCommand("giveguns", new Action<int, List<object>, string>((src, args, raw) =>
			{	if (respawnCount < 99) //first spawn is the load in
				{
					Game.PlayerPed.Weapons.Give(WeaponHash.CombatPistol, 225, false, true);
					Game.PlayerPed.Weapons.Give(WeaponHash.PumpShotgun, 225, false, true);
					Game.PlayerPed.Weapons.Give(WeaponHash.Flashlight, 2, false, true);
					Game.PlayerPed.Weapons.Give(WeaponHash.Flare, 225, false, true);

					if (isRunner)
					{
						Debug.WriteLine("gaverunnerguns");
						Game.PlayerPed.Weapons.Give(WeaponHash.BullpupRifleMk2, 1000, false, true);

					}
				}
				else
				{
					return;
				}
				
			}), false);
			API.RegisterCommand("removeguns", new Action<int, List<object>, string>((src, args, raw) =>
			{
				Game.PlayerPed.Weapons.RemoveAll();

			}), false);
			#endregion
		}

		void onPlayerLoaded(string res) // res from mapmanager_cliend.lua line 47, stores name of map resource
		{
			TriggerServerEvent("sth:NeedLicense");  //asks server for license, ends
			Respawn();
		}


		void ReceivedLicense(int myLicense,int runnerLicense)	//gets license from server
		{
			Debug.WriteLine($"^2license recieved, mine: {myLicense} runner: {runnerLicense}^7");
			License = myLicense;
			RunnerLicense = runnerLicense;
			TriggerEvent("sth:licenseStored"); //when license stored, to prevent spawning without client storing license
		}
		void RunnerHandleUpdate(int newRunnerHandle)
		{
			RunnerLicense = newRunnerHandle;
		}
		void ResetRespawnCounter()
		{
			respawnCount = 0;
		}
		void Respawn()
		{
			if (isRunner)
			{
				sthvClient.Spawn.SpawnPlayer("A_M_Y_BEVHILLS_01", -181f, -210f, 47f, 0f);
			}
			else
			{
				sthvClient.Spawn.SpawnPlayer("S_M_Y_MARINE_01", -181f, -210f, 47f, 0f);
			}
			respawnCount++;
		}
		void onPlayerSpawned()
		{
			API.ExecuteCommand("giveguns");
		}

		public void RegisterEventHandler(string eventName, Delegate action)
		{
			EventHandlers[eventName] += action;
		}
		public static void SendChatMessage(string title, string message, int r, int g, int b)
		{
			var msg = new Dictionary<string, object>
			{
				["color"] = new[] { r, g, b },
				["args"] = new[] { title, message }
			};
			TriggerEvent("chat:addMessage", msg);
		}

	}
}
