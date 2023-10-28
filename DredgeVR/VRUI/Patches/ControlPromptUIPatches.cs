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

		foreach (var controlPrompt in __instance.controlPrompts)
		{
			// For stuff that isn't bound in VR just slap them on the non-dominant hand so they can be laser pointered
			var handName = VRInputModule.Instance.DominantHand.hand == Valve.VR.SteamVR_Input_Sources.LeftHand ? "left" : "right";

			var playerAction = controlPrompt?.dredgePlayerAction?.playerAction as PlayerAction;
			if (playerAction != null && VRInputManager.PlayerActionBindings.TryGetValue(playerAction, out var vrBinding))
			{
				handName = vrBinding.action.GetHandName();
			}
			var hand = handName == "left" ? VRUIManager.LeftHandPromptsContainer : VRUIManager.RightHandPromptsContainer;

			// Moves the prompt to the right hand
			controlPrompt.gameObject.transform.parent = hand;
			controlPrompt.gameObject.transform.localPosition = Vector3.zero;
			controlPrompt.gameObject.transform.localRotation = Quaternion.identity;
		}
	}
}
