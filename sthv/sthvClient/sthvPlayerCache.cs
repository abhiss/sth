using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX;
using CitizenFX.Core.Native;

namespace sthv
{
	class sthvPlayerCache : BaseScript
	{
		
		public bool isAlreadyDead { get; set; }
		public int ServerId { get; set; }


		public sthvPlayerCache()
		{
			Tick += CheckIfDead;
			ServerId= Game.Player.ServerId;
			

		}
		async Task CheckIfDead()
		{

			if (Game.PlayerPed.IsDead && !isAlreadyDead)
			{

				Debug.WriteLine("you are now dead! :D");
				TriggerServerEvent("sthv:playerJustDead"); //for killfeed?
				isAlreadyDead = true;
			}
			else if (Game.PlayerPed.IsAlive && isAlreadyDead) //is alive was dead
			{
				TriggerServerEvent("sthv:playerJustAlive");
				isAlreadyDead = false;


			}

			await Delay(100);

		}
	}
}
