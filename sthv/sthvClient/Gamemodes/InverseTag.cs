using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core.Native;
using CitizenFX.Core;

namespace sthv.Gamemodes
{
	class InverseTag : BaseGamemode
	{
		bool first = true;
		private Player _runner;
		int runnerServerId = -1;

		public Player Runner
		{
			get { return _runner; }
			set {
				runnerServerId = value.ServerId;
				_runner = value;
			}
		}
		
		public InverseTag() : base(Shared.Gamemode.InverseTag)
		{
			Debug.WriteLine("START INVERSE TAG");
		
			base.AddEventHandler("sth:new_runner", new Action<int>((i)=>{
				client.RunnerServerId = i;
				Runner = new Player(API.GetPlayerFromServerId(i));
				if(Runner == null){
					throw new Exception("Runner is null in sth:new_runner");
				}
				Debug.WriteLine($"New runner {Runner.Name} with id {i}");
			}));

			// base.AddTick(CollisionDetector);
			base.AddTick(DrawCone);
			base.AddTick(LogInfo);
		}
		async Task LogInfo(){
			await BaseScript.Delay(1000);
			if(runnerServerId < 1) Runner = new Player(API.GetPlayerFromServerId(client.RunnerServerId));
			Debug.WriteLine("I think runner is: " + Runner.ServerId + " client.runnersid: " + client.RunnerServerId);
		}
		async Task DrawCone(){
			if(Runner == null){ //if runner hasn't been set yet.

				await BaseScript.Delay(1000);
				Debug.WriteLine("Runner == null in InverseTag");
				Runner = new Player(API.GetPlayerFromServerId(client.RunnerServerId));
				return;
			}
			var runner = Runner;
			//send runner from server 
			if(runner.ServerId != Game.Player.ServerId && Game.PlayerPed.IsInVehicle() && runner.Character.IsInVehicle() && runner.Character.CurrentVehicle.IsTouching(Game.PlayerPed.CurrentVehicle)){
				BaseScript.TriggerServerEvent("sth:tagged_runner", runner.ServerId );
				Debug.WriteLine("tagged runner: " + runner.Name);
				
			}
			var pos = runner.Character.Position;
			float height;
			if(runner.Character.IsInVehicle()){
				height = runner.Character.CurrentVehicle.Model.GetDimensions().Z;
			}
			else{
				height = runner.Character.Model.GetDimensions().Z;
			}
			World.DrawMarker(MarkerType.UpsideDownCone, pos + new Vector3(0, 0, height*2), pos, new Vector3(), new Vector3(height*2, height*2, height), System.Drawing.Color.FromArgb(70, 155, 0, 255), true);
		}
	}
}
