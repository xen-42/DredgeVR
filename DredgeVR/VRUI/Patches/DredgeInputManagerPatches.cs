using DredgeVR.VRInput;
using HarmonyLib;
using InControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DredgeVR.VRInput.VRInputManager;

namespace DredgeVR.VRUI.Patches;

[HarmonyPatch(typeof(DredgeInputManager))]
public static class DredgeInputManagerPatches
{
	[HarmonyPrefix]
	[HarmonyPatch(nameof(DredgeInputManager.GetControlIconForAction), new Type[] { typeof(PlayerAction), typeof(BindingSourceType), typeof(bool) })]
	public static bool DredgeInputManager_GetControlIconForAction(DredgeInputManager __instance, PlayerAction action, BindingSourceType requestedBindingSourceType, bool combineMouseKeyboard)
	{
		if (requestedBindingSourceType == VRBindingSource.Source)
		{
			var vrBindingSource = (VRBindingSource)action.Bindings.FirstOrDefault(x => x is VRBindingSource);
			if (vrBindingSource != null)
			{
				// TODO: Replace icon but how

				return false;
			}
		}

		return true;
	}

}
