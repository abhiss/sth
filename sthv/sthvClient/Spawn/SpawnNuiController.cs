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
		}

		private void requestspawn(IDictionary<string, object> obj)
		{
			isSpawnAllowed = true; //if they could press play, they have discord
			TriggerServerEvent("sthv:requestspawn");
			Debug.WriteLine("requested spawn");
		}

		public void gameInputForNui(int keycode) //from client.isKeyPressed task
		{
			
		}

		public void RegisterNuiEventHandler(string eventName, Action<IDictionary<string, object>> action)
		{
			API.RegisterNuiCallbackType(eventName);
			RegisterEventHandler($"__cfx_nui:{eventName}", new Action<ExpandoObject>(o => {
				IDictionary<string, object> data = o;
				action.Invoke(data);
			}));
		}
		[EventHandler("hudintrooff")]
		private void turnoffintro()
		{
			TriggerNuiEvent("sthvui:introoff");
			Debug.WriteLine("introoff");
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
