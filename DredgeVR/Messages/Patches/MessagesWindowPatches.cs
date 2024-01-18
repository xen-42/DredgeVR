using HarmonyLib;
using UnityEngine;

namespace DredgeVR.VRUI.Patches;

[HarmonyPatch(typeof(MessagesWindow))]
public static class MessagesWindowPatches
{
	[HarmonyPostfix]
	[HarmonyPatch(nameof(MessagesWindow.Show))]
	public static void MessagesWindow_Show(MessagesWindow __instance)
	{
		// Base game method uses global position
		__instance.messageEntryContainer.localPosition = Vector3.zero;
		__instance.transform.Find("Container/Scrim").gameObject.SetActive(false);
	}

	[HarmonyPostfix]
	[HarmonyPatch(nameof(MessagesWindow.OnTabChanged))]
	public static void MessagesWindow_OnTabChanged(MessagesWindow __instance)
	{
		// Not sure why but something in the refresh moves it here as well
		__instance.messageEntryContainer.transform.localPosition = Vector3.zero;
	}
}
