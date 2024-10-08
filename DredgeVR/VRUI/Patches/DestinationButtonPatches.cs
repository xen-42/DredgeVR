﻿using Cinemachine.Utility;
using DredgeVR.Helpers;
using DredgeVR.Options;
using HarmonyLib;
using UnityEngine;

namespace DredgeVR.VRUI.Patches;

/// <summary>
/// These are the buttons for the fishmonger, dry dock, etc, in port
/// </summary>
[HarmonyPatch]
public static class DestinationButtonPatches
{
	private static float _cachedMaxDistance;
	private static float _cachedMinDistance;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(DockUI), nameof(DockUI.ClearUI))]
	public static void DockUI_ClearUI()
	{
		// Clear cache when the UI gets cleared
		_cachedMaxDistance = float.MinValue;
		_cachedMinDistance = float.MaxValue;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DestinationButton), nameof(DestinationButton.LateUpdate))]
	public static bool DestinationButton_LateUpdate(DestinationButton __instance)
	{
		// Originally these buttons place themself to be at the screen point location corresponding to their real world destination location
		// Want them to be moved up and a bit towards the camera, since a lot clip into walls and stuff
		// The farther away they are, the farther up they can go to ensure closer ones dont block them

		// This is super scuffed but seems to work well enough

		var directionToPlayer = (__instance.destination.transform.position - GameManager.Instance.Player.transform.position).ProjectOntoPlane(Vector3.up);
		var distance = directionToPlayer.magnitude;

		// We will show the buttons on a curve based on distance to player
		if (distance > _cachedMaxDistance)
		{
			_cachedMaxDistance = distance;
		}
		if (distance < _cachedMinDistance)
		{
			_cachedMinDistance = distance;
		}

		// Avoid dividing by zero
		var normalizedDistanceModifier = _cachedMaxDistance == _cachedMinDistance ? 0 : Mathf.Clamp01((distance - _cachedMinDistance) / (_cachedMaxDistance - _cachedMinDistance));
		var modifier = normalizedDistanceModifier * normalizedDistanceModifier;

		var yOffset = Vector3.up * (1 + modifier);
		var planeOffset = directionToPlayer.normalized * -1f * (1 + modifier);
		var scaleModifier = 10 * (1 + normalizedDistanceModifier);

		// For the pontoon, they're all so close that scaling them looks really off
		if (GameManager.Instance.Player.CurrentDock.name.Contains("Pontoon"))
		{
			scaleModifier = 6f;
			if (__instance.destination.name == "Fish Market")
			{
				yOffset *= -0.5f;
			}
			else if (__instance.destination.name == "Storage")
			{
				yOffset *= 0.75f;
			}
			else if (__instance.destination.name == "Icy Worksite")
			{
				scaleModifier = 12f;
			}
		}
		// Kinda gross but can't think of a good generic way to fix this one, always blocks the fanatic button
		else if (GameManager.Instance.Player.CurrentDock.name == "Temple" && __instance.destination.name == "Storage")
		{
			yOffset *= 0.5f;
		}
		// For the cave, move it out more so we can actually see it
		else if (__instance.destination.name == "Cave")
		{
			planeOffset *= 3f;
		}
		else if (GameManager.Instance.Player.CurrentDock.name.StartsWith("TPR "))
		{
			// TPR stuff is all way too high up in the air, since we tend to be docked very close 
			yOffset *= 0.33f;
			if (__instance.destination.name == "Ice Shard")
			{
				yOffset *= 0.5f;
				scaleModifier *= 0.5f;
			}
			// Again since we're docked so close set a max size
			scaleModifier = Mathf.Min(scaleModifier, 12f);
		}
		else if (GameManager.Instance.Player.CurrentDock.name.Contains("Iron Rig"))
		{
			scaleModifier *= 0.3f;
			if (__instance.destination.name.Contains("RigBase"))
			{
				planeOffset *= 2.5f;
				yOffset = Vector3.zero;
			}
		}

		if (OptionsManager.Options.playerScale > 1)
		{
			yOffset *= OptionsManager.Options.playerScale;
			scaleModifier /= OptionsManager.Options.playerScale;
		}

		__instance.transform.position = __instance.destination.transform.position + yOffset + planeOffset;
		__instance.transform.localScale = Vector3.one * scaleModifier;

		DirectionHelper.LookAtPlayerInPlane(__instance.transform, true);
		return false;
	}
}
