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
		Vector2 playAreaCenter { get; set; } = new Vector2(-181f, -210f);
		float radius = 9000f;

		public sthvPlayArea()
		{
			;

			int playArea = API.AddBlipForRadius(playAreaCenter.X, playAreaCenter.Y, 130, radius);
			API.SetBlipAlpha(playArea, 90);
			API.SetBlipColour(playArea, 30);
			CitizenFX.Core.Debug.WriteLine("GameAreaManager initialized");

			API.RegisterCommand("printpos", new Action<int, List<object>, string>((src, args, raw) =>
			{
				Vector3 pos = Game.PlayerPed.Position;
				Debug.WriteLine($"{pos.X},{pos.Y},{pos.Z},");

			}), false);
		}
		public async Task OnTickPlayArea()
		{
			await GetDistance();
		}
		public async Task GetDistance()
		{
			float distance = Vector2.Distance(playAreaCenter, new Vector2(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y)); //get horizontal distance between player and playAreaCenter
			if (distance > radius)
			{
				API.ApplyDamageToPed(API.PlayerPedId(), 5, true);
				sthvClient.client.SendChatMessage("WARNING:", "OUT OF PLAY AREA", 255, 0, 0);
			}
			await BaseScript.Delay(1000);
		}

	}
}
