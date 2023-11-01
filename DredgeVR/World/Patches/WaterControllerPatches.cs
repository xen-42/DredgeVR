﻿using HarmonyLib;
using UnityEngine;

namespace DredgeVR.World.Patches;

[HarmonyPatch(typeof(WaterController))]
public static class WaterControllerPatches
{
	private static int _depthID = Shader.PropertyToID("_Depth");
	private static float _previousDepth;

	[HarmonyPostfix]
	[HarmonyPatch(nameof(WaterController.Update))]
	public static void WaterController_Update(WaterController __instance) => UpdateDepth(__instance);

	[HarmonyPostfix]
	[HarmonyPatch(nameof(WaterController.OnDisable))]
	public static void WaterController_OnDisable(WaterController __instance) => UpdateDepth(__instance);

	private static void UpdateDepth(WaterController __instance)
	{
		// Was good at 800-2000 when originally it was 1
		var currentDepth = Shader.GetGlobalFloat(_depthID);

		if (GameManager.Instance.Player != null)
		{
			var t = Mathf.Clamp01(Mathf.InverseLerp(5, 40, GameManager.Instance.Player.PlayerDepthMonitor.currentDepth * 100));
			var realDepth = currentDepth * Mathf.Lerp(1400, 600, t);

			float depthToUse;

			if (_previousDepth == -1f)
			{
				depthToUse = realDepth;
			}
			else
			{
				// Lerp from old depth to new depth
				depthToUse = Mathf.Lerp(_previousDepth, realDepth, Time.deltaTime);
			}

			_previousDepth = depthToUse;
			Shader.SetGlobalFloat(_depthID, depthToUse);
		}
		else 
		{
			_previousDepth = -1f;
			Shader.SetGlobalFloat(_depthID, currentDepth * 2000);
		}
	}
}