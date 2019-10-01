using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX;
using CitizenFX.Core;
using CitizenFX.Core.Native;


namespace sthv
{
	class test : BaseScript
	{
		static List<Vector4> listofcoords = new List<Vector4>();

		public test()
		{
			API.RegisterCommand("log", new Action<int, List<object>, string>((src, args, raw) =>
			{
				Vector4[] sendarray = listofcoords.ToArray();
				TriggerServerEvent("logCoords", sendarray);
				foreach (Vector4 i in listofcoords)
				{
					Debug.WriteLine($"new Vector4({i.X}f, {i.Y}f, {i.Z}f, {i.W}f),");
				}
				Debug.WriteLine("command log");
			}), false);

		}
		public static void addCoordToList()
		{
			if (Game.PlayerPed.IsInVehicle())
			{
				var i = new Vector4(Game.PlayerPed.CurrentVehicle.Position.X, Game.PlayerPed.CurrentVehicle.Position.Y, Game.PlayerPed.CurrentVehicle.Position.Z, Game.PlayerPed.CurrentVehicle.Heading);
				listofcoords.Add(i);

				Debug.WriteLine($"new Vector4({i.X}f, {i.Y}f, {i.Z}f, {i.W}f),");
			}
			else
			{
				Debug.WriteLine("not in car, position not added");
			}
		}
	}
}
