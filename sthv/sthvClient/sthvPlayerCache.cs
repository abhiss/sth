using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
namespace sthv
{
	class sthvPlayerCache : BaseScript
	{

		static public bool isAlreadyDead { get; set; } = true;
		public int ServerId { get; set; }
		static public bool isHuntActive { get; set; }
		static public int playerid = Game.Player.Handle;
		static public int playerpedid = Game.Player.Character.Handle;
		Dictionary<string, int> playersPing = new Dictionary<string, int>();//key is serverid

		public sthvPlayerCache()
		{
			Debug.WriteLine($"isAlreadyDead: {isAlreadyDead}");
			Tick += CheckIfDead;
			Tick += ScoreboardUpdater;
			ServerId = Game.Player.ServerId;

		}
		
		async Task ScoreboardUpdater()
		{
			foreach(Player p in Players)
			{
				Debug.WriteLine($"{p.ServerId} {p.Name} {p.IsAlive} {(sthvClient.client.RunnerHandle == p.ServerId)}");
				API.SendNuiMessage(JsonConvert.SerializeObject(new sthv.NuiModels.NuiEventModel {
					EventName = "hunt.testNuiEvent",
					EventData = new sthv.NuiModels.Player {
						alive = p.IsAlive,
						name = p.Name,
						runner = (sthvClient.client.RunnerHandle == p.ServerId),
						serverid = p.ServerId
					}
				}));

			}
			await Delay(2000);
		}
		async Task CheckIfDead()
		{

			if (Game.PlayerPed.IsDead && !isAlreadyDead)
			{

				Debug.WriteLine("you are now just dead! :D");
				TriggerServerEvent("sthv:playerJustDead"); //for killfeed?
				TriggerEvent("sthv:updateAlive", true);
				isAlreadyDead = true;

			}
			else if (Game.PlayerPed.IsAlive && isAlreadyDead) //is alive was dead
			{
				TriggerServerEvent("sthv:playerJustAlive");
				TriggerEvent("sthv:updateAlive", true);

				isAlreadyDead = false;
			}

			await Delay(100);

		}
	}
}
