using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		var raycastCamera = VRInputModule.Instance.RaycastCamera;
		eventData.position = new Vector2(raycastCamera.pixelWidth / 2, raycastCamera.pixelHeight / 2);

		var resultAppendList = new List<RaycastResult>();
		GameManager.Instance.GridManager.GraphicRaycaster.Raycast(eventData, resultAppendList);

		foreach (var raycastResult in resultAppendList)
		{
			if (__instance.slotLayer.Contains(raycastResult.gameObject.layer))
			{
				GridCell component = raycastResult.gameObject.GetComponent<GridCell>();
				if (component)
				{
					cellHit = component;
					if (component.OccupyingObject)
						objectHit = component.OccupyingObject;
				}
			}
		}

		return false;
	}
}
