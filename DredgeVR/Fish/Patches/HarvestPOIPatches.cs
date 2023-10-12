using HarmonyLib;

namespace DredgeVR.Fish.Patches;

[HarmonyPatch(typeof(HarvestPOI))]
internal class HarvestPOIPatches
{
	[HarmonyPatch(nameof(HarvestPOI.Update))]
	[HarmonyPostfix]
	public static void HarvestPOI_Update(HarvestPOI __instance)
	{
		// Trying to reduce lag
		__instance.harvestParticles.enabled = false;
	}
}
