using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace sthv
{
	public class sthvPlayerList : IEnumerable<Player>
	{
		public const int MaxPlayers = 256;

		public IEnumerator<Player> GetEnumerator()
		{
			for (var i = 0; i < MaxPlayers; i++)
			{
				if (API.NetworkIsPlayerActive(i))
				{
					yield return new Player(i);
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public Player this[int netId] => this.FirstOrDefault(player => player.ServerId == netId);

		public Player this[string name] => this.FirstOrDefault(player => player.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

		public Player GetNearestPlayer(){
			var pedpos = Game.PlayerPed.Position;
			Player nearestPlayer = this.First();
			float leastDistance = World.GetDistance(pedpos, nearestPlayer.Character.Position);
			foreach(var p in this){
				var newDistance = World.GetDistance(pedpos, p.Character.Position);
				if(newDistance < leastDistance){
					nearestPlayer = p;
					leastDistance = newDistance;
				}
			}
			return nearestPlayer;
		}
	}

}