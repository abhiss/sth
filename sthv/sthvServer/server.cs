using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace sthvServer
{

	class server : BaseScript
	{

		public server()
		{
			API.RegisterCommand("resetspawns", new Action<int, List<object>, string>((src, args, raw) =>
			{
				TriggerClientEvent("sth:resetrespawncounter");

			}), true);
			API.RegisterCommand("spawnall", new Action<int, List<object>, string>((src, args, raw) =>
			{	
				TriggerClientEvent("sth:spawnall");

			}), true);

 			EventHandlers["NeedLicense"] += new Action<Player>(OnRequestedLicense);

		}
		void OnRequestedLicense([FromSource] Player source)        //send client their license 
		{
			Debug.WriteLine("^3triggered:onRequestedLicense");
			string licenseId = source.Identifiers["license"];
			TriggerClientEvent("sth:returnlicense", licenseId);
		}


	}

}
