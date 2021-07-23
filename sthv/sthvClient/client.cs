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
		public static Shared.Gamemode GamemodeId;
		public static bool IsRunner { get; set; }
		int MyServerId { get; } = Game.Player.ServerId;
		static public int RunnerServerId { get; set; }

		public static sthv.sthvMapModel CurrentMap { get; set; }
		public Ped _thisPed { get; set; }
		private SpawnNuiController spawnnuicontroller { get; set; } = new SpawnNuiController();


		public client()
		{
			TriggerServerEvent("sth:NeedLicense");//so player gets license on resource restarting
			int _ped = Game.Player.Character.Handle;
			//test 		
			API.RegisterCommand("sendpos", new Action<int, List<object>, string>((src, args, raw) =>
			{
				TriggerServerEvent("sth:sendServerDebug", $"{Game.PlayerPed.CurrentVehicle.Position.X}f, {Game.PlayerPed.CurrentVehicle.Position.Y}f, {Game.PlayerPed.CurrentVehicle.Position.Z}f");
			}), false);

			API.RegisterCommand("test", new Action<int, List<object>, string>((src, args, raw) =>
			{
				var pos = Game.PlayerPed.Position; var radius = 10f;
				API.CreateCheckpoint(45, pos.X, pos.Y, pos.Z, 0, 0, 0, radius, 225, 0, 0, 80, 0); //red
				API.CreateCheckpoint(45, pos.X, pos.Y, pos.Z, 0, 0, 0, radius * 2, 0, 225, 0, 80, 0); //green
				API.AddBlipForRadius(pos.X, pos.Y, pos.Z, radius);
				Game.PlayerPed.Position = pos + new Vector3(0, radius, 0);

			}), false);


			var playArea = new sthv.sthvPlayArea();
			Tick += playArea.GetDistance;
			var rules = new sthv.sthvRules();


			EventHandlers["removeveh"] += new Action(async () => { await sthv.sthvHuntStart.RemoveAllVehicles(true); });
			EventHandlers["sthv:spawnhuntercars"] += new Action<int>((mapid) => { sthvHuntStart.HunterVehicles(mapid); });
			EventHandlers["sthv:sendChosenMap"] += new Action<int>(i =>
			{
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

			EventHandlers["sth:setguns"] += new Action<bool>((bool shouldgivegun) =>
			{
				Debug.WriteLine("triggered setguns " + shouldgivegun);
				if (shouldgivegun)
				{
					//AllowedWeapons[0] is a placeholder till HostMenu apis are in place
					foreach (var i in sthvRules.AllowedWeapons)
					{
						Game.PlayerPed.Weapons.Give(i, 500, true, true);
					}
				}
				else
				{
					Game.PlayerPed.Weapons.RemoveAll();
				}
			});
			#region commands
			API.RegisterCommand("test2", new Action<int, List<object>, string>(async (src, args, raw) =>
		   {
			   API.SetPedRandomComponentVariation(Game.Player.Character.Handle, false);
		   }), false);

			API.RegisterCommand("checkrunner", new Action<int, List<object>, string>((src, args, raw) =>
			{
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

			_thisPed = Game.PlayerPed;

			API.SetManualShutdownLoadingScreenNui(true);
			Debug.WriteLine("STHV First Tick");
			TriggerServerEvent("sth:NeedLicense");  //asks server for serverid, runnerid, and discord validation.
			Shared.PlayerJoinInfo res = await sthvFetch.Get<Shared.PlayerJoinInfo>("PlayerJoinInfo");

			SpawnNuiController.IsAllowedHostMenu = res.isAllowedHostMenu;
			Debug.WriteLine("host menu allowed: " + res.isAllowedHostMenu);

			RunnerHandleUpdate(res.runnerServerId);

			client.GamemodeId = res.gamemodeId;

			Debug.WriteLine($"^2 serverid recieved, mine: {MyServerId} runner: {RunnerServerId} gamemode: {GamemodeId}^7");
		}
		[EventHandler("sth:setgamemodeid")]
		void setGamemodeIdHandler(int id_int)
		{
			var id = (Shared.Gamemode)id_int;
			client.GamemodeId = id;

			//random cleanup
			sthvPlayArea.RemovePlayarea();

			Debug.WriteLine("Gamemode set to " + id);
			switch (id)
			{
				case Shared.Gamemode.None:
					break;
				case Shared.Gamemode.ClassicHunt:
					break;
				case Shared.Gamemode.CheckpointHunt:
					{
						//create checkpoint gamemode
						var cphunt = new Gamemodes.CheckpointHunt();

						Debug.WriteLine(cphunt.GetTicks().Count() + " ticks initialized.");
						foreach (var cpevent in cphunt.GetEventHandlers())
						{
							EventHandlers[cpevent.Name] += cpevent.Handler;
						}
						foreach (var act in cphunt.GetTicks())
						{
							Tick += act;
						}
					}
					break;
				default:
					break;
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
		[EventHandler("sthv:showRunnerOnMap")]
		async void ShowRunnerOnMap(int runnerServerId)
		{
			Player _runner = Players[runnerServerId];
			int radius = 150;

			var r = new Random();
			int y_offset = r.Next(-radius / 2, radius / 2);
			int x_offset = r.Next(-radius / 2, radius / 2);

			Debug.WriteLine($"showing runner {_runner.Name} on map");

			var RunnerRadiusBlip = new Blip(API.AddBlipForRadius(_runner.Character.Position.X + x_offset, _runner.Character.Position.Y + y_offset, _runner.Character.Position.Z, radius));

			RunnerRadiusBlip.Color = BlipColor.Red;
			RunnerRadiusBlip.Alpha = 80;
			RunnerRadiusBlip.ShowRoute = true;
			Debug.WriteLine($"is on minimap: {RunnerRadiusBlip.IsOnMinimap}");

			await Delay(10 * 1000);
			if (RunnerRadiusBlip.Exists())
			{
				RunnerRadiusBlip.Delete();
			}
		}

		void OnPlayerKilled(int killerServerIndex, ExpandoObject info)
		{
			Debug.WriteLine($"killer: {killerServerIndex}");
			TriggerServerEvent("sth:sendserverkillerserverindex", killerServerIndex);
		}

		[EventHandler("sth:updateRunnerHandle")]
		void RunnerHandleUpdate(int newRunnerHandle)
		{
			RunnerServerId = newRunnerHandle;
			client.IsRunner = (RunnerServerId == MyServerId);
			Debug.WriteLine($"^5updated runner handle{RunnerServerId}, my handle {MyServerId}");


			if (newRunnerHandle <= 0)
			{
				sthv.sthvPlayerCache.isHuntActive = false;
				IsRunner = false;
				sthv.sthvPlayerCache.runnerPlayer = null;
			}
			else
			{
				sthv.sthvPlayerCache.isHuntActive = true;
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
