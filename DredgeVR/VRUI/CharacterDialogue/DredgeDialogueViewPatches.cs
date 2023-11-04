using DredgeVR.Helpers;
using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace DredgeVR.VRUI.CharacterDialogue;

[HarmonyPatch(typeof(DredgeDialogueView))]
public static class DredgeDialogueViewPatches
{
	[HarmonyPostfix]
	[HarmonyPatch(nameof(DredgeDialogueView.ShowPortrait), new Type[] { typeof(SpeakerData) })]
	public static void DredgeDialogueView_ShowPortrait(DredgeDialogueView __instance)
	{
		// Bit hacky
		// Move the background back a bit if it exists
		var background = __instance.characterPortraitContainer.GetComponentsInChildren<Transform>().FirstOrDefault(x => x.name.Contains("Background"));
		if (background != null)
		{
			background.transform.localPosition += Vector3.forward * 50f;
		}
	}
}
