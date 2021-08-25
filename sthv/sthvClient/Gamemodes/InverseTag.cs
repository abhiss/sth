using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core.Native;
using CitizenFX.Core;

namespace sthv.Gamemodes
{
	class InverseTag : BaseGamemode
	{
		public InverseTag() : base(Shared.Gamemode.InverseTag)
		{

		}
		async Task CollisionDetector()
		{
			await BaseScript.Delay(1000);
			var closest_player = API.GetNearestPlayerToEntity(API.PlayerPedId());
			var player = new Player(closest_player);
			Debug.WriteLine("Closest player to me is: " + player.Name);
			API.IsEntityTouchingEntity(Game.PlayerPed.Handle, closest_player);
			
		}
	}
}
