using DredgeVR.VRCamera;
using HarmonyLib;
using UnityEngine;

namespace DredgeVR.VRUI.Patches;

[HarmonyPatch(typeof(GridObject))]
public static class GridObjectPatches
{
	[HarmonyPostfix]
	[HarmonyPatch(nameof(GridObject.FixedUpdate))]
	public static void GridObject_FixedUpdate(GridObject __instance)
	{
		if (!__instance.isPickedUp)
		{
			__instance.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, __instance.CurrentRotation);
			__instance.transform.localPosition = new Vector3(__instance.transform.localPosition.x, __instance.transform.localPosition.y, 0f);
		}
	}
}
