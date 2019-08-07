using System;
using System.Collections.Generic;
using System.Dynamic;

using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;



namespace sthvClient
{
	public class client : BaseScript
	{
		bool IsRunner { get; set; }
		int License { get; set; }
		int RunnerLicense;
		int respawnCount = 0;
		bool isAlreadyDead = false;
		bool isAlreadyKilled = false;
		bool isFrozen = false;

		public client()
		{
			int _ped = Game.Player.Character.Handle;
			//test 		
			API.RegisterCommand("sendpos", new Action<int, List<object>, string>((src, args, raw) =>
			{
				TriggerServerEvent("sth:sendServerDebug", $"{Game.PlayerPed.CurrentVehicle.Position.X.ToString()}f, {Game.PlayerPed.CurrentVehicle.Position.Y.ToString()}f, {Game.PlayerPed.CurrentVehicle.Position.Z.ToString()}f");
				Debug.WriteLine("sent pos");
			}), false);

			var playArea = new sthvClient.sthvPlayArea();
			var rules = new sthvClient.sthvRules();

			Tick += rules.AutoBrakeLight;
			Tick += playArea.OnTickPlayArea;
			Tick += rules.isKeyPressed; //for big map toggle
			Tick += OnTick;

			//Tick += 

			//Killfeed stuff:
			EventHandlers["baseevents:onPlayerKilled"] += new Action<int, ExpandoObject>(OnPlayerKilled);
			EventHandlers["baseevents:onPlayerDied"] += new Action<int, Vector3>((int killer, Vector3 deathCooords) => { Debug.WriteLine("onplayerdied"); }); // event from mapmanager_cliend.lua line 47
			EventHandlers["baseevents:onPlayerWasted"] += new Action<int, Vector3>((int killer, Vector3 deathCoordsb) => { Debug.WriteLine("onplayerwasted"); }); // event from mapmanager_cliend.lua line 47
			EventHandlers["sth:sendKillFeed"] += new Action<string, string>((string killerName, string killedName) => { SendChatMessage("killfeed", $"{killerName} killed {killedName}", 225, 0, 0); });



			TriggerServerEvent("sth:showMeOnMap", Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.X);
			TriggerServerEvent("sth:NeedLicense");
			EventHandlers["onClientMapStart"] += new Action<string>(OnPlayerLoaded); // event from mapmanager_cliend.lua line 47
			EventHandlers["sth:spawnall"] += new Action(Respawn);
			EventHandlers["sth:resetrespawncounter"] += new Action(ResetRespawnCounter);
			EventHandlers["sth:returnlicense"] += new Action<int, int>(ReceivedLicense); //gets license from server
			EventHandlers["sth:updateRunnerHandle"] += new Action<int>(RunnerHandleUpdate);
			//EventHandlers["playerSpawned"] += new Action(onPlayerSpawned); //called from client
			EventHandlers["sth:freezePlayer"] += new Action<bool>(async (bool freeze) => {
				Debug.WriteLine($"freeze event executed, bool: {freeze}, runner: {IsRunner}");
				if (!IsRunner) {
					Spawn.FreezePlayer(-1, freeze);
					this.isFrozen = freeze;
					if (freeze == true)
					{
						Game.PlayerPed.ApplyDamage(900);
						Game.PlayerPed.Weapons.RemoveAll();
						Game.PlayerPed.IsInvincible = true;


						
					}
					else if (!freeze) {
						Respawn();
						await BaseScript.Delay(30000);
						
						Game.PlayerPed.Weapons.Give(WeaponHash.CombatPistol, 225, false, true);
						Game.PlayerPed.Weapons.Give(WeaponHash.PumpShotgun, 225, false, true);
						Game.PlayerPed.Weapons.Give(WeaponHash.Flashlight, 2, false, true);
						Game.PlayerPed.IsInvincible = false;

					}
				}
				if (IsRunner)
				{
					Game.PlayerPed.Weapons.Give(WeaponHash.CombatPistol, 225, false, true);
					Game.PlayerPed.Weapons.Give(WeaponHash.PumpShotgun, 225, false, true);
					Game.PlayerPed.Weapons.Give(WeaponHash.Flashlight, 2, false, true);
					Game.PlayerPed.Weapons.Give(WeaponHash.CarbineRifle, 500, false, true);
					Game.PlayerPed.IsInvincible = false;

					if (freeze)
					{
						await sthv.sthvHuntStart.RemoveAllVehicles(true);
						//Respawn();
						await BaseScript.Delay(15000);
						sthv.sthvHuntStart.HunterVehicles();
					}
				}
			});
					
			EventHandlers["sendChatMessageToAll"] += new Action<string, string>((string header, string message) => { SendChatMessage(header, message, 225, 225, 225); });

			//relating to sthvHuntStart
			//EventHandlers["sthv:OnHuntStartRunner"]
			
			//EventHandlers["basescript:"]
			#region commands
			API.RegisterCommand("license", new Action<int, List<object>, string>((src, args, raw) =>
			{
				Debug.WriteLine(License.ToString());
			}), false);
			//API.RegisterCommand("test", new Action<int, List<object>, string>(async (src, args, raw) =>
			//{
			//	//				await sthvClient.Spawn.SpawnPlayer("a_f_y_hipster_01", 367f, -1698f, 48f, 0f);
			//	//Vehicle car = await World.CreateVehicle(new Model(VehicleHash.Warrener), new Vector3(367f, -1698f, 48f), 300f);
			//	//while (!API.DoesEntityExist(car.Handle))
			//	//{
			//	//	await Delay(1);
			//	//}
			//	//API.SetPedIntoVehicle(Game.Player.Character.Handle, car.Handle, -1);

			//	sthv.sthvHuntStart.HunterVehicles();
			//	//TriggerServerEvent("testevent");
			//}), false);
			//API.RegisterCommand("test2", new Action<int, List<object>, string>(async (src, args, raw) =>
			//{
			//	Vector3 lastPos = Game.PlayerPed.Position;
			//	Game.PlayerPed.Position = new Vector3(0, 0, 0);
			//	await Delay(20);
			//	Game.PlayerPed.Position = lastPos;
				
 		//		//await sthv.sthvHuntStart.RemoveAllVehicles(true);
			//}), false);


			API.RegisterCommand("spawn", new Action<int, List<object>, string>((src, args, raw) =>
			{

				if (respawnCount < 0)																		//respawncount
				{
					Debug.WriteLine($"name = {Game.Player.ServerId}");
					Respawn();
					respawnCount++;
					Debug.WriteLine($"respawncount: {respawnCount}");
				}
				else
				{
					Debug.WriteLine($"spawn limit reached: Limit = 1 RespawnCount = {respawnCount}");
				}

			}), false);
			API.RegisterCommand("checkrunner", new Action<int, List<object>, string>((src, args, raw) =>
			{
				TriggerServerEvent("NeedLicense");
				if (RunnerLicense.Equals(License))
				{
					IsRunner = true;
				}
				SendChatMessage("runner:", $"{IsRunner}", 255, 255, 200);
				Debug.WriteLine($"RUNNER:^2{IsRunner}\nyou:{License}\nrunner: {RunnerLicense}");

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
			API.RegisterCommand("getPlayerPedId", new Action<int, List<object>, string>((src, args, raw) =>
			{
				Debug.WriteLine(API.GetPlayerPed(-1).ToString());
			}), false);

			#endregion

		}

		async Task OnTick() //if killed self
		{
			if(IsRunner == true)
			{
				if (Game.PlayerPed.IsDead)
				{
					//Debug.WriteLine($"isdead, isAlreadyDead: {isAlreadyDead} isAlreadyKilled: {isAlreadyKilled}");
					if ((!isAlreadyDead) && (!isAlreadyKilled))
					{	
						isAlreadyDead = true;
						TriggerServerEvent("sth:killedSelfOrAi");		//suicide or by AI 
						Debug.WriteLine("runner is dead special");	
					}
				}
			}
			else { };
			await BaseScript.Delay(1000);
		} 
		void OnPlayerLoaded(string res) // res from mapmanager_cliend.lua line 47, stores name of map resource
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
				IsRunner = true;
				Respawn();
				SendChatMessage("", "you are now a runner", 255, 255, 255);
			}
			else if (IsRunner == true && License != RunnerLicense)
			{
				IsRunner = false;
				Respawn();
			}
		}
		void OnPlayerKilled(int killerServerIndex, ExpandoObject info) 
		{
			isAlreadyKilled = true;
			Debug.WriteLine($"killer: {killerServerIndex}");
			TriggerServerEvent("sth:sendserverkillerserverindex", killerServerIndex);

		}
		void RunnerHandleUpdate(int newRunnerHandle)
		{
			RunnerLicense = newRunnerHandle;
			Debug.WriteLine($"updated runner handle{RunnerLicense}");
			if(License == RunnerLicense)
			{
				IsRunner = true;
				Respawn();
			}
			else if (IsRunner == true && License != RunnerLicense)
			{
				IsRunner = false;
				Respawn();
			}
		}
		void ResetRespawnCounter()
		{
			respawnCount = 0;
			Debug.WriteLine("Spawns reset! RespawnCounts = 0");
		}
		async void Respawn()
		{
			isAlreadyDead = false;
			isAlreadyKilled = false;
			if (IsRunner)
			{

				await Delay(1000);
				await sthvClient.Spawn.SpawnPlayer("mp_m_freemode_01", 367f, -1698f, 48f, 0f);
				API.SetPedRandomComponentVariation(Game.Player.Character.Handle, false);
				Vehicle car = await World.CreateVehicle(new Model(VehicleHash.Warrener), new Vector3(367f, -1698f, 48f), 300f);
				while (!API.DoesEntityExist(car.Handle))
				{
					await Delay(1);
				}
				API.SetPedIntoVehicle(Game.Player.Character.Handle, car.Handle, -1);

			}
			else
			{
				sthvClient.Spawn.SpawnPlayer("s_m_y_swat_01", 362f, -1705f, 48.3f, 300f);
			}
			respawnCount++;

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
