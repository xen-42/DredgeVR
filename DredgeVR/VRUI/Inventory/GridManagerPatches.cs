using DredgeVR.VRInput;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using Winch.Core;

namespace DredgeVR.VRUI.Patches;

[HarmonyPatch(typeof(GridManager))]
public static class GridManagerPatches
{
	/// <summary>
	/// Could have been a transpiler but those are scary
	/// Instead of doing a graphics raycast through to the cursor position 
	/// we just use the same raycasting we have elsewhere
	/// </summary>
	/// <param name="__instance"></param>
	/// <returns></returns>
	[HarmonyPrefix]
	[HarmonyPatch(nameof(GridManager.DoHitTestFromCursorProxy))]
	public static bool GridManager_DoHitTestFromCursorProxy(GridManager __instance, out GridObject gridObjectHit, out GridCell gridCellHit)
	{
		gridObjectHit = null;
		gridCellHit = null;

		var eventData = new PointerEventData(EventSystem.current);
		var raycastCamera = VRInputModule.Instance.RaycastCamera;
		eventData.position = new Vector2(raycastCamera.pixelWidth / 2, raycastCamera.pixelHeight / 2);
		__instance.results.Clear();

		GameManager.Instance.GridManager.GraphicRaycaster.Raycast(eventData, __instance.results);

		foreach (var result in __instance.results)
		{
			if (__instance.gridCellLayer.Contains(result.gameObject.layer))
			{
				var component = result.gameObject.GetComponent<GridCell>();
				if (component)
				{
					gridCellHit = component;
					if (component.OccupyingObject)
					{
						gridObjectHit = component.OccupyingObject;
					}
				}
			}
		}

		return false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(nameof(GridManager.ObjectPickedUp))]
	public static void GridManager_ObjectPickedUp(GridManager __instance)
	{
		// By default it's 0 and the held item is invisible
		__instance.heldItemContainer.GetComponent<RectTransform>().sizeDelta = __instance.currentlyHeldObject.GetComponent<RectTransform>().sizeDelta;
	}

	[HarmonyPostfix]
	[HarmonyPatch(nameof(GridManager.LateUpdate))]
	public static void GridManager_LateUpdate(GridManager __instance)
	{
		if (__instance.currentlyHeldObject)
		{
			__instance.currentlyHeldObject.transform.position = GameManager.Instance.GridManager.CursorProxy.CursorSquare.transform.position;
		}
	}
}