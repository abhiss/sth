﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX;
using CitizenFX.Core;
using CitizenFX.Core.Native;



namespace sthvClient
{
	public class client : BaseScript
	{
		bool isRunner { get; set; }
		string License { get; set; }
		int respawnCount = 0;
		string RunnerLicense = "56e8e0ce67ae257ab5c4db6209ba4e0de0ea776e";

		public client()
		{
			//test 
			TriggerServerEvent("NeedLicense");


			var playArea = new sthv.sthvPlayArea();
			var rules = new sthv.sthvRules();
			Tick += rules.AutoBrakeLight;
			Tick += playArea.OnTickPlayArea;
			
			EventHandlers["sth:spawnall"] += new Action(Respawn);
			EventHandlers["sth:resetrespawncounter"] += new Action(ResetRespawnCounter);
			EventHandlers["sth:returnlicense"] += new Action<string>(ReceivedLicense);


			
			API.RegisterCommand("license", new Action<int, List<object>, string>((src, args, raw) =>
			{
				//Debug.WriteLine("triggered: NeedLicense");
				TriggerServerEvent("NeedLicense");
				Debug.WriteLine(License);
			}), false);
			API.RegisterCommand("spawn", new Action<int, List<object>, string>((src, args, raw) =>
			{
				Respawn();
			}), false);
			API.RegisterCommand("runner", new Action<int, List<object>, string>((src, args, raw) =>
			{
				if (RunnerLicense.Equals(License))
				{
					isRunner = true;
					Debug.WriteLine("\nn\nn\nn\nnRunner= true");
				}
				Debug.WriteLine($"isRunner:{isRunner}\n runnerLicense: {RunnerLicense}, \n mylicense: {License}");

			}), false);
			API.RegisterCommand("giveguns", new Action<int, List<object>, string>((src, args, raw) =>
			{	if (respawnCount < 99) //first spawn is the load in
				{
					Game.PlayerPed.Weapons.Give(WeaponHash.CombatPistol, 225, false, true);
					Game.PlayerPed.Weapons.Give(WeaponHash.PumpShotgun, 225, false, true);
					if (isRunner)
					{
						Debug.WriteLine("gaverunnerguns");
						Game.PlayerPed.Weapons.Give(WeaponHash.BullpupRifleMk2, 1000, false, true);
						Game.PlayerPed.Weapons.Give(WeaponHash.SpecialCarbine, 1000, false, true);

					}
				}
				else
				{
					return;
				}
				
			}), false);

			//Client.Managers.Spawn.SpawnPlayer("S_M_Y_MARINE_01", 0.916756f, 528.485f, 174.628f, 0f);
		}
		 
		void ReceivedLicense(string ID)
		{
			Debug.WriteLine(ID);
			License = ID;
		}
		void ResetRespawnCounter()
		{
			respawnCount = 0;
		}
		void Respawn()
		{
			if (isRunner)
			{
				Client.Managers.Spawn.SpawnPlayer("A_M_Y_BEVHILLS_01", -181f, -210f, 47f, 0f);
			}
			else
			{
				Client.Managers.Spawn.SpawnPlayer("S_M_Y_MARINE_01", -181f, -210f, 47f, 0f);
			}
			//TriggerServerEvent("respawn", respawnCount);
			respawnCount++;
		}

		public static void SendChatMessage(string title, string message, int r, int g, int b)
		{
			var msg = new Dictionary<string, object>
			{
				["color"] = new[] { r, g, b },
				["args"] = new[] { title, message }
			};
			TriggerEvent("chat:addMessage", msg);
		}

	}		
}
