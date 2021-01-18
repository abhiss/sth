using System;
using System.Collections.Generic;
using System.Text;
using CitizenFX.Core;

namespace sthvServer
{

	//unused
	public class SthvPlayer
	{
		private stateEnum _state;
		public stateEnum State
		{
			get { return _state; }
			set
			{
				switch (value)
				{
					case stateEnum.inactive:
						_state = stateEnum.inactive;
						break;
					case stateEnum.waiting:
						_state = stateEnum.waiting;
						break;
					case stateEnum.alive:
						_state = stateEnum.alive;
						KillerNameAndLicense = ("", "");

						break;
					case stateEnum.dead:
						_state = stateEnum.dead;
						if(KillerNameAndLicense == ("",""))
						{
							Utilities.logError($"Killer name and license was not set when player {player.Name} was killed");
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

		public string teamname;

		/// <summary>
		/// FiveM Player object
		/// </summary>
		public Player player;
		public SthvPlayer(Player source)
		{
			player = source;
			State = stateEnum.inactive;
		}

		/// <summary>
		/// inactive, //still loading or hasn't logged in
		/// waiting, //not alive or dead, waiting for a game to start
		///	alive, //active during a game
		///	dead, //dead in game
		/// </summary>
		public enum stateEnum
		{
			inactive = 0, //still loading or hasn't logged in
			waiting, //not alive or dead, waiting for a game to start
			alive, //active during a game
			dead, //dead in game
		}
	}
}
