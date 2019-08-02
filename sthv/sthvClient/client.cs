using System;
using System.Collections.Generic;
using System.Dynamic;
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
									  //Killfeed stuff:
			EventHandlers["baseevents:onPlayerKilled"] += new Action<int, ExpandoObject>(OnPlayerKilled);
			EventHandlers["baseevents:onPlayerDied"] += new Action( () => { Debug.WriteLine("onplayerdied"); }); // event from mapmanager_cliend.lua line 47
			EventHandlers["baseevents:onPlayerWasted"] += new Action( () => { Debug.WriteLine("onplayerwasted"); }); // event from mapmanager_cliend.lua line 47
			EventHandlers["sth:sendKillFeed"] += new Action<string, string>((string killerName, string killedName) => { SendChatMessage("killfeed", $"{killerName} killed {killedName}", 225, 0, 0); });




			TriggerServerEvent("sth:showMeOnMap", Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.X);
			TriggerServerEvent("sth:NeedLicense");
			EventHandlers["onClientMapStart"] += new Action<string>(onPlayerLoaded); // event from mapmanager_cliend.lua line 47
			EventHandlers["sth:spawnall"] += new Action(Respawn);
			EventHandlers["sth:resetrespawncounter"] += new Action(ResetRespawnCounter);
			EventHandlers["sth:returnlicense"] += new Action<int,int>(ReceivedLicense); //gets license from server
			EventHandlers["sth:updateRunnerHandle"] += new Action<int>(RunnerHandleUpdate);
			EventHandlers["playerSpawned"] += new Action(onPlayerSpawned); //basically just give guns 
			//EventHandlers["basescript:"]
			#region commands
			API.RegisterCommand("license", new Action<int, List<object>, string>((src, args, raw) =>
			{
				Debug.WriteLine(License.ToString());
			}), false);
			API.RegisterCommand("spawn", new Action<int, List<object>, string>((src, args, raw) =>
			{

				if(respawnCount < 500)
				{
					Respawn();
					respawnCount++;		
					Debug.WriteLine($"respawncount: {respawnCount}");
				}
				else{
					Debug.WriteLine($"spawn limit reached: Limit = 1 RespawnCount = {respawnCount}");
				}

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
						Game.PlayerPed.Weapons.Give(WeaponHash.BullpupRifleMk2, 3000, false, true);

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
			API.RegisterCommand("isdead", new Action<int, List<object>, string>((src, args, raw) =>
			{
				Debug.WriteLine(Game.Player.IsDead.ToString());
				int killer = API.GetPedKiller(API.PlayerPedId());
				Debug.WriteLine($"killer: {killer}");
			}), false);
			API.RegisterCommand("test", new Action<int, List<object>, string>((src, args, raw) =>
			{
				Debug.WriteLine(API.GetPlayerPed(-1).ToString());
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
			if(License == RunnerLicense)
			{
				isRunner = true;
				Respawn();
				SendChatMessage("", "you are now a runner", 255, 255, 255);
			}
			else if (isRunner == true && License != RunnerLicense)
			{
				isRunner = false;
				Respawn();
			}
		}
		void OnPlayerKilled(int killerServerIndex, ExpandoObject info) 
		{

			Debug.WriteLine($"killer: {killerServerIndex}");
			TriggerServerEvent("sth:sendserverkillerserverindex", killerServerIndex);
		}
		void RunnerHandleUpdate(int newRunnerHandle)
		{
			RunnerLicense = newRunnerHandle;
			Debug.WriteLine($"updated runner handle{RunnerLicense}");
			if(License == RunnerLicense)
			{
				isRunner = true;
				Respawn();
			}
			else if (isRunner == true && License != RunnerLicense)
			{
				isRunner = false;
				Respawn();
			}
		}
		void ResetRespawnCounter()
		{
			respawnCount = 0;
			Debug.WriteLine("Spawns reset! RespawnCounts = 0");
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
