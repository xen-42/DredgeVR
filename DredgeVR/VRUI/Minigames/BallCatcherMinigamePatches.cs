using HarmonyLib;
using UnityEngine;

namespace DredgeVR.VRUI.Patches;

[HarmonyPatch]
public static class BallCatcherMinigamePatches
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(BallCatcherMinigame), nameof(BallCatcherMinigame.PrepareGame))]
	public static void BallCatcherMinigame_PrepareGame(BallCatcherMinigame __instance, HarvestDifficultyConfigData difficultyConfig)
	{
		// Base game uses global rotation instead of local
		float num = 0f;
		__instance.targetZoneAngleMin = MathUtil.TransformAngleToNegative180Positive180(-(difficultyConfig.targetZoneDegrees * 0.5f) - num);
		__instance.targetZoneAngleMax = MathUtil.TransformAngleToNegative180Positive180(difficultyConfig.targetZoneDegrees * 0.5f + num);
		__instance.targetZoneImage.fillAmount = difficultyConfig.targetZoneDegrees / 360f;
		// Only line that changed
		__instance.targetZoneImage.gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, __instance.targetZoneAngleMax - num);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(BallCatcherBall), nameof(BallCatcherBall.Init))]
	public static void BallCatcherBall_Init(BallCatcherBall __instance)
	{
		// Base game uses global rotation instead of local
		if (__instance.direction == BallCatcherBallDirection.LEFT)
		{
			__instance.transform.localRotation = Quaternion.Euler(0f, 0f, __instance.launcherAngle);
		}
		else if (__instance.direction == BallCatcherBallDirection.RIGHT)
		{
			__instance.transform.localRotation = Quaternion.Euler(0f, 0f, -__instance.launcherAngle);
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(BallCatcherBall), nameof(BallCatcherBall.Update))]
	public static bool BallCatcherBall_Update(BallCatcherBall __instance)
	{
		// Basically have to replace the entire method since it uses global rotation
		__instance.cachedTransformedAngle = __instance.TransformedAngle;
		if (!__instance.hasGonePastTargetZone)
		{
			if (__instance.direction == BallCatcherBallDirection.LEFT && __instance.cachedTransformedAngle < __instance.targetZoneEdge)
			{
				__instance.hasGonePastTargetZone = true;
				__instance.Deactivate();
			}
			else if (__instance.direction == BallCatcherBallDirection.RIGHT && __instance.cachedTransformedAngle > __instance.targetZoneEdge)
			{
				__instance.hasGonePastTargetZone = true;
				__instance.Deactivate();
			}
		}
		if (!__instance.hasReachedOtherSide)
		{

			// The only part that was changed
			__instance.transform.localRotation *= Quaternion.AngleAxis(__instance.speed * Time.deltaTime, Vector3.forward);
			//

			if (__instance.direction == BallCatcherBallDirection.LEFT && __instance.cachedTransformedAngle < -__instance.launcherAngle)
			{
				__instance.hasReachedOtherSide = true;
				var onReachedOtherSide = __instance.OnReachedOtherSide;
				if (onReachedOtherSide == null)
				{
					return false;
				}
				onReachedOtherSide(__instance);
				return false;
			}
			else if (__instance.direction == BallCatcherBallDirection.RIGHT && __instance.cachedTransformedAngle > __instance.launcherAngle)
			{
				__instance.hasReachedOtherSide = true;
				var onReachedOtherSide2 = __instance.OnReachedOtherSide;
				if (onReachedOtherSide2 == null)
				{
					return false;
				}
				onReachedOtherSide2(__instance);
			}
		}


		return false;
	}
}
