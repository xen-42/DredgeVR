using HarmonyLib;
using UnityEngine;

namespace DredgeVR.Ability.Patches;

// TODO

/*
[HarmonyPatch(typeof(SpyglassMapStamp))]
public static class SpyglassMapStampPatches
{
	[HarmonyPrefix]
	[HarmonyPatch(nameof(SpyglassMapStamp.LateUpdate))]
	public static bool SpyglassMapStamp_LateUpdate(SpyglassMapStamp __instance)
	{
		// Normal method has these flat on the screen
		// We keep them at their world position

		__instance.transform.position = __instance.worldPosition + Vector3.up;

		// Count them as viewable if within 100m
		// TODO: Would probably be good to also actually check if they are in front of the player
		__instance.isOnScreen = (__instance.transform.position - GameManager.Instance.Player.transform.position).magnitude < 100;
		__instance.stampImage.enabled = __instance.isOnScreen;
		__instance.distanceTextField.enabled = __instance.isOnScreen;

		if (__instance.isOnScreen)
		{
			__instance.timeUntilDistanceUpdate -= Time.deltaTime;
			if (__instance.timeUntilDistanceUpdate < 0f)
			{
				__instance.timeUntilDistanceUpdate = __instance.timeBetweenDistanceUpdates;
				__instance.UpdatePlayerDistance();
			}
		}

		return false;
	}
}
*/
