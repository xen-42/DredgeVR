﻿using HarmonyLib;

namespace DredgeVR.VRInput.Patches;

[HarmonyPatch(typeof(DredgeInputManager))]
public static class DredgeInputManagerPatches
{
	[HarmonyPostfix]
	[HarmonyPatch(nameof(DredgeInputManager.IsUsingController), MethodType.Getter)]
	public static void DredgeInputManager_IsUsingController(DredgeInputManager __instance, ref bool __result)
	{
		// There's some places where the sticks don't get checked if it doesn't think we're using a controller
		__result = true;
	}
}