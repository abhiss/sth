using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
namespace sthvServer
{
	class Identifiers : server
	{
		public Identifiers()
		{
			EventHandlers["PlayerConnecting"] += new Action<Player>(OnPlayerPreload);
		}
		void OnPlayerPreload([FromSource] Player source)
		{
			Debug.WriteLine("done: OnPlayerPreload");
			string licenseId = source.Identifiers["license"];
			TriggerClientEvent("licenseId", licenseId);

		}
	}
}
