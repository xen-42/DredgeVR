using AeLa.EasyFeedback.APIs;
using DredgeVR.Helpers;
using HarmonyLib;
using InControl;
using System.Collections.Generic;
using System.Linq;
using Winch.Core;

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
