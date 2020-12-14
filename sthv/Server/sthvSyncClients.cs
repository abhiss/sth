using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sthvServer
{
	class sthvSyncClients : BaseScript
	{
		static public bool isMissionPedActive { get; set; }
		public Vector3 pedmissionpos { get; set; }
		int pedmissionhealth { get;set; }
		public int currentMpedNetId { get; set; } = 0; //netid of 0 means mission is over
		internal sthvSyncClients()
		{
			API.RegisterCommand("mped", new Action<int, List<object>, string>((src, args, raw) =>
			{
				ActivateMissionPed();
				Debug.WriteLine("mped mission started");
			}), true);

			EventHandlers["sthv:missionpedmade"] += new Action<int>(onNewMissionPed);
			EventHandlers["missionpedstatus"] += new Action<Player, int, Vector3>(targett);
			Tick += updatepedblip;
		}
		async Task updatepedblip()
		{
			if (isMissionPedActive)
			{
				TriggerClientEvent("updateonmped", true, 50, pedmissionpos);
				await Delay(6000);
			}
			else
			{
				TriggerClientEvent("updateonmped", false, 50, pedmissionpos);
			}

		}
		void targett([FromSource]Player source, int health, Vector3 pos)
		{
			Debug.WriteLine($"heath: {health}, pos: {pos}, source: {source.Name}");
			pedmissionhealth = health;
			pedmissionpos = pos;
			if(pedmissionhealth < 1)
			{
				isMissionPedActive = false;
				onNewMissionPed(0);
				Debug.WriteLine("^3mission over^7");
			}
			
		}
		static void ActivateMissionPed()
		{
			server.runner.TriggerEvent("sthv:setnewpedmission");
			isMissionPedActive = true;
		}
		void onNewMissionPed(int netid)
		{
			Console.WriteLine("netid: " + netid);
			TriggerClientEvent("sthv:updatepednetid", netid);
			currentMpedNetId = netid;
		}
	}
}
