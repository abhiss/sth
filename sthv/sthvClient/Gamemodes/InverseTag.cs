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
		int seconds_runner_not_in_car = 0;
		bool isTaggingAllowed = true;
		public Player Runner
		{
			get { return _runner; }
			set {
				runnerServerId = value.ServerId;
				client.RunnerServerId = value.ServerId;
				Debug.WriteLine($"Runner is {value.Name} with id {value.ServerId}.");
				
				_runner = value;
			}
		}
		int serverid = Game.Player.ServerId;

		public InverseTag() : base(Shared.Gamemode.InverseTag)
		{
			Debug.WriteLine("START INVERSE TAG");
		
			base.AddEventHandler("sthv:no_tagback_time_ms", new Action<int>(async(no_tagback_time_ms)=>{
				isTaggingAllowed = false;
				await BaseScript.Delay(no_tagback_time_ms);
				isTaggingAllowed = true;
			}));
			base.AddEventHandler("sth:new_runner", new Action<int>((i)=>{
				Runner = new Player(API.GetPlayerFromServerId(i));
				if(Runner == null){
					throw new Exception("Runner is null in sth:new_runner");
				}
			}));

			base.AddTick(OnTick); //detect collisions and draw cone on runner
			base.AddTick(RunnerCarCheck);//checks that runner is in a car
			base.AddTick(LogInfo);
		}
		async Task LogInfo(){
			await BaseScript.Delay(20_000);
			if(runnerServerId < 1) Runner = new Player(API.GetPlayerFromServerId(client.RunnerServerId));
			Debug.WriteLine("I think runner is: " + Runner.ServerId + " client.runnersid: " + client.RunnerServerId);
		}

		async Task RunnerCarCheck(){
			var player = Game.Player;
			//hunters skip function body
			if(Runner == null) {
				Debug.WriteLine("^5 Runner is null. Exception 12392.");
				await BaseScript.Delay(2000);
			}
			if(Runner.ServerId != player.ServerId)
			{
				Debug.WriteLine("I'm not runner");
				
				await BaseScript.Delay(10_000);
				return;
			}

			//check if in vehicle
			if(player.Character.IsInVehicle()){
				seconds_runner_not_in_car = 10;
			}
			else{
				if(seconds_runner_not_in_car < 0){
					BaseScript.TriggerServerEvent("sth:player_not_in_car_too_long");
					await BaseScript.Delay(5000);
					seconds_runner_not_in_car = 10;
				}
				seconds_runner_not_in_car--;
				client.SendChatMessage("Inverse Tag", "You must be in a vehicle as runner! You will lose in " + seconds_runner_not_in_car + " seconds.");
			}
			await BaseScript.Delay(1000);
		}
		
		//runs per frame (if runner isn't null (when runner is culled) & if client isn't runner).
		async Task OnTick(){
			if(Runner == null){ //if runner hasn't been set yet.
				
				await BaseScript.Delay(1000);
				Debug.WriteLine("Runner == null in InverseTag");
				Runner = new Player(API.GetPlayerFromServerId(client.RunnerServerId));
				return;
			}

			var runner = Runner;
			
			//only hunters detect collisions
			if(runnerServerId != serverid){
				//collision detection
				if(runner.ServerId != Game.Player.ServerId &&
				Game.PlayerPed.IsInVehicle() && 
				( 
					(runner.Character.IsInVehicle() && 
					runner.Character.CurrentVehicle.IsTouching(Game.PlayerPed.CurrentVehicle)) //runner vehicle is touched 
				|| runner.Character.IsTouching(Game.PlayerPed.CurrentVehicle)) //runner character is touched
				)
				{
					//report to server
					BaseScript.TriggerServerEvent("sth:tagged_runner", runner.ServerId );
					Debug.WriteLine("tagged runner: " + runner.Name);
				}
			}

			//draw cone
			var pos = runner.Character.Position;
			float height;
			if(runner.Character.IsInVehicle()){
				height = runner.Character.CurrentVehicle.Model.GetDimensions().Z;
			}
			else{
				height = runner.Character.Model.GetDimensions().Z;
			}
			
			int alpha = (runnerServerId == serverid) ? 10 : 75; 
			System.Drawing.Color color;
			if(isTaggingAllowed){
				color = System.Drawing.Color.FromArgb(alpha, 255,0,255);
			}
			else{
				color = System.Drawing.Color.FromArgb(alpha,170,170,170);
			}
			World.DrawMarker(MarkerType.UpsideDownCone, pos + new Vector3(0, 0, height*2), pos, new Vector3(), new Vector3(height*2, height*2, height), color, true);
		}
	}
}
