using System;
using System.Collections.Generic;
using System.Text;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace sthvServer.sthvGamemodes
{
	class TerrorTag : BaseGamemodeSthv
	{
		internal TerrorTag() : base(Shared.Gamemode.TerrorTag, gameLengthInSeconds: 60 * 25, minimumNumberOfPlayers: 2, numberOfTeams: 2)
		{}

		public override void CreateEvents()
		{
			throw new NotImplementedException();
		}


		[EventHandler("TerrorTag:targetVisible")]
		void TargetVisibleHandler() {
			//show target position to all hunters when any hunter has line of sight to target
		}
	}
}
