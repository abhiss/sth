using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace sthv
{
	static class sthvHuntStart
	{

		public static async void HunterVehicles(int gametimeInMsec = 25)
		{
			Vehicle veh;
			List<Vehicle> vehList = new List<Vehicle>();
			veh = await World.CreateVehicle(new Model(VehicleHash.Omnis), new Vector3(368.0813f, -1692.764f, 47.94969f), 48.4f);
			vehList.Add(veh);
			veh = await World.CreateVehicle(new Model(VehicleHash.Minivan), new Vector3(370.3453f, -1690.081f, 47.95068f), 48.4f);
			vehList.Add(veh);
			veh = await World.CreateVehicle(new Model(VehicleHash.Specter), new Vector3(372.4258f, -1687.622f, 47.95185f), 48.4f);
			vehList.Add(veh);
			veh = await World.CreateVehicle(new Model(VehicleHash.Camper), new Vector3(374.9341f, -1684.406f, 47.90953f), 48.4f);
			vehList.Add(veh);
			veh = await World.CreateVehicle(new Model(VehicleHash.Futo), new Vector3(377.0466f, -1681.64f, 47.91183f), 48.4f);
			vehList.Add(veh);
			veh = await World.CreateVehicle(new Model(VehicleHash.Gauntlet2), new Vector3(379.2932f, -1678.444f, 48.06121f), 48.4f);
			vehList.Add(veh);
			veh = await World.CreateVehicle(new Model(VehicleHash.RatLoader), new Vector3(381.9486f, -1676.27f, 47.91648f), 48.4f);
			vehList.Add(veh);
			veh = await World.CreateVehicle(new Model(VehicleHash.RapidGT), new Vector3(383.7508f, -1673.141f, 48.29026f), 48.4f);
			vehList.Add(veh);
			veh = await World.CreateVehicle(new Model(VehicleHash.Maverick), new Vector3(382.9529f, -1656.624f, 48.69487f), 48.4f);
			vehList.Add(veh);

			Debug.WriteLine($"list: {vehList.Count}");


			// repeats
			//World.CreateVehicle(new Model(VehicleHash.PoliceT), new Vector3(383.7508f, -1673.141f, 48.29026f), 48.4f);
			//World.CreateVehicle(new Model(VehicleHash.PoliceT), new Vector3(383.7508f, -1673.141f, 48.29026f), 48.4f);
		}


		/// <summary>
		/// delete props along with vehicles?
		/// </summary>
		/// <param name="props"></param>
		/// <returns></returns>
		public static async Task RemoveAllVehicles(bool props)
		{
			Debug.WriteLine("^4destroy all vehicles^7");
			Vehicle[] allVeh = World.GetAllVehicles();
			foreach (Vehicle veh in allVeh)
			{
				veh.Delete();
			}
			if (props)
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
