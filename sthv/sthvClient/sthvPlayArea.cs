using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace sthv
{
	class sthvPlayArea
	{
		static Vector2 playAreaCenter { get; set; } = new Vector2(100f, -1740f);
		static float Radius { get; set; }//570

		static Blip playarea = new Blip(-1);

		public sthvPlayArea()
		{
			playarea = new Blip(API.AddBlipForRadius(playAreaCenter.X, playAreaCenter.Y, 130, Radius));
			playarea.Color = BlipColor.Blue;
			playarea.Alpha = 60;
		}
		[Command("playareainfo")]
		void command_playareainfo()
		{
			Debug.WriteLine(playAreaCenter.X + " " + playAreaCenter.Y);
			Debug.WriteLine($"gamemode: {client.GamemodeId} | IsHuntActive: {sthvPlayerCache.isHuntActive} | distance: {Vector2.Distance(playAreaCenter, new Vector2(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y))}");
		}

		public async Task GetDistance()
		{
			//only classic hunt uses play areas like this.
			if (client.GamemodeId != Shared.Gamemode.ClassicHunt) return;
			Debug.WriteLine("Current gamemode: " + client.GamemodeId);

			float distance = Vector2.Distance(playAreaCenter, new Vector2(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y)); //get horizontal distance between player and playAreaCenter
			if ((sthv.sthvPlayerCache.isHuntActive) && (Game.PlayerPed.IsAlive) && (distance > Radius) && (distance != 0))
			{
				API.ApplyDamageToPed(API.PlayerPedId(), 8, true);
				sthv.client.SendChatMessage("WARNING:", "OUT OF PLAY AREA", 255, 0, 0);
				API.SetPedIsDrunk(sthvPlayerCache.playerpedid, true);
			}
			else
			{
				API.SetPedIsDrunk(sthvPlayerCache.playerpedid, false);
			}
			await BaseScript.Delay(1000);
		}
		public static void SetPlayarea(float radius, float x, float y)
		{
			Debug.WriteLine("!!!!!Radius " + radius + " x " + x + " y " + y);
			playarea.Delete();
			playAreaCenter = new Vector2(x, y);
			Radius = radius;

			playarea = new Blip(API.AddBlipForRadius(x, y, 130, radius));
			playarea.Color = BlipColor.Blue;
			playarea.Alpha = 60;
		}
		public static void RemovePlayarea()
		{
			playarea.Delete();
			playarea = null;
		}
	}
}
