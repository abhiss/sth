using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace sthv
{
	class sthvSyncLobbyInfo : BaseScript
	{
		public sthvSyncLobbyInfo()
		{
			EventHandlers["sthv:updateAlive"] += new Action<string, bool>(onSomoneDiedOrSomething);
		}
		void onSomoneDiedOrSomething(string serverId, bool isAlive)
		{
			Debug.WriteLine("triggered updateAlive event");

			API.SendNuiMessage(JsonConvert.SerializeObject(new sthv.NuiModels.NuiEventModel
			{
				EventName = "sthvnui:updateAlive",
				EventData = new sthv.NuiModels.updateAlive
				{
					serverid = int.Parse(serverId),
					isalive = isAlive
				}
			}));
			                                                

		}

	}
}
