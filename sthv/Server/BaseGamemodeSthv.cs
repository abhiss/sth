using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;


namespace sthvServer
{
	abstract internal class BaseGamemodeSthv : BaseScript
	{
		private uint GameLengthInSeconds { get; set; }
		private protected bool endMode = false; //used to end mode prematurely. Can be called either by gamemode manager (server.cs) or the gamemode.
		private protected string endModeReason = null;
		public int MinimumPlayers { get; }
		private Dictionary<uint, Action> TimedEventsList = new Dictionary<uint, Action>();
		public Action Finalizer = null;
		public string Name { get; }
		private sthvGamemodeTeam[] gamemodeTeams;
		public bool isGameloopActive = false;
		private uint timeleft;
		private uint timeSecondsSinceRoundStart;

		public uint TimeLeft { get { return timeleft; } }
		public uint TimeSinceRoundStart { get { return timeSecondsSinceRoundStart; } }
		internal BaseGamemodeSthv(string gamemodeName, uint gameLengthInSeconds, int minimumNumberOfPlayers, int numberOfTeams = 2)
		{
			Name = gamemodeName;
			this.GameLengthInSeconds = gameLengthInSeconds;
			this.MinimumPlayers = minimumNumberOfPlayers;

			Debug.WriteLine("^1Message from BaseGamemodeSthv. Triggered by " + gamemodeName + ".");
			sthvLobbyManager.setAllActiveToWaiting();


			#region test
			Debug.WriteLine("^1Invariant: all players should be inactive or waiting. ");

			//#remove inactive from list and handle else
			foreach (var p in sthvLobbyManager.GetPlayersOfState(playerState.alive, playerState.inactive, playerState.ready, playerState.dead))
			{
				Debug.WriteLine(p.player.Name + " is in state " + p.State.ToString());
			}
			Debug.WriteLine("0b2");
			Debug.WriteLine("\n\n^7");
			#endregion
		}

		#region manager_methods
		//used in server.cs for now



		public async Task AwaitStartConditions()
		{

		}

		/// <summary>
		/// Starts the gamemode! 
		/// </summary>
		public async Task<(string, string)> Run()
		{
			int playerCount = 0;
			while (true)
			{
				//alive and dead players are "ready" for next hunt since it means they're done loading
				playerCount = sthvLobbyManager.GetPlayersOfState(playerState.ready, playerState.alive, playerState.dead).Count;
				if (playerCount > this.MinimumPlayers || (Server.TestMode && playerCount == 1))
				{
					break;
				}
				else
				{
					Server.SendChatMessage("hunt", "waiting for 2 people before hunt starts", 105, 0, 225);
					Server.SendToastNotif("Waiting for 2 players before the hunt starts.", 2000);

					Debug.WriteLine("^8 Not enough players to start gamemode^7 " + playerCount);
					await Delay(5000);
				}
			}


			Debug.WriteLine($"Starting gamemode {Name} with {sthvLobbyManager.GetPlayersOfState(playerState.ready).Count} players.");
			
			timeSecondsSinceRoundStart = 0;
			uint accuracy = 1; //second
			Debug.WriteLine(TimedEventsList.Count + "events in TimedEventList.");
			foreach (var i in TimedEventsList)
			{
				Debug.WriteLine($"key: {i.Key} value: {i.Value}");
			}
			isGameloopActive = true;

			//game loop
			while (GameLengthInSeconds - timeSecondsSinceRoundStart > 0 && String.IsNullOrEmpty(sthvLobbyManager.winnerTeamAndReason.Item1))
			{
				timeleft = GameLengthInSeconds - timeSecondsSinceRoundStart;
				Action action;
				if (TimedEventsList.TryGetValue(timeSecondsSinceRoundStart, out action))
				{
					Debug.WriteLine("^2Running TimedEvent at time: " + timeSecondsSinceRoundStart + "^7"); ;
					action.Invoke();
					TimedEventsList.Remove(timeSecondsSinceRoundStart);
				}
				if ((timeleft % 10) == 0)
				{
					Debug.WriteLine($"timeleft: {timeleft}");
					TriggerClientEvent("sth:starttimer", timeleft);
				}

				await Delay((int)accuracy * 1000);
				timeSecondsSinceRoundStart += 1;
			}

			/**
			 * Clean up
			 */
			Finalizer.Invoke(); //from AddFinalizerEvent

			//remove player teams
			foreach( var p in sthvLobbyManager.GetAllPlayers())
			{
				p.teamname = null;
			}

			isGameloopActive = false;

			Debug.WriteLine("Ending run task");

			var winners = sthvLobbyManager.winnerTeamAndReason;
			sthvLobbyManager.winnerTeamAndReason = (null, null);

			if (winners == (null, null))
			{
				return ("runner", "time ran out");
			}
			else return winners;
		}
		public void endGamemode(string reason)
		{
			if (String.IsNullOrWhiteSpace(endModeReason))
			{
				endMode = true;
				endModeReason = reason;
			}
		}

		#endregion


		#region gamemode_methods
		//methods the gamemode must implement.

		public abstract void test();

