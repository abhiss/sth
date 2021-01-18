using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
namespace sthvServer
{
	abstract internal class BaseGamemodeSthv : BaseScript
	{
		private int GameLengthInSeconds { get; set; }
		private protected bool endMode = false; //used to end mode prematurely. Can be called either by gamemode manager (server.cs) or the gamemode.
		private protected string endModeReason = null;
		public int MinimumPlayers { get; }
		private Dictionary<uint, Action> TimedEventsList = new Dictionary<uint, Action>();
		public string Name { get; }
		private sthvGamemodeTeam[] gamemodeTeams;
		
		internal BaseGamemodeSthv(string gamemodeName, int gameLengthInSeconds = 0, int minimumNumberOfPlayers = 2, int numberOfTeams = 2)
		{
			Name = gamemodeName;
			Debug.WriteLine("^1Message from BaseGamemodeSthv. Triggered by " + gamemodeName);
		}

		#region abstract_methods
		//methods the gamemode must implement.

		/// <summary>
		/// </summary>
		/// <returns></returns>
		//public abstract Task<string[]> Main(); 

		public abstract void test();
		#endregion

		internal void CreateTeams(sthvGamemodeTeam[] teams)
		{
			if (teams.Length == 0) throw new Exception("Empty teams array passed to CreateTeams. Cannot create a gamemode with no teams.");
			if (teams == null) throw new Exception("Null passed to CreateTeams.");

			int maxIndex = teams.Length - 1;
			List<int> unallowed = new List<int>(maxIndex + 1);
			int currentIndex = 0;

			//List<sthvGamemodeTeam> teamsL = new List<sthvGamemodeTeam>(teams);
			foreach (SthvPlayer sthvPlayer in sthvLobbyManager.GetPlayersOfState(SthvPlayer.stateEnum.waiting)) //sthv guarantees all players are inactive or waiting at gamemode start.
			{
				for (;;)
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
			foreach(var t in teams)
			{
				foreach(var sthvPlayer in t.TeamPlayers)
				{
					Debug.WriteLine(sthvPlayer.player.Name + " is in team " + t.Name + ".");
				}
			}
		}
		
		#region methods for gamemode manager
		//used in server.cs for now
		/// <summary>
		/// Runs the gamemode
		/// </summary>
		public async Task<string[]> Run()
		{
			//if(gamemodeTeams == null)
			//{
			//	throw new NullReferenceException("GamemodeTeams not set before calling run");
			//}
			//int time = 0;
			//const int accuracy = 5;
			//while 

			string[] winners = { "sd", "sd" };
			return winners;
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

		#region methods for gamemodes
		/// <summary>
		///	Used to form the timeline of the gamemode.
		///	The action will be triggered after the specified time passes during the gamemode.
		/// </summary>
		/// <param name="seconds">Time after start of gamemode when action will occur.
		/// Throws exception (error) if given time exceeds GameLengthInSeconds set in constructor </param>
		/// <param name="action"></param>
		void AddTimeEvent(uint seconds, Action action)
		{
			if (action == null) throw new Exception("Action cannot be null. Exception triggered at " + seconds + " seconds.");
			if (seconds > GameLengthInSeconds)
			{
				throw new Exception("Gamemode TimeEvent added at time: " + seconds + "s. Time cannot be greater than gameLengthInSeconds (set in constructor): " + GameLengthInSeconds + "s");
			}
			TimedEventsList.Add(seconds, action);
		}
		void sthvTriggerClientEvent(sthvGamemodeTeam team, string eventName, params object[] args)
		{
			foreach (var sthvPlayer in team.TeamPlayers)
			{
				sthvPlayer.player.TriggerEvent("_" + Name + "_" + eventName, args);
			}
		}
		void sthvTriggerClientEvent(Player p, string eventName, params object[] args)
		{
			p.TriggerEvent("_" + Name + "_" + eventName, args);
		}
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
}
