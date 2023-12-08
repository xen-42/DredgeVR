using HarmonyLib;

namespace DredgeVR.Ability.Spyglass.Patches;

[HarmonyPatch(typeof(HarvestPOI))]
internal class SpyglassHarvestablePOIPatches
{
	[HarmonyPostfix]
	[HarmonyPatch(nameof(HarvestPOI.Start))]
	public static void HarvestPOI_Start(HarvestPOI __instance)
	{
		__instance.gameObject.AddComponent<SpyglassHarvestPOIUI>();
	}
}
