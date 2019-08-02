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
		int runnerHandle = 7;
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

 			EventHandlers["sth:NeedLicense"] += new Action<Player>(OnRequestedLicense);

			API.RegisterCommand("assignhunter", new Action<int, List<object>, string>((source, args, raw) =>
			{
				try
				{
					var argsList = args.Select((object o) => o.ToString()).ToList();
					if (argsList.Any())
					{
						runnerHandle = int.Parse(argsList[0]);
						Debug.WriteLine($"hunter handle is now: {runnerHandle}");
						TriggerClientEvent("sth:updateRunnerHandle", runnerHandle);

					}
				}
				catch(Exception ex)
				{
					Debug.WriteLine($"^4assignhunter broke with exception: {ex}^7");
				}

			}), true);
		}
		void OnRequestedLicense([FromSource] Player source)        //send client their license 
		{
			Debug.WriteLine($"^3triggered:onRequestedLicense from: {source.Name}^7");
			string licenseId = source.Handle;
			TriggerClientEvent(source, "sth:returnlicense", licenseId, runnerHandle);
		}

	}

}
