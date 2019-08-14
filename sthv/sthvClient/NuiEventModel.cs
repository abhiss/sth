using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sthv
{
	public class NuiEventModel
	{
		public string EventName { get; set; }
		public object EventData { get; set; }
	}
	public class NuiMessageModel
	{
		public string Message { get; set; }
		public int Seconds { get; set; }
	}
}
