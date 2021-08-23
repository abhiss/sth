using System;
using Xunit;
using CitizenFX.Core;
using sthvServer;

using sthvServer.sthvGamemodes;
using System.Collections.Generic;

namespace sthvServerTests
{
	public class UnitTest1
	{
		//[Fact]
		//public void TestBaseGamemodeCreateTeams()
		//{
		//	BaseGamemodeSthv game = new ClassicHunt();
		//	List<SthvPlayer> players = new List<SthvPlayer>();

		//	for (int i = 0; i < 10; i++)
		//	{
		//		SthvPlayer p = new SthvPlayer(null);
		//		p.State = playerState.ready;
		//		p.Name = "Player" + i;
		//		players.Add(p);
		//	}

		//	string output = game.CreateTeams(players, 
		//					new sthvGamemodeTeam { MaximumPlayers = 1, MinimumPlayers = 1, Name = "runner" },
		//					new sthvGamemodeTeam { MaximumPlayers = 99, MinimumPlayers = 1, Name = "hunter" });
		//	Console.WriteLine(output);
		//	Assert.NotNull(output);
		//}
	}
}
