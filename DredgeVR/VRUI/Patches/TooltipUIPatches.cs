using DredgeVR.Items;
using HarmonyLib;
using UnityEngine;

namespace DredgeVR.VRUI.Patches;

[HarmonyPatch(typeof(TooltipUI))]
internal class TooltipUIPatches
{
	[HarmonyPostfix]
	[HarmonyPatch(nameof(TooltipUI.Awake))]
	public static void TooltipUI_Awake(TooltipUI __instance)
	{
		// Need to add an intermediate game object for good rotation
		// Doesn't work when directly on the tooltip since something is overriding the position
		var tooltipHeldRoot = new GameObject("VRToolTip").transform;
		tooltipHeldRoot.parent = __instance.transform;
		tooltipHeldRoot.localPosition = Vector3.zero;
		tooltipHeldRoot.localRotation = Quaternion.identity;
		__instance.containerRect.parent = tooltipHeldRoot;
		tooltipHeldRoot.gameObject.AddComponent<HeldUI>().SetOffset(100, 300);
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(TooltipUI.LateUpdate))]
	public static bool TooltipUI_LateUpdate(TooltipUI __instance)
	{
		return false;
	}
}
