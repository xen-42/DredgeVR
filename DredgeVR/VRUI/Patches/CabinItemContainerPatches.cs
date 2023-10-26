using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace DredgeVR.VRUI.Patches;

[HarmonyPatch(typeof(CabinItemContainer))]
public static class CabinItemContainerPatches
{
	[HarmonyPostfix]
	[HarmonyPatch(nameof(CabinItemContainer.OnEnable))]
	public static void CabinItemContainer_OnEnable(CabinItemContainer __instance)
	{
		var itemScroller = __instance.itemEntryContainer;
		var nonSpatialItemGrid = __instance.itemEntryContainer.Find("NonSpatialItemGrid");
		itemScroller.localPosition = Vector3.down * 80;
		nonSpatialItemGrid.localPosition = Vector3.zero;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(CabinItemContainer.OnEntrySelected))]
	public static bool CabinItemContainer_OnEntrySelected(CabinItemContainer __instance, NonSpatialGridEntryUI entryUI)
	{
		if (__instance.lerpCoroutine != null)
			__instance.StopCoroutine(__instance.lerpCoroutine);
		__instance.lerpCoroutine = __instance.StartCoroutine(LerpToDestinationPos(__instance, __instance.scrollRect.GetSnapToPositionToBringChildIntoView(entryUI.gameObject.transform as RectTransform)));

		return false;
	}

	private static IEnumerator LerpToDestinationPos(CabinItemContainer cabinItemContainer, Vector2 destinationPos)
	{
		Canvas.ForceUpdateCanvases();
		cabinItemContainer.isLerpingToDestinationPos = true;
		while (cabinItemContainer.isLerpingToDestinationPos)
		{
			float t = Mathf.Min(10f * Time.deltaTime, 1f);
			// Only difference is it's a Vector3 keeping the right local z
			var vec2 = Vector2.Lerp(cabinItemContainer.scrollRect.content.anchoredPosition, destinationPos, t);
			cabinItemContainer.scrollRect.content.anchoredPosition = new Vector3(vec2.x, vec2.y, cabinItemContainer.transform.position.z);
			if (Vector2.SqrMagnitude(cabinItemContainer.scrollRect.content.anchoredPosition - destinationPos) < 10.0)
			{
				cabinItemContainer.scrollRect.content.anchoredPosition = destinationPos;
				cabinItemContainer.isLerpingToDestinationPos = false;
			}
			yield return null;
		}
		cabinItemContainer.lerpCoroutine = null;
	}
}
