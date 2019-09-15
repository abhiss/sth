using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core.Native;
using CitizenFX.Core;
using CitizenFX;



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
		Dictionary<Player, bool> playerIsAlive = new Dictionary<Player, bool>();

		public sthvLobbyManager()
		{
			EventHandlers["sthv:playerJustAlive"] += new Action<Player>(SyncJustAlive);
			Tick += UpdatePlayers;
 

		}
		void SyncJustAlive([FromSource] Player source)
		{
			if (playerIsAlive.TryGetValue(source, out bool val)) //declares val inline 
			{
				if (val != true)
				{
					playerIsAlive[source] = true;
				}
			}
			else
			{
				playerIsAlive.Add(source, true);
			}
		}

		void SyncJustdead([FromSource] Player source)
		{
			if(playerIsAlive.TryGetValue(source, out bool val)){
				if(val != false)
				{
					playerIsAlive[source] = false;
				}
			}
			else
			{
				playerIsAlive[source] += 
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
