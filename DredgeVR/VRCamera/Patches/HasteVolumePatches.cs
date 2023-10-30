using HarmonyLib;
using UnityEngine;

namespace DredgeVR.VRCamera.Patches;

[HarmonyPatch(typeof(HasteVolume))]
public static class HasteVolumePatches
{
	[HarmonyPostfix]
	[HarmonyPatch(nameof(HasteVolume.Awake))]
	public static void HasteVolume_Awake(HasteVolume __instance)
	{
		// Vignette and motion blur are a bad idea in VR anyway
		// But mostly this thing flips the y axis of the camera when you use it
		Component.Destroy(__instance.volume);
	}
}
