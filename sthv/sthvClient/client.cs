using System;
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
		string RunnerLicense = "01770902dc02d7b6a2183071684a7582fec278fa";
		string mapmanagerRes;
		bool isAbhi;
		public client()
		{
			//test 
			EventHandlers["onClientMapStart"] += new Action<string>(onPlayerLoaded); // event from mapmanager_cliend.lua line 47
			 
			var playArea = new sthv.sthvPlayArea();
			var rules = new sthv.sthvRules();

			Tick += rules.AutoBrakeLight;
			Tick += playArea.OnTickPlayArea;
			
			EventHandlers["sth:spawnall"] += new Action(Respawn);
			EventHandlers["sth:resetrespawncounter"] += new Action(ResetRespawnCounter);
			EventHandlers["sth:returnlicense"] += new Action<string>(ReceivedLicense); //gets license from server
			EventHandlers["sth:licenseStored"] += new Action(Respawn); //when client knows license
			//because client must know license before spawning

			API.RegisterCommand("license", new Action<int, List<object>, string>((src, args, raw) =>
			{
				Debug.WriteLine(License);
			}), false);
			API.RegisterCommand("spawn", new Action<int, List<object>, string>((src, args, raw) =>
			{
				Respawn();
			}), false);
			API.RegisterCommand("checkrunner", new Action<int, List<object>, string>((src, args, raw) =>
			{
				if (RunnerLicense.Equals(License))
				{
					isRunner = true;
					Debug.WriteLine("Runner= ^2true^7");
					SendChatMessage("server:", "youre a runner", 255, 255, 255);
				}

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
		 

		void onPlayerLoaded(string res) // res from mapmanager_cliend.lua line 47
		{
			TriggerServerEvent("NeedLicense");	//asks server for license, ends 
			mapmanagerRes = res;
			Debug.WriteLine($"^2iiiiiiiiiiiiiii mapmanagerRes = {mapmanagerRes}");
		}

		void ReceivedLicense(string license)	//gets license from server
		{
			Debug.WriteLine(license);
			License = license;
			TriggerEvent("sth:licenseStored"); //when license stored, to prevent spawning without client storing license
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
