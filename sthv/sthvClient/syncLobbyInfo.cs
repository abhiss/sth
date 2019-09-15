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
	class syncLobbyInfo : BaseScript
	{
		public syncLobbyInfo()
		{
			Tick += syncInfo;

		}
		async Task syncInfo()
		{
			


			await Delay(1000);
		}
	}
}
