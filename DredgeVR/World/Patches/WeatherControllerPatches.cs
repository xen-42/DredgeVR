using HarmonyLib;
using UnityEngine;

namespace DredgeVR.World.Patches;

[HarmonyPatch(typeof(WeatherController))]
internal class WeatherControllerPatches
{
	[HarmonyPostfix]
	[HarmonyPatch(nameof(WeatherController.Update))]
	public static void WeatherController_Update(WeatherController __instance)
	{
		// TODO: Could make this an optional setting for crazy people
		// Meant to lessen sea-sickness
		__instance._waveSteepness = 0;
		Shader.SetGlobalFloat("_WaveSteepness", 0);
		GameManager.Instance.WaveController.Steepness = 0;
	}
}
