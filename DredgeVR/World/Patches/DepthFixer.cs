using HarmonyLib;
using UnityEngine;

namespace DredgeVR.World.Patches;

[HarmonyPatch(typeof(WaterController))]
public static class DepthFixer
{
	private static int _depthID = Shader.PropertyToID("_Depth");
	private static float _previousDepth;

	// Exposed for testing with unity explorer
	public static float depthModifier = 1f;

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
			var depth = GameManager.Instance.Player.PlayerDepthMonitor.currentDepth * 100; // m

			var t = Mathf.Clamp01(Mathf.InverseLerp(5, 30, depth));
			// High "depth" is clearer than low "depth"
			var realDepth = currentDepth * Mathf.Lerp(1200, 400, t*t);

			// In the middle of the ocean just make the water clear since theres no bottom anyway
			if (depth >= 100)
			{
				realDepth = currentDepth * 1000f;
			}

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
			Shader.SetGlobalFloat(_depthID, depthToUse * depthModifier);
		}
		else 
		{
			_previousDepth = -1f;
			Shader.SetGlobalFloat(_depthID, currentDepth * 2000);
		}
	}
}
