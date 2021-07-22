using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace sthv.Gamemodes
{
	class CheckpointHunt : BaseGamemode
	{

		List<Blip> ActiveBlips = new List<Blip>();
		public CheckpointHunt() : base(Shared.Gamemode.CheckpointHunt)
		{
			AddEventHandler("createcheckpoint", new Action<float, float, float>((radius, x, y) =>
				{
					Debug.WriteLine($"sth:checkpoint:createcheckpoint radius {radius}, x {x}, y {y}.");
				})
			);
		
		}
	}
}
