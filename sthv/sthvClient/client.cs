using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Newtonsoft.Json;



namespace sthv
{
	public class client : BaseScript
	{
		bool IsRunner { get; set; }
		int License { get; set; }
		static public int RunnerHandle { get; set; }

		bool isFrozen = false;
		public static sthv.sthvMapModel CurrentMap { get; set; }


		public client()
		{
			int _ped = Game.Player.Character.Handle;
			//test 		
			API.RegisterCommand("sendpos", new Action<int, List<object>, string>((src, args, raw) =>
			{
				//TriggerServerEvent("sth:sendServerDebug", $"{Game.PlayerPed.CurrentVehicle.Position.X.ToString()}f, {Game.PlayerPed.CurrentVehicle.Position.Y.ToString()}f, {Game.PlayerPed.CurrentVehicle.Position.Z.ToString()}f");
				Debug.WriteLine($"{Game.PlayerPed.Position}");

			}), false);
			API.RegisterCommand("test", new Action<int, List<object>, string>((src, args, raw) =>
			{
				//sthv.NuiModels.Player = new sthv.NuiModels.Player { alive = true, name = Game.Player.Name, runner = IsRunner, score = 0, serverid = License, spectating = false }
				List<sthv.NuiModels.Player> ScoreboardPlayerList = new List<sthv.NuiModels.Player>();
				foreach(Player p in Players)
				{
					new sthv.NuiModels.Player { alive = p.IsAlive, name = p.Name, runner = p.ServerId == RunnerHandle, score = 0, serverid = p.ServerId, spectating = false };
				}
			}), false);


			var playArea = new sthv.sthvPlayArea();
			var rules = new sthv.sthvRules();

			Tick += rules.AutoBrakeLight;
			Tick += playArea.OnTickPlayArea;
			Tick += rules.isKeyPressed; //for big map toggle
			Tick += OnTick;

			EventHandlers["removeveh"] += new Action(async () => { await sthv.sthvHuntStart.RemoveAllVehicles(true); });

			//Killfeed stuff:
			EventHandlers["baseevents:onPlayerKilled"] += new Action<int, ExpandoObject>(OnPlayerKilled);
			EventHandlers["sthv:kill"] += new Action(() => { Game.PlayerPed.ApplyDamage(900); });
			//timer
			EventHandlers["sth:starttimer"] += new Action<int>((timeInSecs) => {
				API.SendNuiMessage(JsonConvert.SerializeObject(new sthv.NuiModels.NuiEventModel { EventName = "hunt.countdown", EventData = new sthv.NuiModels.NuiMessageModel { Message = "", Seconds = timeInSecs } }));
				if (timeInSecs < 1)
				{
					sthv.sthvPlayerCache.isHuntActive = false;
				}
				else
				{
					sthv.sthvPlayerCache.isHuntActive = true;
				}
			});
			
			//nui
			EventHandlers["AskRunnerOpt"] += new Action(() =>
			{
				TriggerNuiEvent("sthv:runneropt");
				API.SetNuiFocus(true, true);
			});
			RegisterNuiEventHandler(("nui:returnWantsToRun"), new Action<IDictionary<string, object>>((i) => {
				bool wanttorun = (bool)i["opt"];
				Debug.WriteLine($"opt returned: {wanttorun}");
				API.SetNuiFocus(false, false);
				if (wanttorun)
				{
					TriggerServerEvent("sthv:opttorun");
				}
				
				DefaultSpawn();

			}));
			EventHandlers["sthv:nuifocus"] += new Action<bool>((bool focus) => { API.SetNuiFocus(focus, focus); }); //used as freeze

			EventHandlers["sthv:spawnhuntercars"] += new Action(() => sthv.sthvHuntStart.HunterVehicles());
			EventHandlers["sthv:sendChosenMap"] += new Action<int>(i => sthvHuntStart.SetMap(i));

			TriggerServerEvent("NumberOfAvailableMaps", sthvMaps.Maps.Length);
			
			TriggerServerEvent("sth:NeedLicense");
			EventHandlers["onClientMapStart"] += new Action<string>(OnPlayerLoaded); // event from mapmanager_cliend.lua line 47
			EventHandlers["sth:spawnall"] += new Action(DefaultSpawn);
			EventHandlers["sth:returnlicense"] += new Action<int, int>(ReceivedLicense); //gets license from server
																						 //EventHandlers["playerSpawned"] += new Action(onPlayerSpawned); //called from client
			EventHandlers["sth:updateRunnerHandle"] += new Action<int>(RunnerHandleUpdate);
			EventHandlers["sth:spawn"] += new Action<int>(async(int i) => {
				if (i == 1)
				{
					Debug.WriteLine("Runner spawned");
					await sthv.Spawn.SpawnPlayer("mp_m_freemode_01", CurrentMap.RunnerSpawn.X, CurrentMap.RunnerSpawn.Y, CurrentMap.RunnerSpawn.Z, CurrentMap.RunnerSpawn.W);
					API.SetPedRandomComponentVariation(Game.Player.Character.Handle, false);
					Vehicle car = await World.CreateVehicle(new Model(VehicleHash.Warrener), new Vector3(CurrentMap.RunnerSpawn.X, CurrentMap.RunnerSpawn.Y, CurrentMap.RunnerSpawn.Z), CurrentMap.RunnerSpawn.W);
					while (!API.DoesEntityExist(car.Handle))
					{
						await Delay(1);
					}
					API.SetPedIntoVehicle(Game.Player.Character.Handle, car.Handle, -1);
					IsRunner = true;

				}
				else if(i == 2) 
				{
					await sthv.Spawn.SpawnPlayer("s_m_y_swat_01", CurrentMap.HunterSpawn.X, CurrentMap.HunterSpawn.Y, CurrentMap.HunterSpawn.Z, CurrentMap.HunterSpawn.W);
					IsRunner = false;
				}
			});
			EventHandlers["sth:freezePlayer"] += new Action<bool>((bool freeze) => {
				Debug.WriteLine($"freeze event executed, bool: {freeze}, runner: {IsRunner}");
				if (!IsRunner) {
					Spawn.FreezePlayer(Game.Player.Handle, freeze);
					isFrozen = freeze;
					if (freeze == true)
					{
						Game.PlayerPed.ApplyDamage(900);
						Game.PlayerPed.Weapons.RemoveAll();
						Debug.WriteLine("nui focus true to freeze");

						
					}
					else if (!freeze) {
						API.SetNuiFocus(false, false);
					}
				}

			});
			EventHandlers["sth:invincible"] += new Action<bool>((bool makeGod) => { Game.Player.IsInvincible = makeGod;
				Debug.WriteLine(makeGod.ToString());
			});
			EventHandlers["sth:giveguns"] += new Action<bool>((bool shouldgivegun) =>
			{
				if (shouldgivegun)
				{
					Game.PlayerPed.Weapons.Give(WeaponHash.PistolMk2, 500, true, true);
				}
				else
				{
					Game.PlayerPed.Weapons.RemoveAll();
				}
			});

			#region commands
			API.RegisterCommand("test2", new Action<int, List<object>, string>((src, args, raw) =>
			{
			}), false);

			
			//API.RegisterCommand("starttimer", new Action<int, List<object>, string>((src, args, raw) =>
			//{
			//	try {
			//		int timerCountInSeconds = int.Parse(args[0].ToString());
			//		//Debug.WriteLine($"^3 {args[0].ToString()}");


			//		Debug.WriteLine("started timer");
			//		API.SendNuiMessage(JsonConvert.SerializeObject(new sthv.NuiModels.NuiEventModel { EventName = "hunt.countdown", EventData = new sthv.NuiModels.NuiMessageModel { Message = "", Seconds = timerCountInSeconds } }));

			//			//string testObj = JsonConvert.SerializeObject(new sthv.NuiEventModel { EventName = "this is the eventname" });
			//			//sthv.NuiEventModel deserializedObj = JsonConvert.DeserializeObject<sthv.NuiEventModel>(testObj);
			//			//Debug.WriteLine(deserializedObj.EventName);
			//	}

			//	catch (Exception ex) { Debug.WriteLine($"^3{ex}"); }


			//}), false);
			//API.RegisterCommand("test2", new Action<int, List<object>, string>((src, args, raw) =>
			//{
			//	API.SetNuiFocus(true, true);
			//	TriggerNuiEvent("sthv:runneropt");
			//}), false);


			//API.RegisterCommand("spawn", new Action<int, List<object>, string>((src, args, raw) =>
			//{

			//	if (areSpawnsAllowed)																	
			//	{
			//		DefaultSpawn();
			//	}
			//	else
			//	{
			//		Debug.WriteLine($"you do not have permission to use this command");
			//	}

			//}), false);
			API.RegisterCommand("checkrunner", new Action<int, List<object>, string>((src, args, raw) =>
			{
				TriggerServerEvent("NeedLicense");
				if (RunnerHandle.Equals(License))
				{
					IsRunner = true;
				}
				SendChatMessage("runner:", $"{IsRunner}", 255, 255, 200);
				Debug.WriteLine($"RUNNER:^2{IsRunner}\nyou:{License}\nrunner: {RunnerHandle}");

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
				if ( Game.PlayerPed.IsInSub || Game.PlayerPed.IsInFlyingVehicle)
				{
					World.AddExplosion(Game.PlayerPed.Position, ExplosionType.Rocket, 5f, 2f);
				}
			}

			if(Game.PlayerPed.IsSittingInVehicle() && (Game.PlayerPed.LastVehicle.ClassType == VehicleClass.Super))
			{
				Vehicle veh = Game.PlayerPed.LastVehicle;
				veh.MaxSpeed = 30;
			}
			if (API.IsPedInAnyPoliceVehicle(sthvPlayerCache.playerpedid))
			{
				Debug.WriteLine("in pol car");
				API.SetVehicleColours(Game.PlayerPed.LastVehicle.Handle, (int)VehicleColor.Chrome, (int)VehicleColor.MatteDarkRed);
			}


			await BaseScript.Delay(15000);
		}

		void OnPlayerLoaded(string res) // res from mapmanager_cliend.lua line 47, stores name of map resource
		{
			TriggerServerEvent("sth:NeedLicense");  //asks server for license, ends
			//Respawn();
			TriggerNuiEvent("sthv:runneropt");
			API.SetNuiFocus(true, true);
			
		}



		void ReceivedLicense(int myLicense,int runnerHandle)	//gets license from server
		{
			Debug.WriteLine($"^2license recieved, mine: {myLicense} runner: {RunnerHandle}^7");
			License = myLicense;
			RunnerHandle = runnerHandle;
			//if(License == RunnerHandle)
			//{
			//	IsRunner = true;
			//	Respawn();
			//	SendChatMessage("", "you are now a runner", 255, 255, 255);
			//}
			//else if (IsRunner == true && License != RunnerHandle)
			//{
			//	IsRunner = false;
			//	Respawn();
			//}
		}
		void OnPlayerKilled(int killerServerIndex, ExpandoObject info) 
		{
			Debug.WriteLine($"killer: {killerServerIndex}");
			TriggerServerEvent("sth:sendserverkillerserverindex", killerServerIndex);

		}
		void RunnerHandleUpdate(int newRunnerHandle)
		{
			RunnerHandle = newRunnerHandle;
			Debug.WriteLine($"updated runner handle{RunnerHandle}");
			sthv.sthvPlayerCache.runnerPlayer =  GetPlayerFromServerId(RunnerHandle, Players);

			if( newRunnerHandle == -1)
			{
				sthv.sthvPlayerCache.isHuntActive = false;
				//means hunt is over
			}
			if (License == RunnerHandle) //forced spawn to update runner weapon/ outfit
			{
				IsRunner = true;
			}
			else if (IsRunner == true && License != RunnerHandle)
			{
				IsRunner = false;
			}
		}
		async void DefaultSpawn() //only used for /spawnall i think
		{
			if (CurrentMap != null)
			{
				await sthv.Spawn.SpawnPlayer("s_m_y_swat_01", CurrentMap.HunterSpawn.X, CurrentMap.HunterSpawn.Y, CurrentMap.HunterSpawn.Z, CurrentMap.HunterSpawn.W);
			}
			else
			{
				await sthv.Spawn.SpawnPlayer("s_m_y_swat_01", 1800, 2600, 45, 200);
			}
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
		public void RegisterNuiEventHandler(string eventName, Action<IDictionary<string, object>> action)
		{
			API.RegisterNuiCallbackType(eventName);
			RegisterEventHandler($"__cfx_nui:{eventName}", new Action<ExpandoObject>(o => {
				IDictionary<string, object> data = o;
				action.Invoke(data);
			}));
		}

		public void TriggerNuiEvent(string eventName, dynamic data = null)
		{
			API.SendNuiMessage(JsonConvert.SerializeObject(new sthv.NuiModels.NuiEventModel
			{
				EventName = eventName,
				EventData = data ?? new object()
			}));
			API.SetCursorLocation(0.5f, 0.5f);
		}

		public static Player GetPlayerFromServerId(int playerId, PlayerList players)
		{
			try
			{
				foreach (Player p in players)
				{
					if (p.ServerId == playerId) 
					{
						return p;
					}
				}
				return null;
			}
			catch (Exception ex)
			{
				Debug.Write($"^3ERROR THROWN IN GetPlayerFromId (client): {ex}");
				return null;
			}
		}
	}
}
