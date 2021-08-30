using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;


namespace sthv.Gamemodes
{
	struct Marker {
		public Blip blip;
		public Checkpoint checkpoint;
		public int id;
	}
	class CheckpointHunt : BaseGamemode
	{

		List<Marker> CheckpointMarkers = new List<Marker>();
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

				var marker = new Marker(){blip=blip, checkpoint = check, id = id};
				CheckpointMarkers.Add(marker);
			}));

			AddEventHandler("removecheckpoint", new Action<int>((checkpointid) =>
			{
				var markers = CheckpointMarkers.FindAll(p => p.id == checkpointid);
				foreach (var marker in markers)
				{
					marker.blip.Delete();
					marker.checkpoint.Delete();
				}
				CheckpointMarkers.RemoveAll(p => p.id == checkpointid);
			}));

			

			AddTick(CheckpointChecker);
		}
		
		async Task CheckpointChecker()
		{
		
			if (client.IsRunner)
			{
				foreach (var mark in CheckpointMarkers)
				{
					var b = mark.blip;
					var me = Game.PlayerPed;
					var distance = Vector2.Distance(new Vector2(b.Position.X, b.Position.Y), new Vector2(me.Position.X, me.Position.Y));
					Debug.WriteLine("blips: " + b.Position.X + " " + b.Position.Y + " mine: " + me.Position.X + " " + me.Position.Y);
					Debug.WriteLine(distance.ToString());
					if (distance < Radius && (me.Position.Z - b.Position.Z) > 0 && (me.Position.Z - b.Position.Z - 2) < 15)
					{
						Debug.WriteLine("Taken checkpoint");
						BaseScript.TriggerServerEvent("tookcheckpoint", mark.id);
					}
				}
			}
			await BaseScript.Delay(500);
		
		}

		override public void GamemodeFinalizer()
		{
			Debug.WriteLine($"^2---- Gammode Finalized with {CheckpointMarkers.Count} markers. ----");
			foreach(var marker in CheckpointMarkers)
			{
					Blip b = marker.blip;
					b.Delete(); //deletes blip from game

					Checkpoint c = marker.checkpoint;
					c.Delete(); //deletes checkpoint from game 
					Debug.WriteLine($"Marker has blip: {b.Handle} and checkpoint: {c.Handle}");

				
			}
			CheckpointMarkers.Clear();

			//TODO: this isn't right? Blips seem to get removed correctly now.
			foreach(var b in World.GetAllBlips()){
				Debug.WriteLine("Undeleted blip: " + b.Handle);
			}
		}
	}
}
