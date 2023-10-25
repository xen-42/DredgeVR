using HarmonyLib;

namespace DredgeVR.VRUI.Patches;

[HarmonyPatch(typeof(GameManager))]
public static class GameManagerPatches
{
	[HarmonyPrefix]
	[HarmonyPatch(nameof(GameManager.ScaleFactor), MethodType.Getter)]
	public static bool GameManager_ScaleFactor_Get(GameManager __instance, ref float __result)
	{
		if (__instance.canvasScaler == null)
		{
			__result = 0.001f;
			return false;
		}
		else
		{
			return true;
		}
	}
}
