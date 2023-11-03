using DredgeVR.Options;
using HarmonyLib;
using UnityEngine;

namespace DredgeVR.World.Patches;

[HarmonyPatch(typeof(WeatherController))]
public static class WeatherControllerPatches
{
	[HarmonyPostfix]
	[HarmonyPatch(nameof(WeatherController.Update))]
	public static void WeatherController_Update(WeatherController __instance)
	{
		// Meant to lessen sea-sickness
		if (OptionsManager.Options.removeWaves)
		{
			__instance._waveSteepness = 0;
			Shader.SetGlobalFloat("_WaveSteepness", 0);
			GameManager.Instance.WaveController.Steepness = 0;
		}
	}
}
