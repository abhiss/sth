using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Newtonsoft.Json;


namespace sthv
{
	class SpawnNuiController : BaseScript
	{
		private static bool optionOpen;

		public static bool GetInput { get { return optionOpen; } }
		public bool isSpawnAllowed { get; set; } = false;

		public static bool IsAllowedHostMenu = false;

		public SpawnNuiController()
		{

			EventHandlers["AskRunnerOpt"] += new Action(async () =>
			{
				optionOpen = true;
				TriggerNuiEvent("sthv:runneropt", true);
				await Delay(10000);
				optionOpen = false;

			});
			EventHandlers["sthv:input:key:8"] += new Action(() => //respond false
			{
				if (optionOpen)
				{
					Debug.WriteLine("picked false");
					optionOpen = false;
					TriggerNuiEvent("sthv:runneropt", false);
				}
			});
			EventHandlers["sthv:input:key:9"] += new Action(() => //respond true
			{
				if (optionOpen)
				{
					Debug.WriteLine("picked true");
					optionOpen = false;
					TriggerNuiEvent("sthv:runneropt", false);
					TriggerServerEvent("sthv:opttorun");
				}
			});
			RegisterNuiEventHandler("sthv:requestspawn", requestspawn);
			//RegisterNuiEventHandler(("nui:returnWantsToRun"), new Action<IDictionary<string, object>>((i) =>
			//{
			//	bool wanttorun = (bool)i["opt"];
			//	Debug.WriteLine($"opt returned: {wanttorun}");
			//	if (wanttorun)
			//	{
			//		TriggerServerEvent("sthv:opttorun");
			//	}
			//}));
			RegisterNuiEventHandler("admin_menu_close", new Action<IDictionary<string, object>>(e =>
			{
				API.SetNuiFocus(false, false);
			}));

			RegisterNuiEventHandler("admin_menu_save", new Action<IDictionary<string, object>>(e =>
			{
				Debug.WriteLine("sending admin_menu settings to server.");
				TriggerServerEvent("admin_menu_save_request", JsonConvert.SerializeObject(e));
			}));
			RegisterNuiEventHandler("lose_focus", new Action<IDictionary<string, object>>(e =>
			{
				API.SetNuiFocus(false, false);
			}));
			RegisterNuiEventHandler("nui_send_to_server", new Action<IDictionary<string, object>>(e =>
			{
				throw new NotImplementedException();
				object eventName;
				object jsonData;
				if (e.TryGetValue("event_name", out eventName) && e.TryGetValue("data", out jsonData))

					TriggerServerEvent((string) eventName, (string) jsonData);
			}));

#if DEBUG
			RegisterNuiEventHandler("save_map_maker_car_finalize", new Action<IDictionary<string, object>>(e =>
			{
				TriggerServerEvent("sthv:MapMaker:save_car_final");
			}));

			RegisterNuiEventHandler("save_map_maker_car", new Action<IDictionary<string, object>>(e =>
			{
				Debug.WriteLine("save_map_maker_car recieved");
				Debug.WriteLine(JsonConvert.SerializeObject(e));
				object carTag;
				object carTeam;
				if(e.TryGetValue("tag", out carTag) && e.TryGetValue("team", out carTeam))
				{
					var tag = (string)carTag;
					var team = (string)carTeam;
					var veh = Game.PlayerPed.CurrentVehicle;

					TriggerServerEvent("sthv:MapMaker:save_car", carTag, carTeam, veh.Position, veh.Heading, veh.Model.Hash);
				}

				API.SetNuiFocus(false, false);
			}));

			API.RegisterKeyMapping("ToggleMapMakerCarMenuOn", "Opens that car map maker menu for STHV, dev only.", "keyboard", "F6");
#endif

			API.RegisterKeyMapping("ToggleHostMenu", "Test", "keyboard", "F4");

		}
#if DEBUG
		[Command("ToggleMapMakerCarMenuOn")]
		private void toggle_map_maker_car_menu()
		{
			TriggerNuiEvent("sthv:map_maker_open_car_label_menu");
			API.SetNuiFocus(true, true);
		}
#endif

		[Command("ToggleHostMenu")]
		public void toggle_host_menu()
		{
			if (IsAllowedHostMenu)
			{
				TriggerNuiEvent("sthv:toggleHostMenu");
				API.SetNuiFocus(true, true);
			}
		}
		private void requestspawn(IDictionary<string, object> obj)
		{
			isSpawnAllowed = true; //if they could press play, they have discord
			TriggerServerEvent("sthv:requestspawn");
			Debug.WriteLine("requested spawn");
		}

		void RegisterNuiEventHandler(string eventName, Action<IDictionary<string, object>> action)
		{
			API.RegisterNuiCallbackType(eventName);
			RegisterEventHandler($"__cfx_nui:{eventName}", new Action<ExpandoObject>(o =>
			{
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
			//API.SetCursorLocation(0.5f, 0.5f);
		}
		public void RegisterEventHandler(string eventName, Delegate action)
		{
			EventHandlers[eventName] += action;
		}
	}
}
