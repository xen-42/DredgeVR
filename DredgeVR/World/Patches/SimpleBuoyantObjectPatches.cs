using DredgeVR.Options;
using HarmonyLib;
using UnityEngine;

namespace DredgeVR.World.Patches;

[HarmonyPatch(typeof(SimpleBuoyantObject))]
public static class SimpleBuoyantObjectPatches
{
	[HarmonyPrefix]
	[HarmonyPatch(nameof(SimpleBuoyantObject.UpdateWaveSteepness))]
	public static bool SimpleBuoyantObject_UpdateWaveSteepness()
	{
		// No point doing an expensive calculation if there's no waves
		return !OptionsManager.Options.removeWaves;
	}

	[HarmonyPostfix]
	[HarmonyPatch(nameof(SimpleBuoyantObject.Awake))]
	public static void SimpleBuoyantObject_Awake(SimpleBuoyantObject __instance)
	{
		// In base game all simple buoyant objects start at the same time, meaning they all call the expensive UpdateWaveSteepness method at the same time
		// We stagger their starts so that it spreads out the call more
		__instance.timeSinceWaveSteepnessUpdated = Random.Range(0f, __instance.timeBetweenUpdatingWaveSteepnessSec);
	}
}
