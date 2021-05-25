using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace sthv
{
	public static class sthvHuntStart
	{
		public static async void HunterVehicles(int mapId)
		{

			//cop cars: 
			//List<Vehicle> vehList = new List<Vehicle>();
			//await World.CreateVehicle(new Model(VehicleHash.Policeb),	new Vector3(368.0813f, -1692.764f, 47.94969f), 48.4f);
			//await World.CreateVehicle(new Model(VehicleHash.Police3),	new Vector3(370.3453f, -1690.081f, 47.95068f), 48.4f);
			//await World.CreateVehicle(new Model(VehicleHash.StingerGT),	new Vector3(372.4258f, -1687.622f, 47.95185f), 48.4f);
			//await World.CreateVehicle(new Model(VehicleHash.PoliceT),	new Vector3(374.9341f, -1684.406f, 47.90953f), 48.4f);
			//await World.CreateVehicle(new Model(VehicleHash.PoliceOld2),new Vector3(377.0466f, -1681.64f, 47.91183f), 48.4f);
			//await World.CreateVehicle(new Model(VehicleHash.Police2),	new Vector3(379.2932f, -1678.444f, 48.06121f), 48.4f);
			//await World.CreateVehicle(new Model(VehicleHash.Police3),	new Vector3(381.9486f, -1676.27f, 47.91648f), 48.4f);
			//await World.CreateVehicle(new Model(VehicleHash.Police),	new Vector3(383.7508f, -1673.141f, 48.29026f), 48.4f);
			//await World.CreateVehicle(new Model(VehicleHash.Polmav),	new Vector3(382.9529f, -1656.624f, 48.69487f), 48.4f); //heli
			//await World.CreateVehicle(new Model(VehicleHash.Police4),	new Vector3(380f, -1655f, 48.5f), 48.4f);

			Debug.WriteLine("Spawning vehicles for map: " + mapId);
			var map = Shared.sthvMaps.Maps[mapId];
			bool toggle = false;
			foreach (Vector4 i in map.CarSpawnpoints)
			{
				await World.CreateVehicle(new Model(toggle ? VehicleHash.Police : VehicleHash.Police2), new Vector3(i.X,i.Y,i.Z), i.W);
				toggle = !toggle;
				Debug.WriteLine("veh");
			}
			foreach(Vector4 i in map.HeliSpawnPoints)
			{
				await World.CreateVehicle(new Model(VehicleHash.Polmav), new Vector3(i.X, i.Y, i.Z), i.W);

			}
		
		}
		/// <summary>
		/// delete props along with vehicles?
		/// </summary>
		/// <param name="shouldRemoveProps"></param>
		/// <returns></returns>
		public static async Task RemoveAllVehicles(bool shouldRemoveProps)
		{
			Debug.WriteLine("^4destroy all vehicles^7");
			Vehicle[] allVeh = World.GetAllVehicles();
			foreach (Vehicle veh in allVeh)
			{
				veh.Delete();
			}
			if (shouldRemoveProps)
			{
				Prop[] allProp = World.GetAllProps();
				foreach (Prop prop in allProp)
				{
					prop.Delete();
				}
			}
		
			await BaseScript.Delay(0);
		}
	}
}
