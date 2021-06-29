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
		static public int playerpedid = Game.PlayerPed.Handle;
		static public Player runnerPlayer { get; set; } = null;
		Dictionary<string, int> playersPing = new Dictionary<string, int>();//key is serverid
		public int[] playersInHeliServerId { get; set; } = { -1 };
		bool isInHeli = false;
		public sthvPlayerCache()
		{
			Debug.WriteLine($"isAlreadyDead: {isAlreadyDead}");
			Tick += ScoreboardUpdater;
			Tick += CheckHeliStatus;
			ServerId = Game.Player.ServerId;


			EventHandlers["sthv:refreshsb"] += new Action<string>(async (string playersinheliserverid)
			=>
			{
				Debug.WriteLine("RECEVED REFRESH SCOREBOARD EVENT");
				playersInHeliServerId = JsonConvert.DeserializeObject<int[]>(playersinheliserverid);
				await ScoreboardUpdater();
				Debug.WriteLine("^5updated sb");
				Debug.WriteLine(playersInHeliServerId.ToString());
			});
		}

		public async Task ScoreboardUpdater()
		{
			List<NuiModels.Player> playerInfoList = new List<NuiModels.Player>();
			foreach (Player p in Players)
			{
				playerInfoList.Add(
					new NuiModels.Player
					{
						alive = p.IsAlive,
						name = p.Name,
						runner = (sthv.client.RunnerServerId == p.ServerId),
						serverid = p.ServerId,
						isinheli = (playersInHeliServerId.Contains(p.ServerId))
					}
				);
			}
			API.SendNuiMessage(JsonConvert.SerializeObject(new sthv.NuiModels.NuiEventModel
			{
				EventName = "sthv:updatesb",
				EventData = playerInfoList
			}));
			await Delay(10000);
		}
		async Task CheckHeliStatus()
		{
			if (!isInHeli && Game.PlayerPed.IsInFlyingVehicle)
			{
				TriggerServerEvent("sthv:isinheli", true);
				isInHeli = true;
			}
			if (isInHeli && !Game.PlayerPed.IsInFlyingVehicle)
			{
				TriggerServerEvent("sthv:isinheli", false);
				isInHeli = false;
			}
			await Delay(1000);
		}

	}
}
