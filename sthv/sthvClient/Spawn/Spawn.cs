using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace sthv
{
	public class Spawn : BaseScript
	{

		private static bool _spawnLock = false;

		public static void FreezePlayer(int playerId, bool freeze)
		{
			var ped = GetPlayerPed(playerId);

			SetPlayerControl(playerId, !freeze, 0);

			if (!freeze)
			{
				if (!IsEntityVisible(ped))
					SetEntityVisible(ped, true, false);

				if (!IsPedInAnyVehicle(ped, true))
					SetEntityCollision(ped, true, true);

				FreezeEntityPosition(ped, false);
				//SetCharNeverTargetted(ped, false)
				SetPlayerInvincible(playerId, false);
			}
			else
			{
				if (IsEntityVisible(ped))
					SetEntityVisible(ped, false, false);

				SetEntityCollision(ped, false, true);
				FreezeEntityPosition(ped, true);
				//SetCharNeverTargetted(ped, true)
				SetPlayerInvincible(playerId, true);

				if (IsPedFatallyInjured(ped))
					ClearPedTasksImmediately(ped);
			}
		}
		///<exception cref="Exception">Throws when <paramref name="skin"/> is invalid.</exception>
		public static async Task SpawnPlayer(string skin, float x, float y, float z, float heading)
		{
			if (_spawnLock)
				return;

			_spawnLock = true;

			DoScreenFadeOut(500);

			//model loaded to be indirectly used by Game.Player.ChangeModel
			RequestModel((uint)GetHashKey(skin));
			RequestCollisionAtCoord(x, y, z);

			while (IsScreenFadingOut())
			{
				await Delay(1);
			}

			FreezePlayer(PlayerId(), true);
			//ChangeModel times out and returns false if model fails to load in 1000ms
			var retries = 0;
			while (!await Game.Player.ChangeModel(GetHashKey(skin)))
			{
				if (retries > 5)
				{
					Debug.WriteLine("^3SpawnPlayer failed because skin was invalid");
					break;
				}
				++retries;
			}
			SetPedDefaultComponentVariation(GetPlayerPed(-1));
			RequestCollisionAtCoord(x, y, z);

			var ped = GetPlayerPed(-1);

			SetEntityCoordsNoOffset(ped, x, y, z, false, false, false);
			NetworkResurrectLocalPlayer(x, y, z, heading, true, true);
			ClearPedTasksImmediately(ped);
			RemoveAllPedWeapons(ped, false);
			ClearPlayerWantedLevel(PlayerId());
			NetworkSetInSpectatorMode(false, 0);

			while (!HasCollisionLoadedAroundEntity(ped))
			{
				await Delay(1);
			}
			ShutdownLoadingScreenNui();
			ShutdownLoadingScreen();
			DoScreenFadeIn(500);

			while (IsScreenFadingIn())
			{
				await Delay(1);
			}

			FreezePlayer(PlayerId(), false);

			//TriggerEvent("playerSpawned", PlayerId());

			_spawnLock = false;
		}
	}
}