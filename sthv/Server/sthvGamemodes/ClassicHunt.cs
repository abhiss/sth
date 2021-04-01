using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
namespace sthvServer.sthvGamemodes
{
	internal class ClassicHunt : BaseGamemodeSthv
	{
		SthvPlayer runner = null;
		string runnerServerId= null; 
		internal ClassicHunt() : base(gamemodeName: "ClassicHunt", gameLengthInSeconds: 1 * 60, minimumNumberOfPlayers: 1)
		{
			
	

			AddTimeEvent(0, new Action(async () =>
			{

				log("This happens when gamemode starts");
				runner = sthvLobbyManager.GetPlayersInTeam("runner")[0];
				if (sthvLobbyManager.GetPlayersInTeam("runner").Count != 1) throw new Exception("Unexpected number of runners were assigned in ClassicHunt");
				runnerServerId = runner.player.Handle;

				log($"^2runner handle is now: {runnerServerId}^7");
				TriggerClientEvent("sth:updateRunnerHandle", runnerServerId);
				Server.SendChatMessage("^2HUNT", $"Runner is:{runner.Name}", 255, 255, 255);
				Server.SendToastNotif($"Hunt starting with runner: {runner.Name}", 3000);

				await Delay(100);
				runner.player.TriggerEvent("sth:spawn", (int)Server.spawnType.runner);
				sthvLobbyManager.MarkPlayerAlive(runner.player);

				//offer hunters to opt into runner ?
				TriggerClientEvent("removeveh");
				await Delay(500);
				runner.player.TriggerEvent("sthv:spawnhuntercars");

				runner.player.TriggerEvent("sthv:spectate", false);
				Server.refreshscoreboard();




				TriggerClientEvent("sthv:sendChosenMap", 3);
			}));
			AddTimeEvent(5, new Action(() =>
			{
				log("spawning hunters after 5 seconds.");
				var runners = sthvLobbyManager.GetPlayersInTeam("hunter");
				foreach (var r in runners)
				{
					r.player.TriggerEvent("sth:spawn", (int)Server.spawnType.hunter);
					sthvLobbyManager.MarkPlayerAlive(r.player);
				}

			}));
			AddTimeEvent(30, new Action(() =>
			{
				log("Giving weapons after 30 seconds");

				runner.player.TriggerEvent("sth:updateRunnerHandle", runnerServerId);

				TriggerClientEvent("sth:setguns", true);
				Server.SendChatMessage("^5HUNT", "You now have guns");
				Server.SendToastNotif("You now have weapons!");
				Server.SendToastNotif("You now have weapons!");
			}));
			
		}
		[Command("testgm")]
		void commandTestGm()
		{
			Debug.WriteLine("testgm triggered");
		}
		public override void test()
		{
			Debug.WriteLine("Here is a test message!!");
		}
		public override sthvGamemodeTeam[] SetTeams()
		{
			sthvGamemodeTeam[] teams = {
				new sthvGamemodeTeam { Name = "runner", MaximumPlayers = 1, MinimumPlayers = 1 },
				new sthvGamemodeTeam { Name = "hunter", MaximumPlayers = -1, MinimumPlayers = 0 } };
			return teams;
		}
	}
}
