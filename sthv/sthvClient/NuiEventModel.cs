using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sthv.NuiModels
{
	public class NuiEventModel
	{
		public string EventName { get; set; }
		public object EventData { get; set; } 
	}
	public class dataBool
	{
		public bool data { get; set; }
	}
	public class updateAlive
	{
		public int serverid { get; set; }
		public bool isalive { get; set; }
		
	}
	public class addToastNotificationModel
	{
		public string message { get; set; }
		public int display_time { get; set; }

	}
	public class NuiScoreboardModel
	{

	}
	public class Player
	{
		public string name { get; set; }
		public string serverid { get; set; }
		public bool runner { get; set; }
		public int score { get; set; }
		public bool alive { get; set; }
		public bool spectating { get; set; }
		public bool isinheli { get; set; }

	}
	public class NuiTimerMessageModel
	{
		public string Message { get; set; }
		public int Seconds { get; set; }
	}
	public class NuiSpectatorInfoModel
	{
		public int nui_HandleOfSpectatedPlayer { get; set; }
	}

	public class NuiTaggedCarModel
	{
		public string tag { get; set; }
		public string team { get; set; }
	}
}
