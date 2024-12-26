using DredgeVR.Helpers;
using DredgeVR.Options;
using DredgeVR.VRInput;
using HarmonyLib;
using InControl;
using UnityEngine;

namespace DredgeVR.VRUI.Patches;

// Moves prompts to be on the correct hands if not using flat UI

[HarmonyPatch(typeof(ControlPromptUI))]
public static class ControlPromptUIPatches
{
	[HarmonyPostfix]
	[HarmonyPatch(nameof(ControlPromptUI.RefreshControlPromptUI))]
	public static void ControlPromptUI_RefreshControlPromptUI(ControlPromptUI __instance)
	{
		// Only move the ones that appear in the bottom right
		if (__instance.controlPromptMode != ControlPromptUI.ControlPromptMode.BOTTOM_RIGHT || OptionsManager.Options.useFlatUI)
		{
			return;
		}

		// Can't use the actual childCount because theres some overlap between deleting old prompts and making new ones where it reports too many
		var leftChildCount = 0;
		var rightChildCount = 0;
		foreach (var controlPrompt in __instance.controlPrompts)
		{
			// For stuff that isn't bound in VR just slap them on the non-dominant hand so they can be laser pointered
			var handName = (controlPrompt?.dredgePlayerAction?.playerAction as PlayerAction).GetHand();

			var hand = handName == "left" ? VRUIManager.LeftHandPromptsContainer : VRUIManager.RightHandPromptsContainer;
			var promptIndex = handName == "left" ? leftChildCount : rightChildCount;

			if (handName == "left")
			{
				leftChildCount++;
			}
			else
			{
				rightChildCount++;
			}

			// Moves the prompt to the proper hand
			controlPrompt.gameObject.transform.parent = hand;
			controlPrompt.gameObject.transform.localPosition = Vector3.zero + (Vector3.up * 60f * promptIndex);
			controlPrompt.gameObject.transform.localRotation = Quaternion.identity;
			controlPrompt.gameObject.transform.localScale = Vector3.one;
		}
	}
}
