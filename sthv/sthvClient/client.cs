using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;


namespace sthv
{
	public class client : BaseScript
	{
		public static bool IsRunner { get; set; }
		int MyServerId { get; set; }
		static public int RunnerServerId { get; set; }

		public static sthv.sthvMapModel CurrentMap { get; set; }
		public Ped _thisPed { get; set; }
		private SpawnNuiController spawnnuicontroller { get; set; } = new SpawnNuiController();

		public client()
		{
			//set props

			TriggerServerEvent("sth:NeedLicense");//so player gets license on resource restarting
			int _ped = Game.Player.Character.Handle;
			//test 		
			API.RegisterCommand("sendpos", new Action<int, List<object>, string>((src, args, raw) =>
			{
				TriggerServerEvent("sth:sendServerDebug", $"{Game.PlayerPed.CurrentVehicle.Position.X}f, {Game.PlayerPed.CurrentVehicle.Position.Y}f, {Game.PlayerPed.CurrentVehicle.Position.Z}f");
			}), false);

			API.RegisterCommand("test", new Action<int, List<object>, string>(async (src, args, raw) =>
			{
				//var pingRes = await sthvFetch.Get<Shared.Ping>("ping");
				Debug.WriteLine("Hide cursor");
				API.SetNuiFocus(true, true);
			}), false);

			_thisPed = Game.PlayerPed;
			var playArea = new sthv.sthvPlayArea();
			Tick += playArea.GetDistance;
			var rules = new sthv.sthvRules();


			EventHandlers["removeveh"] += new Action(async () => { await sthv.sthvHuntStart.RemoveAllVehicles(true); });
			EventHandlers["sthv:spawnhuntercars"] += new Action<int>((mapid) => { sthvHuntStart.HunterVehicles(mapid); });
			EventHandlers["sthv:sendChosenMap"] += new Action<int>(i => {
				sthvHuntStart.SetMap(i);
				});



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

			EventHandlers["sth:spawnall"] += new Action(DefaultSpawn);

			//EventHandlers["playerSpawned"] += new Action(onPlayerSpawned); //called from client
			EventHandlers["sth:updateRunnerHandle"] += new Action<int>(RunnerHandleUpdate);

			//legacy and shouldn't be used. here for reference.
			//EventHandlers["sth:spawn"] += new Action<int>(async (int i) =>
			//{
			//	if (spawnnuicontroller.isSpawnAllowed)
			//	{
			//		Debug.WriteLine("spawn event " + i.ToString());
			//		if (i == 0)
			//		{
			//			//idle spawn not implemented
			//		}
			//		else if (i == 1)
			//		{
			//			Debug.WriteLine("Runner spawned");
			//			await sthv.Spawn.SpawnPlayer("mp_m_freemode_01", CurrentMap.RunnerSpawn.X, CurrentMap.RunnerSpawn.Y, CurrentMap.RunnerSpawn.Z, CurrentMap.RunnerSpawn.W);
			//			API.SetPedRandomComponentVariation(Game.Player.Character.Handle, false);
			//			Vehicle car = await World.CreateVehicle(new Model(VehicleHash.Warrener), new Vector3(CurrentMap.RunnerSpawn.X, CurrentMap.RunnerSpawn.Y, CurrentMap.RunnerSpawn.Z), CurrentMap.RunnerSpawn.W);
			//			while (!API.DoesEntityExist(car.Handle))
			//			{
			//				await Delay(1);
			//			}
			//			API.SetPedIntoVehicle(Game.Player.Character.Handle, car.Handle, -1);
			//			IsRunner = true;
			//		}
			//		else if (i == 2)
			//		{
			//			await sthv.Spawn.SpawnPlayer("s_m_y_swat_01", CurrentMap.HunterSpawn.X, CurrentMap.HunterSpawn.Y, CurrentMap.HunterSpawn.Z, CurrentMap.HunterSpawn.W);
			//			IsRunner = false;
			//		}
			//		else if (i == 3)
			//		{
			//			await Spawn.SpawnPlayer("s_m_y_swat_01", CurrentMap.HunterSpawn.X, CurrentMap.HunterSpawn.Y, CurrentMap.HunterSpawn.Z, CurrentMap.HunterSpawn.W);
			//			Game.PlayerPed.Kill();
			//		}
			//		API.SetNuiFocus(false, false);
			//	}
			//	else Debug.WriteLine("spawning not allowed for me");
			//});

			EventHandlers["sth:setguns"] += new Action<bool>((bool shouldgivegun) =>
			{
				Debug.WriteLine("triggered setguns " + shouldgivegun);
				if (shouldgivegun)
				{
					//AllowedWeapons[0] is a placeholder till HostMenu apis are in place
					Game.PlayerPed.Weapons.Give(sthvRules.AllowedWeapons[0], 500, true, true);
				}
				else
				{
					Game.PlayerPed.Weapons.RemoveAll();
				}
			});
			#region commands
			API.RegisterCommand("test2", new Action<int, List<object>, string>(async(src, args, raw) =>
		   {
			   API.SetPedRandomComponentVariation(Game.Player.Character.Handle, false);
		   }), false);

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

		[Tick]
		async Task FirstTick() // res from mapmanager_cliend.lua line 47, stores name of map resource
		{
			Tick -= FirstTick;
			API.SetManualShutdownLoadingScreenNui(true);
			Debug.WriteLine("STHV First Tick");
			TriggerServerEvent("sth:NeedLicense");  //asks server for serverid, runnerid, and discord validation.
			Shared.PlayerJoinInfo res = await sthvFetch.Get<Shared.PlayerJoinInfo>("PlayerJoinInfo");

			MyServerId = Game.Player.ServerId;
			RunnerServerId = res.runnerServerId;
			if (res.isInSTHGuild || !res.isDiscordServerOnline)
			{
				spawnnuicontroller.isSpawnAllowed = true;
			}
			else
			{
				spawnnuicontroller.isSpawnAllowed = false;
			}
			Debug.WriteLine($"^2 serverid recieved, mine: {MyServerId} runner: {RunnerServerId}^7");

			TriggerNuiEvent("sthv:discordVerification", new { has_discord = res.hasDiscord, is_in_sth = res.isInSTHGuild, is_in_vc = res.isInVc, is_discord_online = res.isDiscordServerOnline });
			SendChatMessage("test", res.isDiscordServerOnline.ToString());
			if (!res.isDiscordServerOnline)
			{
				Debug.WriteLine("Discord server not online");
				//SendChatMessage("sthv", "Discord verification failed for technical reasons. Anyone can play.");
			}
		}

		[EventHandler("sth:spawn")]
		async void Spawn(Vector4 location, string pedSkin)
		{
			if (pedSkin == "random_runner")
			{
				await sthv.Spawn.SpawnPlayer("mp_m_freemode_01", location.X, location.Y, location.Z, location.W);
				API.SetPedRandomComponentVariation(Game.Player.Character.Handle, false);
			}
			else
			{
				await sthv.Spawn.SpawnPlayer(pedSkin, location.X, location.Y, location.Z, location.W);

			}
			TriggerEvent("sthv:spectate", false);

		}

		//delete this
		void ReceivedLicense(int runnerHandle, bool hasDiscord, bool isInSTH, bool isInVc, bool isDiscordServerOnline)  //gets license from server
		{

			RunnerServerId = runnerHandle;
			if (isInSTH || !isDiscordServerOnline)
			{
				spawnnuicontroller.isSpawnAllowed = true;
			}
			else
			{
				spawnnuicontroller.isSpawnAllowed = false;
			}
			Debug.WriteLine($"^2 serverid recieved, mine: {Game.Player.ServerId} runner: {RunnerServerId}^7");

			TriggerNuiEvent("sthv:discordVerification", new { has_discord = hasDiscord, is_in_sth = isInSTH, is_in_vc = isInVc, is_discord_online = isDiscordServerOnline });
			SendChatMessage("test", isDiscordServerOnline.ToString());
			if (!isDiscordServerOnline)
			{
				Debug.WriteLine("Discord server not online");
				//SendChatMessage("sthv", "Discord verification failed for technical reasons. Anyone can play.");
			}
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
			if (newRunnerHandle <= 0)
			{
				sthv.sthvPlayerCache.isHuntActive = false; //means hunts over
				IsRunner = false;
				sthv.sthvPlayerCache.runnerPlayer = null;
			}
			else
			{
				sthv.sthvPlayerCache.isHuntActive = true; //means hunts started
				sthv.sthvPlayerCache.runnerPlayer = GetPlayerFromServerId(RunnerServerId, Players);

			}
			if (MyServerId == RunnerServerId) //update runner weapon/ outfit
			{
				IsRunner = true;
					API.SetPedRandomComponentVariation(Game.Player.Character.Handle, false);
			}
			else if (MyServerId != RunnerServerId)
			{
				IsRunner = false;
			}
		}
		async void DefaultSpawn() //used for /spawnall
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
