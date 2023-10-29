using DredgeVR.VRInput;
using HarmonyLib;
using InControl;
using System;
using System.Linq;

namespace DredgeVR.VRUI.Patches;

[HarmonyPatch(typeof(DredgeInputManager))]
public static class DredgeInputManagerPatches
{
	[HarmonyPrefix]
	[HarmonyPatch(nameof(DredgeInputManager.GetControlIconForAction), new Type[] { typeof(PlayerAction), typeof(BindingSourceType), typeof(bool) })]
	public static bool DredgeInputManager_GetControlIconForAction(DredgeInputManager __instance, PlayerAction action, BindingSourceType requestedBindingSourceType, bool combineMouseKeyboard, ref ControlIconData __result)
	{
		if (requestedBindingSourceType == VRBindingSource.Source)
		{
			var vrAction = ((VRBindingSource)action.Bindings?.FirstOrDefault(x => x is VRBindingSource))?.action;

			if (vrAction != null)
			{
				var controlIcon = VRControlIcons.GetControlIconData(vrAction);
				if (controlIcon != null)
				{
					__result = controlIcon;
					return false;
				}
			}
		}

		return true;
	}
}
