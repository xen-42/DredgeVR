using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DredgeVR.VRInput.Patches;

[HarmonyPatch(typeof(GridObjectCell))]
public static class GridObjectCellPatches
{
	/// <summary>
	/// Much like with GridManagerPatches, we have to use the graphics raycast from our hand instead of from the screen
	/// </summary>
	/// <param name="__instance"></param>
	/// <param name="cellHit"></param>
	/// <param name="objectHit"></param>
	/// <returns></returns>
	[HarmonyPrefix]
	[HarmonyPatch(nameof(GridObjectCell.DoHitTest))]
	public static bool GridObjectCell_DoHitTest(GridObjectCell __instance, out GridCell cellHit, out GridObject objectHit)
	{
		cellHit = null;
		objectHit = null;

		var eventData = new PointerEventData(EventSystem.current);
		// Gonna do some cursed stuff with the raycast camera
		// This is so we can get a straight raycast into the canvas
		var raycastCamera = VRInputModule.Instance.RaycastCamera;
		var cameraOrigPosition = raycastCamera.transform.position;
		var cameraOrigRotation = raycastCamera.transform.rotation;
		// For the grid objects, their forward local vector is into the screen
		// Position the camera facing the screen slightly in front of it
		raycastCamera.transform.position = __instance.transform.position - 0.1f * __instance.transform.forward;
		raycastCamera.transform.LookAt(__instance.transform);
		eventData.position = new Vector2(raycastCamera.pixelWidth / 2, raycastCamera.pixelHeight / 2);

		var resultAppendList = new List<RaycastResult>();
		GameManager.Instance.GridManager.GraphicRaycaster.Raycast(eventData, resultAppendList);

		// Put the camera back
		raycastCamera.transform.position = cameraOrigPosition;
		raycastCamera.transform.rotation = cameraOrigRotation;

		foreach (var raycastResult in resultAppendList)
		{
			if (__instance.slotLayer.Contains(raycastResult.gameObject.layer))
			{
				var component = raycastResult.gameObject.GetComponent<GridCell>();
				if (component)
				{
					cellHit = component;
					if (component.OccupyingObject)
					{
						objectHit = component.OccupyingObject;
					}
				}
			}
		}

		return false;
	}
}
