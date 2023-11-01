using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;

namespace DredgeVR.VRCamera.Patches;

[HarmonyPatch]
public static class PlanarReflectionsPatches
{
	private static Quaternion _cachedRotation;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(PlanarReflections), nameof(PlanarReflections.DoPlanarReflections))]
	public static void PlanarReflections_DoPlanarReflections_Pre(PlanarReflections __instance, ScriptableRenderContext context, Camera camera)
	{
		_cachedRotation = camera.transform.rotation;

		camera.transform.rotation *= Quaternion.Inverse(Quaternion.identity);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlanarReflections), nameof(PlanarReflections.DoPlanarReflections))]
	public static void PlanarReflections_DoPlanarReflections_Post(PlanarReflections __instance, ScriptableRenderContext context, Camera camera)
	{
		camera.transform.rotation = _cachedRotation;
	}
}
