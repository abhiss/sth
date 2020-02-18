using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core.Native;
using CitizenFX.Core;
using Newtonsoft.Json;

namespace sthv
{
	class sthvSpectate : BaseScript //shows runner on map sometimes
	{
		public bool shouldSpectateOnDeath { get; set; } = true;
		public int RunnerServerId { get; set; }
		int _handleOfSpectatedPlayer = 0;
		private bool isSpectating = false;

		public sthvSpectate()
		{
			shouldSpectateOnDeath = true;
			Tick += OnTick;
			Tick += updateNuiOnSpectateStatus;

			EventHandlers["sthv:spectate"] += new Action<bool>(shouldSpectate => { 
				if (!shouldSpectate) {
					API.NetworkSetInSpectatorMode(false, Game.PlayerPed.Handle);
					Debug.WriteLine("leaving spectator mode");
					isSpectating = false;
				} 
			});

		}


		async Task updateNuiOnSpectateStatus()
		{
			try
			{
				if (sthvPlayerCache.isAlreadyDead && isSpectating && (_handleOfSpectatedPlayer > 0))
				{

					API.SendNuiMessage(JsonConvert.SerializeObject(new sthv.NuiModels.NuiEventModel
					{
						EventName = "sthvui:spectatorinfo",
						EventData = new
						{
							nameOfSpectatedPlayer = sthv.client.GetPlayerFromServerId(_handleOfSpectatedPlayer, Players).Name
						}
					}));
				}
				else
				{

					API.SendNuiMessage(JsonConvert.SerializeObject(new sthv.NuiModels.NuiEventModel
					{
						EventName = "sthvui:spectatorinfo",
						EventData = new
						{
							nameOfSpectatedPlayer = ""
						}
					}));
				}			
			}
			catch(Exception ex)
			{
				Debug.WriteLine($"Exception thrown in sthvSpectate.updateNuiOnSpectateStatus: {ex}");
			}
			await Delay(1000);
		}
		async Task OnTick()
		{
			if (Game.PlayerPed.IsDead && shouldSpectateOnDeath)
			{
				List<Player> runnerPeds = new List<Player>();
				foreach(Player p in Players)
				{

					if (p.ServerId != RunnerServerId && (p.IsAlive))
					{
						runnerPeds.Add(p);
					}
				}
				if (runnerPeds.Count < 1)
				{
					Debug.WriteLine("^1Noone to spectate, trying to spectate dead people^7");
					foreach(Player p in Players)
					{
						if((p.ServerId != RunnerServerId)){
							runnerPeds.Add(p);
						}
					}
				}
				Random i = new Random();
				int runnerToSpectate = i.Next(runnerPeds.Count);
				API.NetworkSetInSpectatorMode(true, runnerPeds[runnerToSpectate].Character.Handle);
				Debug.WriteLine($"^3You are spectating {runnerPeds[runnerToSpectate].Name}");
				isSpectating = true;
				_handleOfSpectatedPlayer = runnerPeds[runnerToSpectate].ServerId;

			}

			await Delay(10000);
		}
	}
}
