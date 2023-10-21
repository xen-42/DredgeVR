using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		__instance.messageEntryContainer.localPosition = new Vector3(__instance.messageEntryContainer.position.x, __instance.messageEntryContainer.sizeDelta.y * -0.5f, 0f);
	}
}
