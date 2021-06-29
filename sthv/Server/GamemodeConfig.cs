using System;
using System.Collections.Generic;
using System.Text;

namespace sthvServer
{
	static class GamemodeConfig
	{
		public static bool isFriendlyFireAllowed = false;
		public static uint huntLengthSeconds = 15 * 60;
		public static uint respawnTimeSeconds;
		private static string next_runner_serverid;
		public static string huntNextRunnerServerId
		{
			get
			{
				var i = next_runner_serverid;
				next_runner_serverid = null;
				return i;
			}
			set { }
		}

		private static int next_map;
		public static int huntNextMapIndex
		{
			get
			{
				var i = next_map;
				next_map = -1;
				return i;
			}
			set { }
		}
		public static bool isPoliceEnabled;
		public static uint secondsBetweenHints = 20;

	}
}
