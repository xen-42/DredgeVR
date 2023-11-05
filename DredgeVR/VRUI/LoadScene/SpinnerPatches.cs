using HarmonyLib;
using UnityEngine;

namespace DredgeVR.VRUI.LoadScene;

[HarmonyPatch(typeof(Spinner))]
public static class SpinnerPatches
{
	[HarmonyPrefix]
	[HarmonyPatch(nameof(Spinner.Update))]
	public static bool Spinner_Update(Spinner __instance)
	{
		// Black Salt Games try not to use global rotation challenge (impossible)
		__instance.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, __instance.transform.localRotation.eulerAngles.z + __instance.degreesPerSecond * Time.unscaledDeltaTime);
		return false;
	}
}
