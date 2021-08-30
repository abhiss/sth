using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core.Native;
using CitizenFX.Core;

namespace sthv.Gamemodes
{
	class ClassicHunt : BaseGamemode
	{
		static Vector2 playAreaCenter { get; set; } = new Vector2(100f, -1740f);
		static float Radius { get; set; }//570
		static Blip playarea = new Blip(-1);
		

		public ClassicHunt() : base(Shared.Gamemode.InverseTag)
		{
			Debug.WriteLine("START INVERSE TAG");
		
			base.AddEventHandler("sthv:sendChosenMap", new Action<int>((mapNumber)=>{
				Debug.WriteLine("SetMap:: " + mapNumber);

				client.CurrentMap = Shared.sthvMaps.Maps[mapNumber];
				SetPlayarea(client.CurrentMap.Radius, client.CurrentMap.AreaCenter.X, client.CurrentMap.AreaCenter.Y);

			}));
            base.AddEventHandler("sth:setcops", new Action<bool>((isCopsEnabled)=>{
				if (isCopsEnabled)
				{
					API.SetMaxWantedLevel(5);
				}
				else
				{
					API.SetMaxWantedLevel(0);
				}
				Debug.WriteLine("Set cops: " + isCopsEnabled);
				
			}));
			// base.AddEventHandler("", new Action(()=>{
			// }));

            base.AddTick(checkOutOfMap);
		}
	

		void SetPlayarea(float radius, float x, float y)
		{
			Debug.WriteLine("!!!!!Radius " + radius + " x " + x + " y " + y);
			playarea.Delete();
			playAreaCenter = new Vector2(x, y);
			Radius = radius;

			playarea = new Blip(API.AddBlipForRadius(x, y, 130, radius));
			playarea.Color = BlipColor.Blue;
			playarea.Alpha = 60;
		}

		async Task checkOutOfMap()
        {
			float distance = Vector2.Distance(playAreaCenter, new Vector2(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y)); //get horizontal distance between player and playAreaCenter
			if ((sthv.sthvPlayerCache.isHuntActive) && (Game.PlayerPed.IsAlive) && (distance > Radius) && (distance != 0))
			{
				API.ApplyDamageToPed(API.PlayerPedId(), 8, true);
				client.SendChatMessage("WARNING:", "OUT OF PLAY AREA", 255, 0, 0);
			}
			else
			{
				API.SetPedIsDrunk(sthvPlayerCache.playerpedid, false);
			}
			await BaseScript.Delay(1000);
		}
	}
}
