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

			EventHandlers["PlayerConnecting"] += new Action<Player>(OnPlayerPreload);   //when player is joining
			EventHandlers["NeedLicense"] += new Action<Player>(OnPlayerPreload);        //license requested from client (not implemented clientside

		}
		void OnPlayerPreload([FromSource] Player source)        //send client their license 
		{

			string licenseId = source.Identifiers["license"];
			TriggerClientEvent("sth:returnlicense", licenseId);
		
			Debug.WriteLine("done: OnPlayerPreload");
		}


	}

}
