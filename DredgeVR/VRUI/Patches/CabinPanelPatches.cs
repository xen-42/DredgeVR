using DredgeVR.Helpers;
using HarmonyLib;
using UnityEngine;

namespace DredgeVR.VRUI.Patches;

[HarmonyPatch(typeof(CabinPanel))]
public static class CabinPanelPatches
{
	// TODO: Find a better place to do this
	[HarmonyPostfix]
	[HarmonyPatch(nameof(CabinPanel.ShowFinish))]
	[HarmonyPatch(nameof(CabinPanel.OnEncyclopediaWindowClosed))]
	[HarmonyPatch(nameof(CabinPanel.OnMessagesWindowClosed))]
	[HarmonyPatch(nameof(CabinPanel.OnJournalWindowClosed))]
	[HarmonyPatch(nameof(CabinPanel.OnMapWindowClosed))]
	public static void CabinPanel_ShowFinish(CabinPanel __instance)
	{
		// Fix bookshelf position
		Delay.FireOnNextUpdate(() =>
		{
			var itemScroller = __instance.transform.Find("Container/ItemScroller");
			var nonSpatialItemGrid = __instance.transform.Find("Container/ItemScroller/NonSpatialItemGrid");
			itemScroller.localPosition = Vector3.down * 80;
			nonSpatialItemGrid.localPosition = Vector3.zero;
		});
	}
}
