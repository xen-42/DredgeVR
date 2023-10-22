using DredgeVR.VRInput;
using HarmonyLib;
using InControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace DredgeVR.VRUI.Patches;


// Todo: Replace this with adding our own sprites into an asset bundle for TMP to use
[HarmonyPatch(typeof(LanguageManager))]
public static class LanguageManagerPatches
{
	[HarmonyPrefix]
	[HarmonyPatch(nameof(LanguageManager.FormatControlPromptString), new Type[] { typeof(LocalizedString), typeof(List<DredgeControlEnum>)})]
	public static bool LanguageManager_FormatControlPromptString(LanguageManager __instance, LocalizedString localizedString, List<DredgeControlEnum> controls, ref string __result)
	{
		List<string> arguments = new List<string>();
		controls.ForEach((Action<DredgeControlEnum>)(dce =>
		{
			PlayerAction playerAction = GameManager.Instance.Input.Controls.GetPlayerAction(dce);
			// Instead of using the control icon sprite name put in the written name
			// Besides this, this function is the same as stock
			var vrBinding = (VRBindingSource)playerAction.Bindings.First(x => x is VRBindingSource);
			if (vrBinding != null)
			{
				arguments.Add(vrBinding.action.GetShortName());
			}
			else
			{
				arguments.Add(" <sprite name=\"" + GameManager.Instance.Input.GetControlIconForActionWithDefault(playerAction).upSpriteName + "\"> ");
			}

		}));
		__result = LocalizationSettings.StringDatabase.GetLocalizedString(localizedString.TableEntryReference, LocalizationSettings.SelectedLocale, FallbackBehavior.UseProjectSettings, (object[])arguments.ToArray());
		return false;
	}
}
