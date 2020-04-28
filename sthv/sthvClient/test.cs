//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using CitizenFX;
//using CitizenFX.Core;
//using CitizenFX.Core.Native;

//namespace sthv
//{
//	class test : BaseScript
//	{
//		static List<Vector4> listofcoords = new List<Vector4>();
//		public Ped MissionPed { get; set; }
//		int mpednetid = 0;
//		public bool ismissionactive { get; set; }
//		public Blip missionBlip { get; set; }
//		public test()
//		{
//			//Tick += checkmissionstuff;
//			//Tick += checkrunner2;
//			//missionBlip = new Blip(API.AddBlipForRadius(0, 0, 0, 50));
//			//missionBlip.Color = BlipColor.TrevorOrange;
//			//missionBlip.Alpha = 0;

		
//			API.RegisterCommand("log", new Action<int, List<object>, string>((src, args, raw) =>
//			{
//				Vector4[] sendarray = listofcoords.ToArray();
//				TriggerServerEvent("logCoords", sendarray);
//				foreach (Vector4 i in listofcoords)
//				{
//					Debug.WriteLine($"new Vector4({i.X}f, {i.Y}f, {i.Z}f, {i.W}f),");
//				}
//				Debug.WriteLine("command log");
//			}), false);
//			API.RegisterCommand("force", new Action<int, List<object>, string>(async (src, args, raw) => //move  mission stuff to sthvRules.cs
//			{
//				if (!ismissionactive)
//				{
//					//Debug.WriteLine(Game.Player.LastVehicle.DisplayName);
//					MissionPed = await World.CreatePed(new Model(PedHash.Autoshop01SMM), new Vector3(504, 5597, 796));
//					log("spawned ped");
//					mpednetid = MissionPed.NetworkId;
//					log(MissionPed.NetworkId.ToString());
//					MissionPed.Task.StandStill(-1);

//					API.SetEntityAsMissionEntity(MissionPed.Handle, false, true);
//					TriggerServerEvent("sthv:missionpedmade", MissionPed.NetworkId);
//					//Debug.WriteLine($"health: {MissionPed.Health}, networkid: {MissionPed.NetworkId}, entityid: {MissionPed.Handle} ");

//					API.SetBlockingOfNonTemporaryEvents(MissionPed.Handle, true);
//					API.TaskSetBlockingOfNonTemporaryEvents(MissionPed.Handle, true);
//					ismissionactive = true;

//					//
//					API.SetNetworkIdCanMigrate(MissionPed.NetworkId, false);
//					API.NetworkSetNetworkIdDynamic(MissionPed.NetworkId, false);

//					foreach (Player p in Players)
//					{
//						API.SetNetworkIdSyncToPlayer(MissionPed.NetworkId, p.Handle, true);
//					}
//				}
//				else
//				{
//					Debug.WriteLine("mission already active");
//				}


//			}), false);
//			EventHandlers["updateonmped"] += new Action<bool, int, Vector3>((bool show, int radius, Vector3 pos) =>
//			{
//				if (show)
//				{
//					missionBlip.Alpha = 70;
//					missionBlip.Position = pos;
//				}
//				else
//				{
//					missionBlip.Alpha = 0;
//				}

//			});
//			EventHandlers["sthv:updatepednetid"] += new Action<int>(netid =>
//			{
//				mpednetid = netid;
//				Debug.WriteLine("netid: " + netid);
//				if (API.NetworkDoesEntityExistWithNetworkId(netid))
//				{
//					Ped missionped = new Ped(API.NetToPed(netid));

//					Debug.WriteLine($"^1 health {missionped.Health} position = {missionped.Position.DistanceToSquared2D(Game.PlayerPed.Position)}");
//				}
//				else
//				{
//					Debug.WriteLine("entity not found");
//				}
//			});

//		}
//		static void log(string input)
//		{
//			Debug.WriteLine("test: " + input);

//		}
//		async Task checkrunner2() //copied to sthvRules.cs, unused here
//		{
//			if (mpednetid != 0)
//			{

//				if (API.NetworkDoesNetworkIdExist(mpednetid))
//				{
//					log($"network id {mpednetid} exsists");
//					int _mped = API.NetToPed(mpednetid);
//					if (API.DoesEntityExist(_mped))
//					{
//						Vector3 mpedpos = API.GetEntityCoords(_mped, false);
//						int health = API.GetEntityHealth(API.NetToPed(mpednetid));
//						log("entity exists");
//						if (API.IsEntityDead(API.NetToPed(mpednetid)))
//						{
//							TriggerServerEvent("missionpedstatus", false, health, mpedpos);
//						}
//						else
//						{
//							TriggerServerEvent("missionpedstatus", true, health, mpedpos);
//						}
//					}
//				}
//				else
//				{
//					log($"network id {mpednetid} does not exsist");
//				}

//			}
//			await Delay(1000);
//		}
//		async Task checkmissionstuff() //copied to sthvRules.cs, unused here
//		{
//			if (ismissionactive && MissionPed.IsDead)
//			{
//				if (!API.DoesEntityExist(MissionPed.Handle))
//				{
//					Debug.WriteLine($"misisonped at {MissionPed.Handle} doesn't exist");
//					if (API.NetworkDoesEntityExistWithNetworkId(mpednetid))
//					{
//						Debug.WriteLine("^1mped exists with netid");
//						MissionPed = new Ped(API.NetToPed(mpednetid));
//						Debug.WriteLine($"health: {MissionPed.Health}");
//					}
//					else
//					{
//						Debug.WriteLine("^1mped doesnt exist with netid either");
//					}
//				}
//				int killerped = API.GetPedKiller(MissionPed.Handle);
//				if (API.IsPedAPlayer(killerped))
//				{
//					Player p = new Player(API.NetworkGetPlayerIndexFromPed(killerped));
//					Debug.WriteLine(p.Name);
//				}
//				else
//				{
//					Player killer = new Player(API.GetNearestPlayerToEntity(MissionPed.Handle));
//					if (World.GetDistance(MissionPed.Position, killer.Character.Position) < 10f)
//					{
//						Debug.WriteLine($"Killer was probably {killer.Name}");
//					}
//					else
//					{
//						Debug.WriteLine("ped killer not found");
//					}
//				}



//				Debug.WriteLine("missionped died");
//				ismissionactive = false;
//			}

//			await Delay(500);
//		}
//		public static void addCoordToList()
//		{
//			if (true)//Game.PlayerPed.IsInVehicle()
//			{
//				var i = new Vector4(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, Game.PlayerPed.Heading);
//				listofcoords.Add(i);

//				Debug.WriteLine($"new Vector4({i.X}f, {i.Y}f, {i.Z}f, {i.W}f),");
//			}
//			else
//			{
//				Debug.WriteLine("not in car, position not added");
//			}
//		}

//	}
//}
