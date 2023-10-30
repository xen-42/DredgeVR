using Cinemachine.Utility;
using DredgeVR.Helpers;
using DredgeVR.VRCamera;
using DredgeVR.VRInput;
using DredgeVR.VRUI;
using HarmonyLib;
using UnityEngine;

namespace DredgeVR.Ability.Patches;

[HarmonyPatch(typeof(AbilityRadial))]
public static class AbilityRadialPatches
{
	private static Vector3 _originalRelativeHandPosition;
	private static VRHand _hand;

	[HarmonyPostfix]
	[HarmonyPatch(nameof(AbilityRadial.ShowRadial))]
	public static void AbilityRadial_ShowRadial(AbilityRadial __instance)
	{
		// What hand is the button on?
		var handName = VRInputManager.PlayerActionBindings[GameManager.Instance.Input.Controls.RadialSelectShow].action.GetHandName();
		_hand = handName == "left" ? VRCameraManager.LeftHand : VRCameraManager.RightHand;

		// Use the raycast camera pointing direction
		__instance.transform.position = _hand.RaycastCamera.transform.position + _hand.RaycastCamera.transform.forward * 0.25f;
		// Thing is mirrored, has to look away from the camera not towards it
		__instance.transform.LookAt(__instance.transform.position + _hand.RaycastCamera.transform.forward, Vector3.up);

		_originalRelativeHandPosition = __instance.transform.InverseTransformPoint(_hand.RaycastCamera.transform.position).normalized;

		VRUIManager.HideHeldUI();
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(AbilityRadial.HideRadial))]
	public static void AbilityRadial_HideRadial(AbilityRadial __instance)
	{
		// Gets called multiple times even when it's already hidden
		if (!GameManager.Instance.UI.IsShowingRadialMenu)
		{
			return;
		}

		VRUIManager.ShowHeldUI();
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(AbilityRadial.Update))]
	public static bool AbilityRadial_Update(AbilityRadial __instance)
	{
		// Replaces the original method since it just has cases for using the controller and then using the mouse

		// This is from the original method
		if (!GameManager.Instance.UI.IsShowingRadialMenu)
		{
			return false;
		}
		__instance.angleHelper = 0.0f;
		__instance.hasChangedHelper = false;

		// This is new code

		var currentHandPosition = __instance.transform.InverseTransformPoint(_hand.RaycastCamera.transform.position);
		var projectedHandPosition = currentHandPosition.ProjectOntoPlane(_originalRelativeHandPosition);
		// Should give the hand position in a 2D plane with origin at the radial UI
		var differenceInPlane = Quaternion.FromToRotation(_originalRelativeHandPosition, Vector3.forward) * projectedHandPosition;

		// Y direction is mirrored
		var moveInput = new Vector2(differenceInPlane.x, -differenceInPlane.y);
		moveInput.Normalize();

		// This is from the original method for handling mouse inputs
		if (moveInput == Vector2.zero)
		{
			__instance.hasChangedHelper = false;
		}
		else if (Vector2.Distance(Input.mousePosition, __instance.transform.position) < __instance.mouseDeadzoneSize)
		{
			__instance.hasChangedHelper = false;
		}
		else
		{
			__instance.angleHelper = Mathf.Atan2(moveInput.y, -moveInput.x) / 3.14159274f;
			__instance.angleHelper *= 180f;
			__instance.angleHelper -= 90f;
			if (__instance.angleHelper < 0.0)
			{
				__instance.angleHelper += 360f;
			}
			__instance.hasChangedHelper = true;
		}

		// This is from the original method
		int newIndex = Mathf.RoundToInt((float)MathUtil.RoundToStep(Mathf.RoundToInt(__instance.angleHelper), Mathf.RoundToInt(__instance.wedgeWidthHelper)) / __instance.wedgeWidthHelper) % __instance.numAbilitiesEnabled;
		if (newIndex == __instance.currentIndex)
		{
			return false;
		}

		GameManager.Instance.AudioPlayer.PlaySFX(__instance.selectSFX, AudioLayer.SFX_UI);
		__instance.ChangeCurrentIndex(newIndex);

		return false;
	}
}
