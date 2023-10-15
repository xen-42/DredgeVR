using HarmonyLib;
using System;
using UnityEngine;

namespace DredgeVR.VRInput.Patches;

[HarmonyPatch(typeof(DredgeInputManager))]
public static class DredgeInputManagerPatches
{
	[HarmonyPostfix]
	[HarmonyPatch(nameof(DredgeInputManager.GetValue), new Type[] { typeof(DredgePlayerActionTwoAxis) })]
	public static void DredgeInputManager_GetValue_DredgePlayerActionTwoAxis(DredgeInputManager __instance, DredgePlayerActionTwoAxis playerActionAxis, ref Vector2 __result)
	{
		if (playerActionAxis == GameManager.Instance?.Player?.Controller?.MoveAction && VRInputManager.RightThumbStick != Vector2.zero)
		{
			__result = VRInputManager.RightThumbStick;
		}
	}
}
