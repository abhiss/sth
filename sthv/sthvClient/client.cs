using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Newtonsoft.Json;



namespace sthvClient
{
	public class client : BaseScript
	{
		bool IsRunner { get; set; }
		int License { get; set; }
		static public int RunnerHandle { get; set; }

		bool isFrozen = false;
		bool areSpawnsAllowed { get; set; } = false;


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


			var playArea = new sthvClient.sthvPlayArea();
			var rules = new sthvClient.sthvRules();

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
			EventHandlers["sthv:nuifocus"] += new Action<bool>((bool focus) => { API.SetNuiFocus(focus, focus); }); //used as makeshift freeze

			EventHandlers["sthv:spawnhuntercars"] += new Action(() => sthv.sthvHuntStart.HunterVehicles());

			TriggerServerEvent("sth:showMeOnMap", Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.X);
			TriggerServerEvent("sth:NeedLicense");
			EventHandlers["onClientMapStart"] += new Action<string>(OnPlayerLoaded); // event from mapmanager_cliend.lua line 47
			EventHandlers["sth:spawnall"] += new Action(DefaultSpawn);
			EventHandlers["sth:returnlicense"] += new Action<int, int>(ReceivedLicense); //gets license from server
																						 //EventHandlers["playerSpawned"] += new Action(onPlayerSpawned); //called from client
			EventHandlers["sth:updateRunnerHandle"] += new Action<int>(RunnerHandleUpdate);
			EventHandlers["sth:spawn"] += new Action<int>(async(int i) => {
				if (i == 1)
				{
					await sthvClient.Spawn.SpawnPlayer("mp_m_freemode_01", 367f, -1698f, 48f, 0f);
					API.SetPedRandomComponentVariation(Game.Player.Character.Handle, false);
					Vehicle car = await World.CreateVehicle(new Model(VehicleHash.Warrener), new Vector3(432f, -1392f, 29.4f), 300f);
					while (!API.DoesEntityExist(car.Handle))
					{
						await Delay(1);
					}
					API.SetPedIntoVehicle(Game.Player.Character.Handle, car.Handle, -1);
					IsRunner = true;

				}
				else if(i == 2) 
				{
					await sthvClient.Spawn.SpawnPlayer("s_m_y_swat_01", 362f, -1705f, 48.3f, 300f);
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
			EventHandlers["sth:invincible"] += new Action<bool>((bool makeGod) => { Game.PlayerPed.IsInvincible = makeGod; });
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
			API.RegisterCommand("license", new Action<int, List<object>, string>((src, args, raw) =>
			{
				Debug.WriteLine(License.ToString());
			}), false);

			
			API.RegisterCommand("starttimer", new Action<int, List<object>, string>((src, args, raw) =>
			{
				try {
					int timerCountInSeconds = int.Parse(args[0].ToString());
					//Debug.WriteLine($"^3 {args[0].ToString()}");


					Debug.WriteLine("started timer");
					API.SendNuiMessage(JsonConvert.SerializeObject(new sthv.NuiModels.NuiEventModel { EventName = "hunt.countdown", EventData = new sthv.NuiModels.NuiMessageModel { Message = "", Seconds = timerCountInSeconds } }));

						//string testObj = JsonConvert.SerializeObject(new sthv.NuiEventModel { EventName = "this is the eventname" });
						//sthv.NuiEventModel deserializedObj = JsonConvert.DeserializeObject<sthv.NuiEventModel>(testObj);
						//Debug.WriteLine(deserializedObj.EventName);
				}

				catch (Exception ex) { Debug.WriteLine($"^3{ex}"); }


			}), false);
			API.RegisterCommand("test2", new Action<int, List<object>, string>((src, args, raw) =>
			{
				API.SetNuiFocus(true, true);
				TriggerNuiEvent("sthv:runneropt");
			}), false);


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
				if (Game.PlayerPed.IsInHeli)
				{
					World.AddExplosion(Game.PlayerPed.Position, ExplosionType.Rocket, 5f, 2f);
				}
			}
			else {

			};


			//SendChatMessage("test", $"word: {API.IsPointOnRoad(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, 0)}", 255, 255, 255);
			// ^ sends in chat if player is on street
			await BaseScript.Delay(5000);
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
			if (License == RunnerHandle) //forced spawn to update runner weapon/ outfit
			{
				IsRunner = true;
				//DefaultSpawn();
			}
			else if (IsRunner == true && License != RunnerHandle)
			{
				IsRunner = false;
				//DefaultSpawn();
			}
		}
		async void DefaultSpawn() //only used for /spawnall i think
		{
			await sthvClient.Spawn.SpawnPlayer("s_m_y_swat_01", 362f, -1705f, 48.3f, 300f);
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
