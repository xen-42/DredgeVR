using HarmonyLib;
using UnityEngine;

namespace DredgeVR.VRUI.Patches;

/// <summary>
/// This is the focus/highlight rectangle that appears over the currently hovered UI
/// </summary>
[HarmonyPatch(typeof(UIFocusObject))]
public static class UIFocusObjectPatches
{
	[HarmonyPostfix]
	[HarmonyPatch(nameof(UIFocusObject.Update))]
	public static void UIFocusObject_Update(UIFocusObject __instance)
	{
		if (__instance.hasTargetObject)
		{
			__instance.transform.position = __instance.targetObject.transform.TransformPoint(__instance.targetObject.GetComponent<RectTransform>().rect.center);
			__instance.transform.rotation = __instance.targetObject.transform.rotation;
			__instance.container.transform.localPosition = Vector3.zero;
			var scaleFactor = __instance.targetObject.transform.lossyScale.x / __instance.transform.lossyScale.x;
			__instance.container.transform.localScale = Vector3.one * scaleFactor;
		}
	}
}
