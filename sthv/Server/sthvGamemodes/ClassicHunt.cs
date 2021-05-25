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
		string runnerServerId = null;
		Shared.sthvMapModel map = Shared.sthvMaps.Maps[2];
		const string TRunner = "runner";
		const string THunter = "hunter";

		int currentmapid = 0;

		internal ClassicHunt() : base(gamemodeName: "ClassicHunt", gameLengthInSeconds: 24 * 60, minimumNumberOfPlayers: 1)
		{

			AddTimeEvent(0, new Action(async () =>
			{
				//pick random playarea 
				Random r = new Random();
				int playareaindex = r.Next(0, Shared.sthvMaps.Maps.Length);
				Server.currentMap = Shared.sthvMaps.Maps[playareaindex];
				map = Shared.sthvMaps.Maps[playareaindex];
				Debug.WriteLine("current map index: " + playareaindex);
				currentmapid = playareaindex;
				TriggerClientEvent("sthv:sendChosenMap", playareaindex);

				runner = sthvLobbyManager.GetPlayersInTeam(TRunner)[0];
				if (sthvLobbyManager.GetPlayersInTeam("runner").Count != 1) throw new Exception("Unexpected number of runners were assigned in ClassicHunt");
				runnerServerId = runner.player.Handle;

				log($"^2runner handle is now: {runnerServerId}^7");
				TriggerClientEvent("sth:updateRunnerHandle", runnerServerId);
				Server.SendChatMessage("^2HUNT", $"Runner is:{runner.Name}", 255, 255, 255);
				Server.SendToastNotif($"Hunt starting with runner: {runner.Name}", 3000);

				await Delay(100);

				runner.Spawn(map.RunnerSpawn, true, playerState.alive);


				//offer hunters to opt into runner ?
				TriggerClientEvent("removeveh");
				await Delay(500);

				runner.player.TriggerEvent("sthv:spawnhuntercars", currentmapid);
				runner.player.TriggerEvent("sthv:spectate", false);
				Server.refreshscoreboard();
			}));
			AddTimeEvent(5, new Action(() =>
			{
				log("spawning hunters after 5 seconds.");
				var hunters = sthvLobbyManager.GetPlayersInTeam(THunter);
				foreach (var h in hunters)
				{
					h.Spawn(map.HunterSpawn, false, playerState.alive);
				}

			}));
			AddTimeEvent(30, new Action(() =>
			{
				log("Giving weapons after 30 seconds");

				runner.player.TriggerEvent("sth:updateRunnerHandle", runnerServerId); //incase runner has wrong clothes

				TriggerClientEvent("sth:setguns", true);
				Server.SendChatMessage("^5HUNT", "You now have guns");
				Server.SendToastNotif("You now have weapons!");
				Server.SendToastNotif("You now have weapons!");
			}));

			AddFinalizerEvent(new Action(() =>
			{
				TriggerClientEvent("sth:updateRunnerHandle", -1);
				foreach( var p in sthvLobbyManager.GetPlayersOfState(playerState.alive, playerState.ready))
				{
				p.Spawn(map.RunnerSpawn, true, playerState.ready);
				}
			}));

		}
		public override void test()
		{
			Debug.WriteLine("Here is a test message!!");
		}
		public override sthvGamemodeTeam[] SetTeams()
		{
			sthvGamemodeTeam[] teams = {
				new sthvGamemodeTeam { Name = TRunner, MaximumPlayers = 1, MinimumPlayers = 1 },
				new sthvGamemodeTeam { Name = THunter, MaximumPlayers = -1, MinimumPlayers = 0 } };
			return teams;
		}

		[EventHandler("gamemode::player_killed")]
		void playerKilledHandler(string killerLicense, string killedLicense)
		{
			//killerLicense is null when there's no killer. Player died from suicide or natural causes.
			if (killerLicense == null)
			{
				return;
			}
			var killer = sthvLobbyManager.getPlayerByLicense(killerLicense);
			var killed = sthvLobbyManager.getPlayerByLicense(killedLicense);

			//friendly fire
			if (killer.teamname == killed.teamname)
			{
				killer.player.TriggerEvent("sthv:kill"); //kills the teamkiller
				Server.SendChatMessage("", $"^5{killer.Name} was killed by Karma and {killed.Name} respawned.");

				if (killed.teamname == TRunner) killed.Spawn(map.RunnerSpawn, true, playerState.alive); //spawns killed player at spawn location
				else if (killed.teamname == THunter) killed.Spawn(map.HunterSpawn, false, playerState.alive);
				else log("Killed isn't a runner or hunter!?");
			}
		}
	}
}
