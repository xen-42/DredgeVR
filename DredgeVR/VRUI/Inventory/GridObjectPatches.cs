﻿using DredgeVR.Helpers;
using DredgeVR.VRCamera;
using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace DredgeVR.VRUI.Patches;

/// <summary>
/// These are for the items in the inventory grid spots
/// </summary>
[HarmonyPatch(typeof(GridObject))]
public static class GridObjectPatches
{
	[HarmonyPostfix]
	[HarmonyPatch(nameof(GridObject.FixedUpdate))]
	public static void GridObject_FixedUpdate(GridObject __instance)
	{
		// The GridManager controls the position of the held item if its picked up
		if (!__instance.isPickedUp)
		{
			__instance.transform.localPosition = new Vector3(__instance.transform.localPosition.x, __instance.transform.localPosition.y, 0f);
		}

		__instance.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, __instance.CurrentRotation);

		// Without this, they have constant world space rotation which gets messed up if the boat has turned
		var z = VRCameraManager.AnchorTransform.transform.forward.normalized;
		var y = Quaternion.AngleAxis(__instance.currentRotation, z) * Vector3.up;

		__instance.gridObjectImage.transform.rotation = Quaternion.LookRotation(z, y);
	}

	[HarmonyPostfix]
	[HarmonyPatch(nameof(GridObject.Init))]
	public static void GridObject_Init(GridObject __instance)
	{
		// The GridObjectCells spawn in the wrong locations, they use global space
		// Need to put them in the proper positions
		// Also need to factor in the scale of the world space canvas
		var cellSize = GameManager.Instance.GridManager.ScaledCellSize / __instance.transform.lossyScale.x;
		
		// Hacky but the infected cell isn't stored anywhere after being generated
		var infectedCells = __instance.transform.GetComponentsInChildren<Transform>()
			.Where(x => x.name.Contains(__instance.infectedObjectCellPrefab.name))
			.ToArray();

		var isInfected = infectedCells.Any();

		for (int i = 0; i < __instance.ItemData.dimensions.Count; i++)
		{
			var cell = __instance.ItemData.dimensions[i];
			var localX = (cell.x * cellSize) + (cellSize / 2f);
			var localY = (cell.y * cellSize) + (cellSize / 2f);

			var localPosition = new Vector3(localX, -localY, 0f);

			__instance.gridObjectCells[i].transform.localPosition = localPosition;

			if (isInfected)
			{
				// The lengths must always be equal
				infectedCells[i].localPosition = localPosition;

				// By default its billboard (looks at the player hands)
				var psr = infectedCells[i].GetComponentInChildren<ParticleSystemRenderer>();
				psr.mesh = AssetLoader.PrimitiveQuad;
				psr.renderMode = ParticleSystemRenderMode.Mesh;
				psr.alignment = ParticleSystemRenderSpace.Local;
			}
		}
	}
}
