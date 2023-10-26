using DredgeVR.Helpers;
using DredgeVR.Options;
using DredgeVR.VRInput;
using HarmonyLib;
using UnityEngine;

namespace DredgeVR.VRUI.Patches;

/// <summary>
/// These are the undock/sleep/research buttons
/// </summary>
[HarmonyPatch(typeof(BoatActionsDestinationUI))]
public static class BoatActionsDestinationUIPatches
{
	[HarmonyPrefix]
	[HarmonyPatch(nameof(BoatActionsDestinationUI.LateUpdate))]
	public static bool BoatActionsDestinationUI_LateUpdate(BoatActionsDestinationUI __instance)
	{
		if (OptionsManager.Options.useFlatUI)
		{
			// Same as DestinationButtonPatches, but slightly smaller
			__instance.transform.position = __instance.destination.transform.position;
			__instance.transform.localScale = Vector3.one * 5;
			DirectionHelper.LookAtPlayerInPlane(__instance.transform, true);
		}

		return false;
	}

	// Would have prefered on awake but there isn't a method for that
	[HarmonyPostfix]
	[HarmonyPatch(nameof(BoatActionsDestinationUI.OnEnable))]
	public static void BoatActionsDestinationUI_OnEnable(BoatActionsDestinationUI __instance)
	{
		if (__instance.gameObject.GetComponent<UIHandAttachment>() == null)
		{
			// Goes on the off hand so the cursor can always target it
			__instance.gameObject.AddComponent<UIHandAttachment>()
				.Init(VRInputModule.Instance.DominantHandInputSource == Valve.VR.SteamVR_Input_Sources.LeftHand, new Vector3(0, 90, 45), new Vector3(0.05f, 0.1f, 0), 1f);
		}
	}
}
