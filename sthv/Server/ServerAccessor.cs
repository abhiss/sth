using System;
using System.Collections.Generic;
using System.Text;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace sthvServer
{
	class ServerAccessor
	{
		protected Server Server{ get; }
		public ServerAccessor(Server server)
		{
			Server = server;
		}
	}
}
