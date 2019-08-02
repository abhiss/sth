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
			var stuffythings = new Identifiers();
			//Tick += stuffythings.PrintTime;


			API.RegisterCommand("resetspawns", new Action<int, List<object>, string>((src, args, raw) =>
			{
				TriggerClientEvent("sth:resetrespawncounter");

			}), true);
			API.RegisterCommand("spawnall", new Action<int, List<object>, string>((src, args, raw) =>
			{
				TriggerClientEvent("sth:spawnall");

			}), true);

			EventHandlers["sth:NeedLicense"] += new Action<Player>(OnRequestedLicense);
			EventHandlers["sth:sendserverkillerserverindex"] += new Action<Player, int>(KillfeedStuff);
			EventHandlers["sth:showMeOnMap"] += new Action<float, float, float>((float x, float y, float z) => { TriggerClientEvent("sth:sendShowOnMap", x, y, z); });

			API.RegisterCommand("assignrunner", new Action<int, List<object>, string>((source, args, raw) =>
			{
				try
				{
					var argsList = args.Select((object o) => o.ToString()).ToList();
					if (argsList.Any())
					{
						runnerHandle = int.Parse(argsList[0]);
						Debug.WriteLine($"runner handle is now: {runnerHandle}");
						TriggerClientEvent("sth:updateRunnerHandle", runnerHandle);

					}
				}
				catch (Exception ex)
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
		void KillfeedStuff([FromSource]Player killed, int KillerIndex)
		{

			var Killer = API.GetPlayerFromIndex(KillerIndex);
			Debug.WriteLine($"Killer: {Killer} KillerIndex: {KillerIndex}");
			foreach (Player i in Players)
			{
				Debug.WriteLine($"playerhandles = {i.Handle}");
				if (int.Parse(i.Handle) == KillerIndex)
				{
					Debug.WriteLine($"SUCESS! killerhandle ={i.Handle}\nkillername = {i.Name}");
					Debug.WriteLine($"{i.Name} killed {killed.Name}");
					TriggerClientEvent("sth:sendKillFeed", i.Name, killed.Name);
				}
			}
		}
	}

}
