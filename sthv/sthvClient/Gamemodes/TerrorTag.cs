using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace sthv.Gamemodes
{
	class TerrorTag : BaseGamemode
	{
		int targetServerId;
		string myTeam;

		public TerrorTag() : base(Shared.Gamemode.TerrorTag)
		{
			AddEventHandler("sthv:spawn_terrortag_vehicles", new Action( async () =>
			{
				await World.CreateVehicle(new Model(1377217886), Game.PlayerPed.Position, 0);
			}));
		}
		async Task CheckTargetInView()
		{
			//Vector3 pos = Game.PlayerPed.Position;
			//var target = await World.CreatePed(new Model(PedHash.Abigail), pos, 0);
			var target = new Ped(API.NetToPed(targetServerId));
			Vector3 pos = target.Position;
			if (World.GetDistance(Game.PlayerPed.Position, target.Position) < 500 && 
				API.IsSphereVisible(pos.X, pos.Y, pos.Z, 2) && 
				API.HasEntityClearLosToEntity(Game.PlayerPed.Handle, target.Handle, 17))
			{
				//API.DrawSphere(pos.X, pos.Y, pos.Z, 1, 255, 255, 100, 0.5f);
				API.DrawRect(0, 0, 0.025f, 0.25f, 255, 0, 0, 100);
				BaseScript.TriggerServerEvent("TerrorTag:targetVisible");
			}
			else
			{
			}
			await BaseScript.Delay(1000);

		}

	}
}
