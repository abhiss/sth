using System;
using System.Collections.Generic;
using System.Text;
using CitizenFX.Core;
using Newtonsoft.Json;

namespace Shared
{
	abstract class BaseFetchClass
	{
		public bool isSuccessful { get; set; }
	}
	class PlayerConfigOptionsModel : BaseFetchClass
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
		/// Shows runner on hunters' map every 30 seconds if enabled. 
		/// </summary>
		public bool EnableRunnerHints { get; set; } = false;

		/// <summary>
		/// Is minimap allowed? Allowing it does NOT show any player on minimap.
		/// </summary>
		public bool AllowRadar { get; set; } = true;
	}
	class PlayerJoinInfo : BaseFetchClass
	{
		public int runnerServerId;
		public bool hasDiscord;
		public bool isInSTHGuild;
		public bool isInVc;
		public bool isDiscordServerOnline;
	}
	class GameInfo : BaseFetchClass
	{
		public int playarea;
	}
	class Ping : BaseFetchClass
	{
		public string response;
	}
	public class AdminMenuSave
	{
		public uint next_respawn_time;
		public string next_runner_serverid;
		public uint next_hunt_length;
		public int next_map;
		public uint seconds_between_hints;
		public bool is_friendly_fire_allowed;
		public bool is_cops_enabled;
		public bool end_hunt;
	}
	public class ScoreboardInfoPlayer
	{
		public string serverid;
		public bool is_in_helicopter;
		public bool is_runner; //todo: remove gamemode specific property
		public bool is_alive;
	}
}
