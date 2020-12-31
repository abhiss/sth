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

	class sthvLobbyManager : BaseScript
	{
		/// <summary>
		/// reset and used in server.cs to check if joining player had died before so people cant rejoin and get a second life.
		/// </summary>
		public static List<string> DeadPlayers = new List<string>();
		//public PlayerList PlayersHunters { get; set; }
		//public PlayerList PlayersRunners { get; set; }
		Dictionary<string, bool> PlayerPing = new Dictionary<string, bool >();
		List<Player> AlivePlayers = new List<Player>();
		public sthvLobbyManager()
		{
			EventHandlers["playerDropped"] += new Action<Player, string>(OnPlayerDropped);
			EventHandlers["playerConnecting"] += new Action(() =>
			{
			});


			API.RegisterCommand("checkaliveplayers", new Action<int, List<object>, string>((src, args, raw) =>
			{
				foreach(Player p in Players)
				{
					if(DeadPlayers.Contains(p.Identifiers["license"])){
						Debug.WriteLine($"player {p.Name} is dead, ping is {p.Ping}");
					}
					else
					{
						Debug.WriteLine($"player {p.Name} is alive, ping is {p.Ping}");
					}
				}
			}), false);
		}
		[EventHandler("test:logIdentifiers")]
		void logIdentifiers([FromSource]Player player)
		{
			Debug.WriteLine(player.Identifiers["discord"]);
		}
		void OnPlayerDropped([FromSource]Player source, string reason)
		{
			if (source != null)
			{
				string _leftHandle = source.Name;
				if (AlivePlayers.Contains(source))
				{
					AlivePlayers.Remove(source);
				}
				else
				{
					Debug.WriteLine($"player {source.Name} not in alive list anyways :(");
				}
				if (Server.hasHuntStarted && _leftHandle == Server.runner.Handle)
				{
					Debug.WriteLine("^1Runner left :( ^7");
					Server.isHuntOver = true;
				}
			}
			else
			{
				Debug.WriteLine("source returned null onplayerdropped");
			}
			Debug.WriteLine($"dropped {source.Name}");
			CheckAlivePlayers();
			Server.refreshscoreboard();
		}
		[EventHandler("sthv:checkaliveplayers")]
		public void CheckAlivePlayers()
		{
			var alivePlayerCount = 0;
			foreach(Player p in Players)
			{
				if (!DeadPlayers.Contains(p.Identifiers["license"]))
				{
					alivePlayerCount += 1;
				}
			}
			Debug.WriteLine("alive players" + alivePlayerCount.ToString());

			if(alivePlayerCount < 2 && Server.hasHuntStarted)
			{
				Server.isHuntOver = true;
				Server.SendChatMessage("^4Hunt", "All hunters dead, hunt over.");
			}
		}
	}
}




