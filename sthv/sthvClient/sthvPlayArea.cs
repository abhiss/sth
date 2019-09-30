using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace sthvClient
{
	class sthvPlayArea
	{
		Vector2 playAreaCenter { get; set; } = new Vector2(100f, -1740f);
		float radius { get; set; } = 570f; //570

		Blip blippy = null;

		public sthvPlayArea()
		{
			
			int playArea = API.AddBlipForRadius(playAreaCenter.X, playAreaCenter.Y, 130, radius);
			API.SetBlipAlpha(playArea, 90);
			API.SetBlipColour(playArea, 30);
			//API.RemoveBlip(ref playArea);
			Blip blippy = new Blip(playArea);
			CitizenFX.Core.Debug.WriteLine("GameAreaManager initialized");
			
			API.RegisterCommand("remblip", new Action<int, List<object>, string>((src, args, raw) =>
			{
				//Vector3 pos = Game.PlayerPed.Position;
				//Debug.WriteLine($"{pos.X},{pos.Y},{pos.Z},");
				API.RemoveBlip(ref playArea);
			}), false);
		}
		public async Task OnTickPlayArea()
		{
			await GetDistance();
		}
		public async Task GetDistance()
		{
			float distance = Vector2.Distance(playAreaCenter, new Vector2(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y)); //get horizontal distance between player and playAreaCenter
			if ((sthv.sthvPlayerCache.isHuntActive) && (Game.PlayerPed.IsAlive) && (distance > radius) && (distance != 0))
			{
				API.ApplyDamageToPed(API.PlayerPedId(), 5, true);
				sthvClient.client.SendChatMessage("WARNING:", "OUT OF PLAY AREA", 255, 0, 0);
			}
			//Debug.WriteLine($"1");
			await BaseScript.Delay(1000);
		}
		void removePlayArea()
		{

			blippy.Delete();
		}


	}
}
