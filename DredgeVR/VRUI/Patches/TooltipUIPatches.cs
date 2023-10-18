using HarmonyLib;
using UnityEngine;

namespace DredgeVR.VRUI.Patches;

[HarmonyPatch(typeof(TooltipUI))]
internal class TooltipUIPatches
{
	[HarmonyPostfix]
	[HarmonyPatch(nameof(TooltipUI.LateUpdate))]
	public static void TooltipUI_LateUpdate(TooltipUI __instance)
	{
		__instance.containerRect.localPosition = Vector3.zero;
	}
}
