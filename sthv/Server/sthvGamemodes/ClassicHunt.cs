using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
namespace sthvServer.sthvGamemodes
{
	internal class ClassicHunt : sthvServer.BaseGamemodeSthv
	{
		internal ClassicHunt() : base("ClassicHunt")
		{
			Debug.WriteLine("^1Message from ClassicHunt.^7");
		}
		[Command("testgm")]
		void commandTestGm()
		{
			Debug.WriteLine("testgm triggered");
		}
		//public async override Task<string[]> Main()
		//{
		//	Debug.WriteLine("Main ran from ClassicHunt.");
		//	await Delay(5000);
		//	Debug.WriteLine("Hunt over.");
		//	string[] res = { "wasd", "dsds" };

		//	return res;
		//}
		public override void test()
		{
			Debug.WriteLine("Here is a test message!!");
		}
	}
}
