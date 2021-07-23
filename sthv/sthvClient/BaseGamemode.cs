using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace sthv
{
	abstract class BaseGamemode
	{
		Shared.Gamemode gamemodeid;
		public BaseGamemode(Shared.Gamemode id)
		{
			gamemodeid = id;
		}

		private List<NamedEvent> GMEventHandlers = new List<NamedEvent>();
		/// <summary>
		/// Ticks added to client for corresponding gamemode.
		/// </summary>
		private List<Func<Task>> GMTicks = new List<Func<Task>>();

		/// <summary>
		/// Automatically adds eventhandlers to client script prefixed by sth:[gamemodename].
		/// </summary>
		protected void AddEventHandler(string name, Delegate eventhandler)
		{
			//todo predix gamemode events 
			var _newname = "sth:GM_" + gamemodeid + ":" + name;
			GMEventHandlers.Add(new NamedEvent()
			{
				Name = name,
				Handler = eventhandler
			});
		}

		/// <summary>
		/// Automatically adds Ticks to client script.
		/// </summary>
		protected void AddTick(Func<Task> act)
		{
			GMTicks.Add(act);
		}

		public List<NamedEvent>GetEventHandlers()
		{
			return GMEventHandlers;
		}
		public List<Func<Task>> GetTicks()
		{
			return GMTicks;
		}
	}

	public class NamedEvent
	{
		public string Name { get; set; }
		public Delegate Handler { get; set; }
	}
}
