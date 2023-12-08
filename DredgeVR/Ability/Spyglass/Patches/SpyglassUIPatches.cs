using HarmonyLib;

namespace DredgeVR.Ability.Spyglass.Patches;

[HarmonyPatch(typeof(SpyglassUI))]
public static class SpyglassUIPatches
{
	[HarmonyPostfix]
	[HarmonyPatch(nameof(SpyglassUI.OnPlayerAbilityToggled))]
	public static void SpyglassUI_OnPlayerAbilityToggled(SpyglassUI __instance)
	{
		__instance.crosshairContainer.SetActive(false);
	}
}
