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
		Dictionary<string, bool> playerPing = new Dictionary<string, bool >();
		Dictionary<string, bool> AlivePlayers = new Dictionary<string, bool>();

		public sthvLobbyManager()
		{
			EventHandlers["sthv:playerJustAlive"] += new Action<Player>(SyncJustAlive);
			EventHandlers["sthv:playerJustDead"] += new Action<Player>(SyncJustDead);
			EventHandlers["playerDropped"] += new Action<Player, string>(OnPlayerDropped);

			//Tick += UpdatePlayers;
			API.RegisterCommand("testaliveplayers", new Action<int, List<object>, string>((src, args, raw) =>
			{
				foreach(Player p in Players)
				{
					if(AlivePlayers.TryGetValue(p.Handle, out bool val) && val){

						Debug.WriteLine($"player {p.Name} is alive, ping is {p.Ping}");
						
					}
					else
					{
						Debug.WriteLine($"player {p.Name} is dead, ping is {p.Ping}");
					}
				}
			}), false);


		}
		void OnPlayerDropped([FromSource]Player source, string reason)
		{
			if (AlivePlayers.ContainsKey(source.Handle)){
				AlivePlayers.Remove(source.Name);
			}
			if (playerPing.ContainsKey(source.Handle))
			{
				playerPing.Remove(source.Handle);
			}
		
			Debug.WriteLine($"dropped {source.Name}");
		}
		void SyncJustAlive([FromSource] Player source)
		{
			Debug.WriteLine($"^4player {source.Name} just alive^7");
			if (AlivePlayers.TryGetValue(source.Handle, out bool val)) //declares val inline 
			{
				if (val != true)
				{
					AlivePlayers[source.Handle] = true;
				}
			}
			else
			{
				AlivePlayers.Add(source.Handle, true);
			}
		}

		void SyncJustDead([FromSource] Player source)
		{
			Debug.WriteLine($"^4player {source.Name} just dead^7");
			if(AlivePlayers.TryGetValue(source.Handle, out bool val)){
				if(val != false)
				{
					AlivePlayers[source.Handle] = false;
				}
			}
			else
			{
				AlivePlayers.Add(source.Handle, false);
			}
		}
		private async Task UpdatePlayers()
		{

			await Delay(3000);
		}
	}
}
