#if DEBUG

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;


namespace sthvServer.sthvGamemodes
{
	class MapMaker : BaseGamemodeSthv
	{
		private List<CarInfoModel> CarInfos = new List<CarInfoModel>();

		public MapMaker() : base(Shared.Gamemode.MapMaker, gameLengthInSeconds: 60 * 250, minimumNumberOfPlayers: 1, numberOfTeams: 1)
		{}

		public override void CreateEvents()
		{
			AddTimeEvent(0, new Action(() =>
			{

			}));
		}

		[EventHandler("sthv:MapMaker:save_car")]
		void save_car_handler(string tag, string team, Vector3 position, float heading, int hash)
		{
			Debug.WriteLine("wdasdwadsdw");
			CarInfos.Add(new CarInfoModel { pos = position, heading = heading , tag = tag, team = team, ModelHash = hash});	
		}

		[EventHandler("sthv:MapMaker:save_car_final")]
		void save_car_final_handler()
		{
			var json = JsonConvert.SerializeObject(CarInfos);
			Debug.WriteLine(json); 
			File.WriteAllText(API.GetResourcePath(API.GetCurrentResourceName()) + "_carinfos.json", json);
			CarInfos.Clear();
		}
	}
}

struct CarInfoModel
{
	public string tag { get; set; }
	public float heading { get; set; }
	public string team{ get; set; }
	public Vector3 pos { get; set; }
	public int ModelHash { get; set; }
}

#endif