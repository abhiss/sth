using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
namespace sthvServer.sthvGamemodes
{
	internal class ClassicHunt : BaseGamemodeSthv
	{

		internal ClassicHunt() : base(gamemodeName: "ClassicHunt", gameLengthInSeconds: 15 * 60, minimumNumberOfPlayers: 1)
		{
			
			CreateTeams(null, 
				new sthvGamemodeTeam { Name = "runner", MaximumPlayers = 1, MinimumPlayers = 1 },
				new sthvGamemodeTeam { Name = "hunter", MaximumPlayers = -1, MinimumPlayers = 0 }
			);

			AddTimeEvent(0, new Action(() =>
			{

				log("This happens when gamemode starts");
				TriggerClientEvent("sthv:sendChosenMap", 3);
			}));
			AddTimeEvent(10, new Action(() =>
			{
				log("This happens after 10 seconds.");
			}));
			AddTimeEvent(20, new Action(() =>
			{
				log("This happens after 20 seconds");
			}));
		}
		[Command("testgm")]
		void commandTestGm()
		{
			Debug.WriteLine("testgm triggered");
		}
		public override void test()
		{
			Debug.WriteLine("Here is a test message!!");
		}
	}
}
