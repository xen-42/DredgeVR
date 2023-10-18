using DredgeVR.VRCamera;
using DredgeVR.VRInput;
using HarmonyLib;
using UnityEngine;

namespace DredgeVR.VRUI.Patches;

/// <summary>
/// These are for the items in the inventory grid spots
/// </summary>
[HarmonyPatch(typeof(GridObject))]
public static class GridObjectPatches
{
	[HarmonyPostfix]
	[HarmonyPatch(nameof(GridObject.FixedUpdate))]
	public static void GridObject_FixedUpdate(GridObject __instance)
	{
		if (!__instance.isPickedUp)
		{
			// Without this patch, they have constant world space rotation which gets messed up if the boat has turned
			__instance.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, __instance.CurrentRotation);
			__instance.transform.localPosition = new Vector3(__instance.transform.localPosition.x, __instance.transform.localPosition.y, 0f);
		}
		else
		{
			// Without this, the held item goes way off in the distance and can't be seen
			__instance.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, __instance.CurrentRotation);
			// The GridManager controls the position of the held item at this point so we leave it be
		}
	}
}
