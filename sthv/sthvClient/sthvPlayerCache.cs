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
		bool isInHeli = false;
		public sthvPlayerCache()
		{
			Debug.WriteLine($"isAlreadyDead: {isAlreadyDead}");
			Tick += CheckHeliStatus;
			ServerId = Game.Player.ServerId;


			EventHandlers["sthv:refreshsb"] += new Action<string>((string playersinheliserverid)
			=>
			{
				Debug.WriteLine("RECEVED REFRESH SCOREBOARD EVENT");
				var sb_playerlist = JsonConvert.DeserializeObject<List<Shared.ScoreboardInfoPlayer>>(playersinheliserverid);

				List<NuiModels.Player> playerInfoList = new List<NuiModels.Player>();
				foreach (var p in sb_playerlist)
				{
					playerInfoList.Add(
						new NuiModels.Player
						{
							alive = p.is_alive,
							name = Players[int.Parse(p.serverid)].Name,
							runner = p.is_runner,
							serverid = int.Parse(p.serverid),
							isinheli = p.is_in_helicopter
						}
					);
				}
				API.SendNuiMessage(JsonConvert.SerializeObject(new sthv.NuiModels.NuiEventModel
				{
					EventName = "sthv:updatesb",
					EventData = playerInfoList
				}));
				Debug.WriteLine("^5updated sb");

			});
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
