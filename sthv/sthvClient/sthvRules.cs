using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX;

namespace sthvClient
{
	class sthvRules
	{
		int _ped;
		int _pid;
		bool showBigMap = false;
		public sthvRules()
		{
			_ped = API.PlayerPedId();
			_pid = API.PlayerId();
			API.StatSetInt((uint)Game.GenerateHash("MP0_STAMINA"), 100, true);
			//API.SetPoliceIgnorePlayer(_pid, true);  //works like "turn cops blind eye", you get cops if you shoot them or something 
			API.SetMaxWantedLevel(0);
			API.SetPlayerCanDoDriveBy(_pid, true); 
			API.NetworkSetFriendlyFireOption(true);
			API.SetCanAttackFriendly(_ped, true, true);
			API.SetMaxWantedLevel(0);



			//sthvClient.client.eventhandlers			


		}

		public async Task isZPressed()
		{
			if (API.IsControlJustReleased(20, 48)) //Z 
			{
				if (API.IsBigmapActive())
				{
					API.SetBigmapActive(false, false);
				}
				else
				{
					API.SetBigmapActive(true, false);
				}
			}
			BaseScript.Delay(20);
		}
		public async Task AutoBrakeLight()              //autobrakelight
		{
			_ped = API.PlayerPedId();

			if (API.IsPedInAnyVehicle(_ped, false))
			{
				int vehId = API.GetVehiclePedIsIn(_ped, false);
				bool isVehStopped = API.IsVehicleStopped(vehId);

				if (isVehStopped)
				{
					API.SetVehicleBrakeLights(vehId, true);
				}
				else;
			}
			else
			{
				await BaseScript.Delay(200);
			}
		}
	}
}
