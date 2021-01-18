using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;


namespace sthv
{
	class sthvGameoverHandler : BaseScript
	{
		//[Command("cam")]
		async Task onCommandCam()
		{
			TriggerServerEvent("sth:sendServerDebug", $"new Vector3({Game.PlayerPed.Position.X}, {Game.PlayerPed.Position.Y}, {Game.PlayerPed.Position.Z})");

			//await Delay(2000);

			Debug.WriteLine("ssdsadgfsdfdfdfdfdas.");



			//Game.PlayerPed.Position = new Vector3(133.2807f, -567.3618f, 43.821f);
			var ped = Game.PlayerPed;

			Game.PlayerPed.IsInvincible = true;
			API.DoScreenFadeOut(1000);

			var pos = Game.PlayerPed.Position;
			API.SetPedDesiredHeading(ped.Handle, 200);

			await Delay(1000);
			API.DoScreenFadeIn(2000);

			var _mainCam = World.CreateCamera(Game.PlayerPed.GetOffsetPosition(new Vector3(0f, 2.75f, 0.35f)), Vector3.Zero, GameplayCamera.FieldOfView);
			_mainCam.PointAt(Game.PlayerPed.Bones[Bone.IK_Head], new Vector3(0f, 0f, 0.2f));

			// Customize Camera
			var _customCam = World.CreateCamera(_mainCam.GetOffsetPosition(new Vector3(-1f, -8f, -1f)), Vector3.Zero, GameplayCamera.FieldOfView);
			_customCam.PointAt(Game.PlayerPed.Bones[Bone.IK_Head], new Vector3(-1.75f, 0f, 0.35f));

			World.RenderingCamera = _customCam;
			API.SetCamActiveWithInterp(_mainCam.Handle, _customCam.Handle, 5000, 100, 100);

			await Delay(5000);
			Game.PlayerPed.Task.ClearAll();
			Game.PlayerPed.Task.WanderAround();
			
			//_currentCam = _mainCam;
			
			await Delay(5000);
			API.DoScreenFadeOut(2000);
			await Delay(2000);

			_mainCam.IsActive = false;
			_mainCam.Delete();
			_customCam.IsActive = false;
			_customCam.Delete();
			World.RenderingCamera = null;
			Game.PlayerPed.Task.ClearAll();
			API.DoScreenFadeIn(2000);


		}
	}
}