		/// <summary>
		/// Used by BaseGamemodeSthv to create teams. Not to be used by gamemodes - gamemodes must set teams via SetTeams. 
		/// </summary>
		/// <param name="players">Custom list of players. Only used by unit tests, set to NULL when used in gamemodes.</param>
		/// <param name="teams">A collection of teams and the associated properties.</param>
		/// <returns></returns>
		public string CreateTeams(List<SthvPlayer> players = null, params sthvGamemodeTeam[] teams)
		{
			if (teams.Length == 0) throw new Exception("Empty teams array passed to CreateTeams. Cannot create a gamemode with no teams.");
			if (teams == null) throw new Exception("Null passed to CreateTeams. SetTeams was likely implemented incorrectly.");

			if (players == null)
			{
				players = sthvLobbyManager.GetPlayersOfState(playerState.ready);
			}
			if (Name == "ClassicHunt" && GamemodeConfig.huntNextRunnerServerId != null)
			{
				foreach (var p in players)
				{
					int numOfRunnersAssigned = 0;
					if (p.player.Handle == GamemodeConfig.huntNextRunnerServerId)
					{
						numOfRunnersAssigned++;
						p.teamname = "runner";
						Debug.WriteLine(p.Name + " assigned runner as assigned by AdminMenu");
					}
					else
					{
						p.teamname = "hunter";
					}

					if (numOfRunnersAssigned != 1)
					{
						log("[BaseGamemode] numOfRunnersAssigned was " + numOfRunnersAssigned + " instead of 1. Maybe the assigned player [ServerId: " + GamemodeConfig.huntNextRunnerServerId + "] left.");
						//doesn't return so the teams are assigned randomly like they normally would.
					}
					else
					{
						return "teams assigned using GamemodeConfig.huntNextRunnerServerId";
					}
				}

			}
			//shuffle players before picking teams.
			var random = new Random();
			players = players.OrderBy(x => random.Next()).ToList();

			int maxIndex = teams.Length - 1;
			List<int> unallowed = new List<int>(maxIndex + 1);
			int currentIndex = 0;

			//assign players to teams
			foreach (SthvPlayer sthvPlayer in players) //sthv guarantees all players are inactive or waiting at gamemode start.
			{
				for (; ; )
				{
					if (teams[currentIndex].MaximumPlayers == teams[currentIndex].TeamPlayers.Count)
					{
						unallowed.Add(currentIndex);
						currentIndex++;
						if (currentIndex > maxIndex) currentIndex = 0;
					}
					else
					{
						teams[currentIndex].TeamPlayers.Add(sthvPlayer);
						sthvPlayer.teamname = (teams[currentIndex].Name);
						break;
					}
				}
			}
			gamemodeTeams = teams;

			string output = "";
			foreach (var t in teams)
			{
				foreach (var sthvPlayer in t.TeamPlayers)
				{
					output += (sthvPlayer.Name + " is in team " + t.Name + ".\n");
				}
			}
			return output;
		}

		/// <summary>
		///	Used to form the timeline of the gamemode.
		///	The action will be triggered after the specified time passes during the gamemode.
		/// </summary>
		/// <param name="seconds">Time after start of gamemode when action will occur.
		/// Throws exception (error) if given time exceeds GameLengthInSeconds set in constructor </param>
		/// <param name="action"></param>
		public void AddTimeEvent(uint seconds, Action action)
		{
			if (action == null) throw new Exception("Action added to AddTimeEvent cannot be null. Exception triggered at " + seconds + " seconds.");
			if (seconds > GameLengthInSeconds)
			{
				throw new Exception("Gamemode TimeEvent added at time: " + seconds + "s. Time cannot be greater than gameLengthInSeconds (set in constructor): " + GameLengthInSeconds + "s");
			}
			TimedEventsList.Add(seconds, action);
		}

		/// <summary>.
		///	The action will be triggered at the end of the gamemode. Should be used to clean up and reset client state.
		/// </summary
		/// <param name="action"></param>
		public void AddFinalizerEvent(Action action)
		{
			Finalizer = action;
		}

		/// <summary>
		/// Triggers event for all memebers of a team.
		/// </summary>
		/// <param name="team">Target team</param>
		/// <param name="eventName">Name of event</param>
		/// <param name="args">event arguments</param>
		public void sthvTriggerClientEventForTeam(sthvGamemodeTeam team, string eventName, params object[] args)
		{
			foreach (var sthvPlayer in team.TeamPlayers)
			{
				sthvPlayer.player.TriggerEvent("_" + Name + "_" + eventName, args);
			}
		}
		public void sthvTriggerClientEvent(Player p, string eventName, params object[] args)
		{
			p.TriggerEvent("_" + Name + "_" + eventName, args);
		}
		internal void log(string i) { Debug.WriteLine("^3[" + Name + "] " + i + "^7"); }

		#endregion

	}
	public class sthvGamemodeTeam
	{
		/// <summary>
		///	Name of team. Must be unique. (is used as key). 
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Game ends if size of the team drops below MinimumPlayers
		/// </summary>
		public int MinimumPlayers { get; set; }
		/// <summary>
		/// Maximum number of players a team can have. 
		/// Set negative value for unlimited players.
		/// </summary>
		public int MaximumPlayers { get; set; } = -1;
		/// <summary>
		/// A heigher weight makes player more likely to join this team. Defaults to 1. 
		/// </summary>
		public float PlayerSelectionWeight { get; set; } = 1;

		/// <summary>
		/// not to be used by Gamemode
		/// </summary>
		public List<SthvPlayer> TeamPlayers = new List<SthvPlayer>();

	}

	public class sthvGamemodeInfo
	{
		public int playArea;

	}
}
