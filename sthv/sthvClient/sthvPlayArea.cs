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

		Blip blippy = null;
		static Blip playarea = new Blip(-1);

		public sthvPlayArea()
		{

			playarea = new Blip(API.AddBlipForRadius(playAreaCenter.X, playAreaCenter.Y, 130, Radius));
			playarea.Color = BlipColor.Blue;
			playarea.Alpha = 60;
		}
		public async Task OnTickPlayArea()
		{
			await GetDistance();
		}
		public async Task GetDistance()
		{
			float distance = Vector2.Distance(playAreaCenter, new Vector2(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y)); //get horizontal distance between player and playAreaCenter
			if ((sthv.sthvPlayerCache.isHuntActive) && (Game.PlayerPed.IsAlive) && (distance > Radius) && (distance != 0))
			{
				API.ApplyDamageToPed(API.PlayerPedId(), 5, true);
				sthv.client.SendChatMessage("WARNING:", "OUT OF PLAY AREA", 255, 0, 0);
			}
			//Debug.WriteLine($"1");
			await BaseScript.Delay(1000);
		}
		public static void SetPlayarea(float radius, float x, float y)
		{
			playarea.Delete();
			playAreaCenter = new Vector2(x, y);
			Radius = radius;

			playarea = new Blip(API.AddBlipForRadius(x, y, 130, radius));
			playarea.Color = BlipColor.Blue;
			playarea.Alpha = 60;
		}
		void removePlayArea()
		{

			blippy.Delete();
		}


	}
}
