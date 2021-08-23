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
		private static Dictionary<string, SthvPlayer> PlayerData = new Dictionary<string, SthvPlayer>();


		/* When gamemodemanager detects winnerTeamAndReason != null,
		 * team is declared winner and the tuple is set to null. 
		 */
		public static (string, string) winnerTeamAndReason = (null, null);
		public static bool isGameActive = false;

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

				var aliveHunterCount = PlayerData.Where(p => (p.Value.State == playerState.alive && p.Value.teamname != "runner")).Count();
				var aliveRunnerCount = PlayerData.Where(p => (p.Value.State == playerState.alive && p.Value.teamname == "runner")).Count();

				Debug.WriteLine($"{aliveHunterCount} alive hunters. {aliveRunnerCount} alive runners.");

				foreach (var p in PlayerData)
				{
					Debug.WriteLine($"Player: {p.Value.player.Name} | State: {p.Value.State.ToString()} | Team: {p.Value.teamname} | license: {p.Value.player.getLicense()}\n");
				}


			}), false);

		}
		[EventHandler("test:logIdentifiers")]
		private void logIdentifiers([FromSource] Player player)
		{
			Debug.WriteLine(player.Identifiers["discord"]);
		}
		#region playerConnectAndDisconnect
		[EventHandler("playerJoining")]
		private void OnPlayerJoin([FromSource] Player source, string playerName, dynamic setKickReason, dynamic deferrals)
		{
			Debug.WriteLine("\nw");
			PlayerData.Add(source.getLicense(), new SthvPlayer(source));
			CheckAlivePlayers();

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

		/// <summary>
		/// Checks if runner or hunter team has <1 alive player. If so, sets winnerTeamAndReason, which is an end condition 
		/// handled by gamemode. Also refreshes scoreboard. 
		/// </summary>
		[EventHandler("sthv:checkaliveplayers")]
		public static void CheckAlivePlayers()
		{

			/* counts waiting players (waiting to spawn before hunt starts) so
			 * CheckAlivePlayers doesn't end hunt while its starting */
			//var activePlayers = GetPlayersOfState(playerState.alive, playerState.ready); 

			//i dont think including ready players is necessary.

			var activePlayers = GetPlayersOfState(playerState.alive);

			var aliveHunterCount = activePlayers.Where(p => (p.teamname != "runner")).Count();
			var aliveRunnerCount = activePlayers.Where(p => (p.teamname == "runner")).Count();
			Debug.WriteLine($"^8{aliveHunterCount} alive hunters. {aliveRunnerCount} alive runners.^7");

			if (!string.IsNullOrEmpty(winnerTeamAndReason.Item1))
				return; //because winner was already declared.

			if (!isGameActive) return; //game isn't started, can't declare winner.
			//check if runner is alive
			if (aliveRunnerCount < 1)
			{
				winnerTeamAndReason = ("hunter", "all runners died");

			}
			//check if any hunters are alive
			if (aliveHunterCount < 1)
			{
				winnerTeamAndReason = ("runner", "all hunters died");
			}
			Server.refreshscoreboard();
			
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
				splayer.State = playerState.dead;
				CheckAlivePlayers();
			}
			else
			{
				Utilities.logError(player.Name + " not found in PlayerData, in MarkPlayerKilled");
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
		/// Returns all players in the given state(s). A call without any states returns all players.
		/// </summary>
		/// <param name="state">array of states to match players with.</param>
		/// <returns></returns>
		public static List<SthvPlayer> GetPlayersOfState(params playerState[] state)
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

		public static List<SthvPlayer> GetAllPlayers()
		{
			List<SthvPlayer> output = new List<SthvPlayer>(32);
			foreach(var p in PlayerData.Values)
			{
				output.Add(p);
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
				if (p.State != playerState.inactive)
				{
					p.State = playerState.ready;
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




