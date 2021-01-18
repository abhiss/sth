using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;



namespace sthv
{
	public class client : BaseScript
	{
		bool IsRunner { get; set; }
		int MyServerId { get; set; }
		static public int RunnerServerId { get; set; }
		public WeaponHash weapon { get; set; } = WeaponHash.PistolMk2;
		public static sthv.sthvMapModel CurrentMap { get; set; }
		public Ped _thisPed { get; set; }
		private SpawnNuiController spawnnuicontroller { get; set; } = new SpawnNuiController();
		public client()
		{
			//set props


			TriggerServerEvent("sth:NeedLicense");//so player gets license on resource restarting
			int _ped = Game.Player.Character.Handle;
			//test 		
			API.RegisterCommand("sendpos", new Action<int, List<object>, string>(async (src, args, raw) =>
			{
				//TriggerServerEvent("sth:sendServerDebug", $"{Game.PlayerPed.CurrentVehicle.Position.X.ToString()}f, {Game.PlayerPed.CurrentVehicle.Position.Y.ToString()}f, {Game.PlayerPed.CurrentVehicle.Position.Z.ToString()}f");
				await sthv.Spawn.SpawnPlayer("s_m_y_swat_01", CurrentMap.HunterSpawn.X, CurrentMap.HunterSpawn.Y, CurrentMap.HunterSpawn.Z, CurrentMap.HunterSpawn.W);
				API.SetNuiFocus(false, false);
			}), false);

			API.RegisterCommand("test", new Action<int, List<object>, string>(async (src, args, raw) =>
		    {
			    //TriggerNuiEvent("sthv:showToastNotification", new { message = "VV", display_time = 4000 });
			    var i = await sthvFetch.DownloadString("ping");
			    Debug.WriteLine(i);
			    //Debug.WriteLine("requesting license");
		 	    //TriggerServerEvent("sth:NeedLicense");  //asks server for serverid, runnerid, and discord validation. 
		    }), false);

			_thisPed = Game.PlayerPed;
			var playArea = new sthv.sthvPlayArea();
			var rules = new sthv.sthvRules();


			Debug.WriteLine("Send toast notif");

			Tick += rules.AutoBrakeLight;
			Tick += playArea.OnTickPlayArea;
			Tick += rules.onTick; //for big map toggle
			Tick += OnTick;

			EventHandlers["removeveh"] += new Action(async () => { await sthv.sthvHuntStart.RemoveAllVehicles(true); });

			//Killfeed stuff:
			EventHandlers["baseevents:onPlayerKilled"] += new Action<int, ExpandoObject>(OnPlayerKilled);

			EventHandlers["sthv:kill"] += new Action(() => { Game.PlayerPed.ApplyDamage(900); });
			//timer
			EventHandlers["sth:starttimer"] += new Action<int>((timeInSecs) =>
			{
				API.SendNuiMessage(JsonConvert.SerializeObject(new sthv.NuiModels.NuiEventModel
				{
					EventName = "hunttimer",
					EventData = new sthv.NuiModels.NuiTimerMessageModel { Message = "", Seconds = timeInSecs }
				}));
			});



			EventHandlers["sthv:showToastNotification"] += new Action<string, int>((message, timeInSeconds) => //time_delay in ms
			{
				TriggerNuiEvent("sthv:showToastNotification", new { message = message, display_time = timeInSeconds });

			});
			EventHandlers["sthv:spawnhuntercars"] += new Action(() => sthv.sthvHuntStart.HunterVehicles());
			EventHandlers["sthv:sendChosenMap"] += new Action<int>(i => sthvHuntStart.SetMap(i));

			TriggerServerEvent("NumberOfAvailableMaps", sthvMaps.Maps.Length);

			EventHandlers["sth:spawnall"] += new Action(DefaultSpawn);
			EventHandlers["sth:returnlicense"] += new Action<int, int, bool, bool, bool, bool>(ReceivedLicense); //gets myserverid, runnerserverid, hasdiscord, isinguild, in pc-voice 

			//EventHandlers["playerSpawned"] += new Action(onPlayerSpawned); //called from client
			EventHandlers["sth:updateRunnerHandle"] += new Action<int>(RunnerHandleUpdate);
			EventHandlers["sth:spawn"] += new Action<int>(async (int i) =>
			{
				if (spawnnuicontroller.isSpawnAllowed)
				{
					Debug.WriteLine("spawn event " + i.ToString());
					if (i == 0)
					{
						//idle spawn not implemented
					}
					else if (i == 1)
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
					else if (i == 2)
					{
						await sthv.Spawn.SpawnPlayer("s_m_y_swat_01", CurrentMap.HunterSpawn.X, CurrentMap.HunterSpawn.Y, CurrentMap.HunterSpawn.Z, CurrentMap.HunterSpawn.W);
						IsRunner = false;
					}
					else if (i == 3)
					{
						await Spawn.SpawnPlayer("s_m_y_swat_01", CurrentMap.HunterSpawn.X, CurrentMap.HunterSpawn.Y, CurrentMap.HunterSpawn.Z, CurrentMap.HunterSpawn.W);
						Game.PlayerPed.Kill();
					}
					API.SetNuiFocus(false, false);
				}
				else Debug.WriteLine("spawning not allowed for me");
			});

			EventHandlers["sth:setguns"] += new Action<bool>((bool shouldgivegun) =>
			{
				Debug.WriteLine("triggered setguns " + shouldgivegun);
				if (shouldgivegun)
				{
					Game.PlayerPed.Weapons.Give(weapon, 500, true, true);
				}
				else
				{
					Game.PlayerPed.Weapons.RemoveAll();
				}
			});
			#region commands
			API.RegisterCommand("test2", new Action<int, List<object>, string>(async (src, args, raw) =>
		   {
			   API.SetNuiFocus(true, true);
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


			API.RegisterCommand("checkrunner", new Action<int, List<object>, string>((src, args, raw) =>
			{
				TriggerServerEvent("NeedLicense");
				if (RunnerServerId.Equals(MyServerId))
				{
					IsRunner = true;
				}
				SendChatMessage("runner:", $"{IsRunner}", 255, 255, 200);
				Debug.WriteLine($"RUNNER:^2{IsRunner}\nyou:{MyServerId}\nrunner: {RunnerServerId}");

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

		async Task OnTick() //checks rules
		{
			Debug.WriteLine("^2 isrunner: " + IsRunner);
			if (IsRunner)
			{
				Debug.WriteLine("is runner");
				if (Game.PlayerPed.IsInSub || Game.PlayerPed.IsInFlyingVehicle)
				{
					World.AddExplosion(Game.PlayerPed.Position, ExplosionType.Rocket, 5f, 2f);
				}
				if (API.IsPedInAnyPoliceVehicle(Game.PlayerPed.Handle))
				{
					Debug.WriteLine("in police car");
					API.SetHornEnabled(Game.Player.LastVehicle.Handle, true);
					API.SetHornEnabled(API.GetVehiclePedIsIn(API.PlayerPedId(), false), true);
					//API.SetVehicleTyreBurst(Game.PlayerPed.LastVehicle.Handle, 0, true, 100);
				}
				if (Game.PlayerPed.IsInSub || Game.PlayerPed.IsInFlyingVehicle)
				{
					World.AddExplosion(Game.PlayerPed.Position, ExplosionType.Rocket, 5f, 2f);
				}
			}
			if (Game.PlayerPed.IsSittingInVehicle() && (Game.PlayerPed.LastVehicle.ClassType == VehicleClass.Super))
			{
				Vehicle veh = Game.PlayerPed.LastVehicle;
				veh.MaxSpeed = 25f;
				//veh.Speed = 100f;
			}
			await BaseScript.Delay(10000);
		}

		[Tick]
		async Task FirstTick() // res from mapmanager_cliend.lua line 47, stores name of map resource
		{
			Tick -= FirstTick;
			API.SetManualShutdownLoadingScreenNui(true);
			Debug.WriteLine("^1ONPLAYERLOADEKKKD");
			TriggerServerEvent("sth:NeedLicense");  //asks server for serverid, runnerid, and discord validation.
			API.SetNuiFocus(true, true);


		}

		void ReceivedLicense(int myServerId, int runnerHandle, bool hasDiscord, bool isInSTH, bool isInVc, bool isDiscordServerOnline)  //gets license from server
		{
			MyServerId = myServerId;
			RunnerServerId = runnerHandle;
			if (isInSTH || !isDiscordServerOnline)
			{
				spawnnuicontroller.isSpawnAllowed = true;
			}
			else
			{
				spawnnuicontroller.isSpawnAllowed = false;
			}
			Debug.WriteLine($"^2 serverid recieved, mine: {myServerId} runner: {RunnerServerId}^7");

			TriggerNuiEvent("sthv:discordVerification", new { has_discord = hasDiscord, is_in_sth = isInSTH, is_in_vc = isInVc, is_discord_online = isDiscordServerOnline });
			SendChatMessage("test", isDiscordServerOnline.ToString());
			if (!isDiscordServerOnline)
			{
				Debug.WriteLine("Discord server not online");
				//SendChatMessage("sthv", "Discord verification failed for technical reasons. Anyone can play.");
			}
			//API.SetNuiFocus(true, true);
		}
		void OnPlayerKilled(int killerServerIndex, ExpandoObject info)
		{
			Debug.WriteLine($"killer: {killerServerIndex}");
			TriggerServerEvent("sth:sendserverkillerserverindex", killerServerIndex);
		}
		void RunnerHandleUpdate(int newRunnerHandle)
		{
			RunnerServerId = newRunnerHandle;
			Debug.WriteLine($"^5updated runner handle{RunnerServerId}, my handle {MyServerId}");
			sthv.sthvPlayerCache.runnerPlayer = GetPlayerFromServerId(RunnerServerId, Players);
			if (newRunnerHandle < 0)
			{
				sthv.sthvPlayerCache.isHuntActive = false;//means hunts over
				IsRunner = false;
			}
			else
			{
				sthv.sthvPlayerCache.isHuntActive = true;//means hunts over
			}
			if (MyServerId == RunnerServerId) //forced spawn to update runner weapon/ outfit
			{
				IsRunner = true;
				if (Game.PlayerPed.Model == new Model(PedHash.Swat01SMY))
				{
					API.SetPedRandomComponentVariation(Game.Player.Character.Handle, false);
				}
			}
			else if (MyServerId != RunnerServerId)
			{
				IsRunner = false;
			}
		}
		async void DefaultSpawn() //only used for /spawnall i think
		{
			API.SetNuiFocus(false, false);
			TriggerNuiEvent("sthv:discordVerification", new { has_discord = false, is_in_sth = false, is_in_vc = false, is_discord_online = false });

			if (CurrentMap != null)
			{
				await sthv.Spawn.SpawnPlayer("s_m_y_swat_01", CurrentMap.HunterSpawn.X, CurrentMap.HunterSpawn.Y, CurrentMap.HunterSpawn.Z, CurrentMap.HunterSpawn.W);
			}
			else
			{
				Debug.WriteLine("^1 ERROR: CurrentMap is null");
				await sthv.Spawn.SpawnPlayer("s_m_y_swat_01", 1800, 2600, 45, 200);
			}
		}

		public void RegisterEventHandler(string eventName, Delegate action)
		{
			EventHandlers[eventName] += action;
		}
		public void UnregisterEventHandler(string eventName, Delegate action)
		{
			EventHandlers[eventName] -= action;
		}
		public static void SendChatMessage(string title, string message, int r = 0, int g = 0, int b = 0)
		{
			var msg = new Dictionary<string, object>
			{
				["color"] = new[] { r, g, b },
				["args"] = new[] { title, message }
			};
			TriggerEvent("chat:addMessage", msg);
		}


		public void TriggerNuiEvent(string eventName, dynamic data = null)
		{
			API.SendNuiMessage(JsonConvert.SerializeObject(new sthv.NuiModels.NuiEventModel
			{
				EventName = eventName,
				EventData = data ?? new object()
			}));
			//API.SetCursorLocation(0.5f, 0.5f);
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
				Debug.WriteLine("couldnt get player from playerid");
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
