using DredgeVR.Helpers;
using HarmonyLib;
using UnityEngine;

namespace DredgeVR.VRUI.Patches;

[HarmonyPatch(typeof(DestinationButton))]
public static class DestinationButtonPatches
{
	[HarmonyPrefix]
	[HarmonyPatch(nameof(DestinationButton.LateUpdate))]
	public static bool DestinationButton_LateUpdate(DestinationButton __instance)
	{
		// Originally these buttons place themself to be at the screen point location corresponding to their real world destination location
		// We'll just leave them at the destination
		__instance.transform.position = __instance.destination.transform.position;
		__instance.transform.localScale = Vector3.one * 10;
		DirectionHelper.LookAtPlayerInPlane(__instance.transform, true);
		return false;
	}
}
