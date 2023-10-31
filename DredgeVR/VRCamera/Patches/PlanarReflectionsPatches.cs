using Cinemachine.Utility;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;
using static PlanarReflections;

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

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlanarReflections), nameof(PlanarReflections.CalculateReflectionMatrix))]
	public static void PlanarReflections_CalculateReflectionMatrix(ref Matrix4x4 reflectionMatrix)
	{
		//reflectionMatrix *= Matrix4x4.Scale(new Vector3(1, -1, 1));
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlanarReflectionSettingData), nameof(PlanarReflectionSettingData.Set))]
	public static void PlanarReflectionSettingData_Set(PlanarReflectionSettingData __instance)
	{
		GL.invertCulling = false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlanarReflectionSettingData), nameof(PlanarReflectionSettingData.Restore))]
	public static void PlanarReflectionSettingData_Restore(PlanarReflectionSettingData __instance)
	{
		GL.invertCulling = true;
	}
}
