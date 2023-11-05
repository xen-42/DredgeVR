using HarmonyLib;
using UnityEngine;

namespace DredgeVR.VRUI.Credits;

[HarmonyPatch(typeof(CreditsController))]
public static class CreditsControllerPatches
{
	[HarmonyPostfix]
	[HarmonyPatch(nameof(CreditsController.Awake))]
	public static void CreditsController_Awake(CreditsController __instance)
	{
		__instance.gameObject.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
		__instance.gameObject.AddComponent<GameCanvasFixer>();
		__instance.transform.Find("Scrim").gameObject.SetActive(false);
	}
}
