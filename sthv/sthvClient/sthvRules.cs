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

namespace sthv
{
	class sthvRules : BaseScript
	{
		int _ped;
		int _pid;

		public Blip RunnerRadiusBlip { get; set; }

		public sthvRules()
		{
			Tick += ShowRunnerOnMap;

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

		async Task ShowRunnerOnMap()
		{
			if (sthv.sthvPlayerCache.isHuntActive && sthvPlayerCache.runnerPlayer != null)
			{	
				Player _runner = sthv.sthvPlayerCache.runnerPlayer;
				RunnerRadiusBlip = new Blip(API.AddBlipForRadius(_runner.Character.Position.X, _runner.Character.Position.Y, _runner.Character.Position.Z, 50));

				Debug.WriteLine("showing runner on map :D");
				Debug.WriteLine(_runner.Name.ToString());

				RunnerRadiusBlip.Color = BlipColor.Red;
				RunnerRadiusBlip.Alpha = 80;
				RunnerRadiusBlip.ShowRoute = true;
				Debug.WriteLine($"is on minimap: {RunnerRadiusBlip.IsOnMinimap}");

				//API.SetBlipColour(RunnerRadiusBlip.Handle, 20);
				//API.SetBlipAlpha(RunnerRadiusBlip.Handle, 50);


				await Delay(25000);
				RunnerRadiusBlip.Delete();
			}
			else
			{
				Debug.WriteLine("There is no hunter lol");
			}

			await Delay(50000);
			RunnerRadiusBlip.Delete();
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
				//sthv.test.addCoordToList();
			}
			if (API.IsControlJustPressed(0, 171))
			{
				API.SendNuiMessage(JsonConvert.SerializeObject(new sthv.NuiModels.NuiEventModel { EventName = "sthv.showsb", EventData = new NuiModels.dataBool { data = true } }));
			}
			if (API.IsControlJustReleased(0, 171))
			{
				await Delay(1000);
				API.SendNuiMessage(JsonConvert.SerializeObject(new sthv.NuiModels.NuiEventModel { EventName = "sthv.showsb", EventData = new NuiModels.dataBool { data = false } }));
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
