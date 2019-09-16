using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX;
using CitizenFX.Core.UI;
using Newtonsoft.Json;

namespace sthvClient
{
	class sthvRules
	{
		int _ped;
		int _pid;
		public sthvRules()
		{
			_ped = API.PlayerPedId();
			_pid = API.PlayerId();
			API.StatSetInt((uint)Game.GenerateHash("MP0_STAMINA"), 100, true);
			//API.SetPoliceIgnorePlayer(_pid, true);  //works like "turn cops blind eye", you get cops if you shoot them or something 
			API.SetMaxWantedLevel(0);
			API.SetPlayerCanDoDriveBy(_pid, false); 
			API.NetworkSetFriendlyFireOption(true);
			API.SetCanAttackFriendly(_ped, true, true);
			API.SetMaxWantedLevel(0);
			API.DisablePlayerVehicleRewards(_pid);
			API.SetEveryoneIgnorePlayer(_pid, true);
			//sthvClient.client.eventhandlers			
			Game.PlayerPed.IsInvincible = true;

			#region ai relationships
			//calm ai 
			API.SetRelationshipBetweenGroups(1, (uint)API.GetHashKey("AMBIENT_GANG_HILLBILLY"),	(uint)API.GetHashKey("PLAYER"));
			API.SetRelationshipBetweenGroups(1, (uint)API.GetHashKey("AMBIENT_GANG_BALLAS"),	(uint)API.GetHashKey("PLAYER"));
			API.SetRelationshipBetweenGroups(1, (uint)API.GetHashKey("AMBIENT_GANG_MEXICAN"),	(uint)API.GetHashKey("PLAYER"));
			API.SetRelationshipBetweenGroups(1, (uint)API.GetHashKey("AMBIENT_GANG_FAMILY"),	(uint)API.GetHashKey("PLAYER"));
			API.SetRelationshipBetweenGroups(1, (uint)API.GetHashKey("AMBIENT_GANG_MARABUNTE"),	(uint)API.GetHashKey("PLAYER"));
			API.SetRelationshipBetweenGroups(1, (uint)API.GetHashKey("AMBIENT_GANG_SALVA"),		(uint)API.GetHashKey("PLAYER"));
			API.SetRelationshipBetweenGroups(1, (uint)API.GetHashKey("GANG_1"),		(uint)API.GetHashKey("PLAYER"));
			API.SetRelationshipBetweenGroups(1, (uint)API.GetHashKey("GANG_2"),		(uint)API.GetHashKey("PLAYER"));
			API.SetRelationshipBetweenGroups(1, (uint)API.GetHashKey("GANG_9"),		(uint)API.GetHashKey("PLAYER"));
			API.SetRelationshipBetweenGroups(1, (uint)API.GetHashKey("GANG_10"),	(uint)API.GetHashKey("PLAYER"));
			API.SetRelationshipBetweenGroups(1, (uint)API.GetHashKey("FIREMAN"),	(uint)API.GetHashKey("PLAYER"));
			API.SetRelationshipBetweenGroups(1, (uint)API.GetHashKey("MEDIC"),		(uint)API.GetHashKey("PLAYER"));
			API.SetRelationshipBetweenGroups(1, (uint)API.GetHashKey("COP"),		(uint)API.GetHashKey("PLAYER"));
			#endregion
		}


		public async Task isKeyPressed() //happens always
		{
			API.HideHudComponentThisFrame((int)HudComponent.Cash);
			API.HideHudComponentThisFrame((int)HudComponent.CashChange);
			API.DisablePlayerVehicleRewards(_pid);
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
			if (API.IsControlJustReleased(0, 86))
			{
				Debug.WriteLine("show me on map");
			}
			if (API.IsControlJustPressed(0, 171))
			{
				API.SendNuiMessage(JsonConvert.SerializeObject(new sthv.NuiModels.NuiEventModel { EventName = "sthv.showsb", EventData = new sthv.NuiModels.dataBool { data = true } }));
			}
			if (API.IsControlJustReleased(0, 171))
			{
				API.SendNuiMessage(JsonConvert.SerializeObject(new sthv.NuiModels.NuiEventModel { EventName = "sthv.showsb", EventData = new sthv.NuiModels.dataBool { data = false } }));
			}
			//await BaseScript.Delay(0);
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
