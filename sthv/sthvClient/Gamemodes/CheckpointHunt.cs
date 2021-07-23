using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;


namespace sthv.Gamemodes
{
	class CheckpointHunt : BaseGamemode
	{

		List<(Blip, Checkpoint, int)> CheckpointMarkers = new List<(Blip, Checkpoint, int)>();
		float Radius;
		public CheckpointHunt() : base(Shared.Gamemode.CheckpointHunt)
		{
			AddEventHandler("createcheckpoint", new Action<int, float, Vector3>((id, radius, pos) =>
			{
				Radius = radius;

				//CreateCheckpoint is incorrectly documented, it takes a diameter instead of radius.
				var check_id = API.CreateCheckpoint(45, pos.X, pos.Y, pos.Z - Game.PlayerPed.HeightAboveGround, 0, 0, 0, radius*2, 225, 0, 225, 90, 0);
				var check = new Checkpoint(check_id);

				var blip = new Blip(API.AddBlipForCoord(pos.X, pos.Y, pos.Z));
				blip.Color = BlipColor.Red;
				blip.Alpha = 80;
				blip.ShowRoute = true;

				CheckpointMarkers.Add((blip, check, id));
			}));

			AddEventHandler("removecheckpoint", new Action<int>((checkpointid) =>
			{
				var markers = CheckpointMarkers.FindAll(p => p.Item3 == checkpointid);
				foreach (var marker in markers)
				{
					Blip b = marker.Item1;
					b.Delete();

					Checkpoint c = marker.Item2;
					c.Delete();
				}
				CheckpointMarkers.RemoveAll(p => p.Item3 == checkpointid);
			}));

			

			AddTick(CheckpointChecker);
		}
		async Task CheckpointChecker()
		{
			{
				Debug.WriteLine("CHECK");
				if (client.IsRunner)
				{
					foreach (var mark in CheckpointMarkers)
					{
						var b = mark.Item1;
						var me = Game.PlayerPed;
						Debug.WriteLine("blips: " + b.Position.X + " " + b.Position.Y + " mine: " + me.Position.X + " " + me.Position.Y);
						var distance = Vector2.Distance(new Vector2(b.Position.X, b.Position.Y), new Vector2(me.Position.X, me.Position.Y));
						Debug.WriteLine(distance.ToString());
						if (distance < Radius && (me.Position.Z - b.Position.Z) > 0 && (me.Position.Z - b.Position.Z - 2) < 15)
						{
							Debug.WriteLine("Taken checkpoint");
							BaseScript.TriggerServerEvent("tookcheckpoint", mark.Item3);
						}
					}
				}
				await BaseScript.Delay(500);
			}
		}


		~CheckpointHunt()
		{
			foreach(var marker in CheckpointMarkers)
			{
			
					Blip b = marker.Item1;
					b.Delete();

					Checkpoint c = marker.Item2;
					c.Delete();
				
			}
			CheckpointMarkers.Clear();

		}
	}
}
