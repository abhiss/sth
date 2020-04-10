/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.IO;
using Newtonsoft.Json;

namespace sthvServer
{
	class Identifiers
	{
		List<Vector4> coordList = new List<Vector4>();

		public Identifiers()
		{

			coordList.Add(new Vector4(1, 2, 3, 4));
			coordList.Add(new Vector4(5, 5, 6, 7));
			coordList.Add(new Vector4(3, 412.3f, 123.123f, 231));

			foreach (var i in coordList)
			{
				Console.WriteLine($"Vector4({i.X}f, {i.Y}f, {i.Z}f, {i.W}f)");
			}



			string userpath = $"{API.GetResourcePath(API.GetCurrentResourceName())}/data.txt";
			Debug.WriteLine($"{userpath}");
			var exists = File.Exists(userpath);

			File.Delete(userpath);
			if (true)
			{
				using (var file = File.CreateText(userpath))
				{

					foreach (var i in coordList)
					{
						file.WriteLine($"Vector4({i.X}f, {i.Y}f, {i.Z}f, {i.W}f)");

					}
					file.Flush();
				}
			}

		}
	}
}
*/