using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core.Native;
using CitizenFX.Core;


namespace sthv
{
	class sthvSpectate : BaseScript
	{
		public bool shouldSpectateOnDeath { get; set; } = true;
		public int RunnerServerId { get; set; }


		public sthvSpectate()
		{
			Tick += OnTick;
		}

		async Task OnTick()
		{
			if (Game.PlayerPed.IsDead && shouldSpectateOnDeath)
			{
				List<int> runnerPeds = new List<int>();
				foreach(Player p in Players)
				{

					if(p.ServerId != RunnerServerId)
					{
						runnerPeds.Add(p.Handle);
					}
				}
				Random i = new Random();
				int runnerToSpectate = i.Next(runnerPeds.Count);
				API.NetworkSetInSpectatorMode(true, runnerPeds[runnerToSpectate]);
				Debug.WriteLine($"^3You are spectating {runnerPeds[runnerToSpectate]}");
			}

			await Delay(10000);
		}
	}
}
