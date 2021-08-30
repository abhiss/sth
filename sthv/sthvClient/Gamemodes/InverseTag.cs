using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core.Native;
using CitizenFX.Core;

namespace sthv.Gamemodes
{
	class InverseTag : BaseGamemode
	{
		bool first = true;
		Player runner;

		public InverseTag() : base(Shared.Gamemode.InverseTag)
		{
			Debug.WriteLine("START INVERSE TAG");
		
			base.AddEventHandler("test22", new Action(()=>{
				first = true;
			}));

			// base.AddTick(CollisionDetector);
			base.AddTick(DrawCone);
		}
		async Task DrawCone(){
			//send runner from server 
			if(runner.Character.IsTouching(Game.PlayerPed.CurrentVehicle));
			var pos = runner.Character.Position;
			var height = runner.Character.CurrentVehicle.Model.GetDimensions().Z;
			World.DrawMarker(MarkerType.UpsideDownCone, pos + new Vector3(0, 0, height*2), pos, new Vector3(), new Vector3(height*2, height*2, height), System.Drawing.Color.FromArgb(70, 155, 0, 255), true);
		}		
		// async Task CollisionDetector()
		// {
			
		// 	var ped = Game.PlayerPed;

		// 	if(first){
		// 		await BaseScript.Delay(5000);	
		// 		vehicle = await World.CreateRandomVehicle(Game.PlayerPed.Position + new Vector3(0,0,10));
		// 		vehicle.IsInvincible = true;
		// 		vehicle2 = await World.CreateRandomVehicle(Game.PlayerPed.Position);
		// 		vehicle2.IsInvincible = true;

		// 		Game.PlayerPed.SetIntoVehicle(vehicle, VehicleSeat.Driver);
		// 		first = false;
		// 	}

		// 	await BaseScript.Delay(1000);

		// 	// var otherPlayer = new sthvPlayerList().GetNearestPlayer();
		// 	// if(!(ped.IsInVehicle() && otherPlayer.Character.IsInVehicle())){
		// 	// 	return;
		// 	// }
		// 	var touching = vehicle.IsTouching(vehicle2);
		// 	if(touching){
		// 		showMarker = true;
		// 		var act = new Action(async ()=>{
		// 			await BaseScript.Delay(5000);
		// 			showMarker = false;
		// 		});
		// 		act();
		// 	}
		// 	Debug.WriteLine("Distance from vehicle: " + World.GetDistance(vehicle.Position, vehicle2.Position) + " Touching: " + touching);

		// }
	}
}
