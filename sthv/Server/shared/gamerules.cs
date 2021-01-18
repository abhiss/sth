using System;
using System.Collections.Generic;
using System.Text;
using CitizenFX.Core;


namespace sthvServer.shared
{
	class gamerules
	{
		/// <summary>
		/// Player's stamina - Determines how much they can sprint.
		/// </summary>
		public bool UnlimitedStamina { get; set; } = true;

		/// <summary>
		/// Maximum stars a player can is allowed to have. 0 - 5. 
		/// </summary>
		public int MaxWantedLevel { get; set; } = 0;

		/// <summary>
		/// Is shooting from a vehicle allowed?
		/// </summary>
		public bool AllowDriveby { get; set; } = false;

		/// <summary>
		/// Can players get items from vehicles? Like getting a shotgun when they enter a police car.
		/// </summary>
		public bool AllowVehicleRewards { get; set; } = false;

		/// <summary>
		/// Shows runner on hunters' map every 25 seconds if enabled. 
		/// </summary>
		public bool EnableRunnerHints { get; set; } = false;

		/// <summary>
		/// Is minimap allowed? Allowing does NOT show any player on minimap.
		/// </summary>
		public bool AllowRadar { get; set; } = true;
	}
}
