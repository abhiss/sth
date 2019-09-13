using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core.Native;
using CitizenFX.Core;



namespace sthvServer
{
	class sthvPlayer : BaseScript
	{

	}
	class sthvLobbyManager : BaseScript
	{
		PlayerList PlayersAlive { get; set; }
		public PlayerList PlayersDead { get; set; }
		public PlayerList PlayersHunters { get; set; }
		public PlayerList PlayersRunners { get; set; }
		Dictionary<Player, bool> playerPing = new Dictionary<Player, bool >();

		public sthvLobbyManager()
		{
			EventHandlers["sthv:syncserver"] += new Action<Player, bool>(ServerSync);
			
			Tick += UpdatePlayers;



		}
		void ServerSync([FromSource] Player source, bool isAlive)
		{
			if (playerPing.TryGetValue(source, out bool val)) //declares val inline 
			{
				if (val != isAlive)
				{
					playerPing[source] = isAlive;
				}
			}
			else
			{
				playerPing.Add(source, isAlive);
			}
		}
		private async Task UpdatePlayers()
		{
			foreach( Player p in Players)
			{


			}

			await Delay(100);
		}
	}
}
