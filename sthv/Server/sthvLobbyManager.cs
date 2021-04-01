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
		//public static List<string> DeadPlayers = new List<string>();

		//List<Player> AlivePlayers = new List<Player>();

		//public static List<sthvPlayer> sthvPlayers = new List<sthvPlayer>(); //replaced by playersinfo
		private static Dictionary<string, SthvPlayer> PlayerData = new Dictionary<string, SthvPlayer>();

		public sthvLobbyManager()
		{
			//add all current players to sthvPlayers
			foreach (var p in Players)
			{
				PlayerData.Add(p.getLicense(), new SthvPlayer(p));
			}

			API.RegisterCommand("checkaliveplayers", new Action<int, List<object>, string>((src, args, raw) =>
			{
				//Debug.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(PlayerData));

				var aliveHunterCount = PlayerData.Where(p => (p.Value.State == SthvPlayer.stateEnum.alive && p.Value.teamname != "runner")).Count();
				var aliveRunnerCount = PlayerData.Where(p => (p.Value.State == SthvPlayer.stateEnum.alive && p.Value.teamname == "runner")).Count();

				Debug.WriteLine($"{aliveHunterCount} alive hunters. {aliveRunnerCount} alive runners.");

				foreach (var p in PlayerData)
				{
					Debug.WriteLine($"Player: {p.Value.player.Name} | State: {p.Value.State.ToString()} | Team: {p.Value.teamname}\n");
				}


			}), false);

		}
		[EventHandler("test:logIdentifiers")]
		private void logIdentifiers([FromSource] Player player)
		{
			Debug.WriteLine(player.Identifiers["discord"]);
		}
		#region playerConnectAndDisconnect
		[EventHandler("playerConnecting")]
		private void OnPlayerConnected([FromSource] Player source, string playerName, dynamic setKickReason, dynamic deferrals)
		{
			Debug.WriteLine("\nw");
			PlayerData.Add(source.getLicense(), new SthvPlayer(source));

			//Debug.WriteLine($"^3Player {source.Name} has {matchingPlayers} instances in sthvPlayers^7");
		}

		[EventHandler("playerDropped")]
		private void OnPlayerDropped([FromSource] Player source, string reason)
		{
			if (source != null)
			{
				//sthvPlayers
				if (!PlayerData.Remove(source.getLicense()))
				{
					Debug.WriteLine($"^3ERROR: PLAYER ${source.Name} WAS NOT IN sthvPlayers WHEN DROPPING.^7");
					Debug.WriteLine("\nPlayers in PlayerInfo");
					foreach (var p in PlayerData)
					{
						Debug.WriteLine("	" + p.Value.player.Name + " - ");
					}
				}
				else
				{
					Debug.WriteLine("dropping player and removing from PlayerData: " + source.Name + ", license: " + source.getLicense());
				}

				if (Server.hasHuntStarted && source.Name == Server.runner.Handle)
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
		#endregion

		[EventHandler("sthv:checkaliveplayers")]
		public static void CheckAlivePlayers()
		{
			var aliveHunterCount = PlayerData.Where(p => (p.Value.State == SthvPlayer.stateEnum.alive && p.Value.teamname != "runner")).Count();
			var aliveRunnerCount = PlayerData.Where(p => (p.Value.State == SthvPlayer.stateEnum.alive && p.Value.teamname == "runner")).Count();
			Debug.WriteLine($"^8{aliveHunterCount} alive hunters. {aliveRunnerCount} alive runners.^7");

			//check if runner is alive
			if (aliveRunnerCount < 1 && Server.hasHuntStarted && !Server.isHuntOver)
			{
				Server.winnerTeamAndReason = ("hunter", "all runners died");

				Server.isHuntOver = true;
				Server.SendChatMessage("^4Hunt", "Hunters win! Hunt ended because runner has died.");
				Server.SendToastNotif("Hunters win! Hunt ended because runner has died.", 7000);
			}
			//check if any hunters are alive
			if (aliveHunterCount < 1 && Server.hasHuntStarted && !Server.isHuntOver)
			{
				Server.winnerTeamAndReason = ("runner", "all hunters died");

				Server.isHuntOver = true;

				Server.SendChatMessage("^4Hunt", "Runner wins! Hunt ended because all hunters have died.");
				Server.SendToastNotif("Runner wins! Hunt ended because all hunters have died.", 7000);
			}
		}


		#region PlayerDataMethods
		/// <summary>
		/// Marks player killed/dead in PlayerData collection
		/// </summary>
		/// <param name="player"></param>
		public static void MarkPlayerDead(Player player, string killerName, string killerLicense)
		{
			SthvPlayer splayer;
			if (PlayerData.TryGetValue(player.getLicense(), out splayer))
			{
				splayer.KillerNameAndLicense = (killerName, killerLicense);
				splayer.State = SthvPlayer.stateEnum.dead;
			}
			else
			{
				Utilities.logError(player.Name + " not found in PlayerData, in MarkPlayerKilled");
			}
		}

		/// <summary>
		/// Marks player alive in PlayerData collection
		/// </summary>
		/// <param name="player"></param>
		public static void MarkPlayerAlive(Player player)
		{
			SthvPlayer splayer;
			if (PlayerData.TryGetValue(player.getLicense(), out splayer))
			{
				splayer.State = SthvPlayer.stateEnum.alive;
			}
			else
			{
				Utilities.logError(player.Name + " not found in PlayerData, in MarkPlayerAlive");
			}
		}

		/// <summary>
		/// Set player's team to provided team name.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="teamname">Can be set to null if player is in no team.</param>
		public static void SetPlayerTeam(Player player, string teamname)
		{
			SthvPlayer splayer;
			if (PlayerData.TryGetValue(player.getLicense(), out splayer))
			{
				splayer.teamname = teamname;
			}
			else
			{
				Utilities.logError(player.Name + " not found in PlayerData, in SetPlayerTeam");
			}
		}
		/// <summary>
		/// Returns all players in the given state(s).
		/// </summary>
		/// <param name="state">array of states to match players with.</param>
		/// <returns></returns>
		public static List<SthvPlayer> GetPlayersOfState(params SthvPlayer.stateEnum[] state)
		{
			List<SthvPlayer> output = new List<SthvPlayer>(32);

			foreach (var item in PlayerData.Values)
			{
				if (state.Contains(item.State))
				{
					output.Add(item);
				}
			}
			return output;
		}

		/// <summary>
		/// Returns all players in the given team(s).
		/// </summary>
		/// <param name="state">array of states to match players with.</param>
		/// <returns></returns>
		public static List<SthvPlayer> GetPlayersInTeam(params string[] teams)
		{
			List<SthvPlayer> output = new List<SthvPlayer>(32);

			foreach (var item in PlayerData.Values)
			{
				if (teams.Contains(item.teamname))
				{
					output.Add(item);
				}
			}
			return output;
		}

		/// <summary>
		/// Sets all alive and dead players' state to waiting. To be used at the end of a round.
		/// </summary>
		public static void setAllActiveToWaiting()
		{
			foreach (var p in PlayerData.Values)
			{
				if (p.State != SthvPlayer.stateEnum.inactive)
				{
					p.State = SthvPlayer.stateEnum.waiting;
				}
			}
		}
		public static SthvPlayer getPlayerByLicense(string license)
		{
			SthvPlayer p;
			if (PlayerData.TryGetValue(license, out p))
			{
				return p;
			}
			else throw new Exception("Player not found by license " + license);

		}
		#endregion


	}
}




