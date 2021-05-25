using System;
using System.Collections.Generic;
using System.Text;
using CitizenFX.Core;


namespace sthvServer
{

	//unused
	public class SthvPlayer
	{
		private playerState _state;
		public playerState State
		{
			get { return _state; }
			set
			{
				switch (value)
				{
					case playerState.inactive:
						_state = playerState.inactive;
						break;
					case playerState.ready:
						_state = playerState.ready;
						break;
					case playerState.alive:
						_state = playerState.alive;
						KillerNameAndLicense = ("", "");

						break;
					case playerState.dead:
						_state = playerState.dead;
						if (KillerNameAndLicense == ("", ""))
						{
							Utilities.logError($"Killer name and license was not set when player {player.Name} was killed.");
						}
						break;
					default:
						Debug.WriteLine("An invalid value was set to SthvPlayer.State for " + player.Name);
						break;
				}
			}
		}


		public (string, string) KillerNameAndLicense { get; set; }

		/// <summary>
		/// A list of teams (team names) that the player is currently in.
		/// </summary>

		//public static List<string> Teams { get; set; } = new List<string>();

		private string _teamname;
		public string teamname {
			get { return _teamname; }
			set
			{
				Debug.WriteLine($"^3Player: {this.player.Name} assigned to team: {value}");
				_teamname = value;
				//throw new Exception("-- test");
			}
		}
		public string Name;

		/// <summary>
		/// FiveM Player object
		/// </summary>
		public Player player;
		public SthvPlayer(Player source)
		{
			player = source;
			State = playerState.inactive;
			if (source is null) { Name = ""; }
			else { this.Name = source.Name;
			}
		}

		/// <summary>
		/// Spawn player from server.
		/// </summary>
		/// <param name="location">Coordinates where player should spawn</param>
		/// <param name="player_state">State to put player. Only alive and ready really make sense.</param>
		public void Spawn(Vector4 location, bool isRunner , playerState player_state)
		{
			string pedskin = isRunner ? PedTypes.RandomRunner : PedTypes.Swat;
			this.player.TriggerEvent("sth:spawn", location, pedskin);
			sthvLobbyManager.getPlayerByLicense(player.getLicense()).State = playerState.alive;

		}
	}
	/// <summary>
	/// inactive, //still loading or hasn't logged in
	/// waiting, //not alive or dead, waiting for a game to start
	///	alive, //active during a game
	///	dead, //dead in game
	/// </summary>
	public enum playerState
	{
		inactive = 0, //still loading or doesn't have permission to play yet.
		ready, //Ready to play. Waiting for a game to start. 
		alive, //active during a game
		dead, //dead in game
	}

	public static class PedTypes
	{
		public static string Swat = "s_m_y_swat_01";
		public static string RandomRunner = "random_runner";
	}
}
