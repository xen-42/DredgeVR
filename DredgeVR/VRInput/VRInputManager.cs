using DredgeVR.VRCamera;
using UnityEngine;
using Valve.VR;

namespace DredgeVR.VRInput;

public class VRInputManager : MonoBehaviour
{
	public static Vector2 RightThumbStick { get; private set; }

	public void Awake()
	{
		SteamVR_Actions._default.LeftTrigger.AddOnStateDownListener(ResetPositionButton, SteamVR_Input_Sources.Any);

		SteamVR_Actions._default.LeftHandPose.AddOnUpdateListener(SteamVR_Input_Sources.Any, LeftHandUpdate);
		SteamVR_Actions._default.RightHandPose.AddOnUpdateListener(SteamVR_Input_Sources.Any, RightHandUpdate);

		SteamVR_Actions._default.RightThumbStick.AddOnUpdateListener(RightThumbStickUpdate, SteamVR_Input_Sources.RightHand);
	}

	private void ResetPositionButton(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
	{
		VRCameraManager.Instance.ResetPosition();
	}

	private void LeftHandUpdate(SteamVR_Action_Pose fromAction, SteamVR_Input_Sources fromSource)
	{
		if (VRCameraManager.LeftHand)
		{
			VRCameraManager.LeftHand.transform.localPosition = SteamVR_Actions._default.LeftHandPose.localPosition;
			VRCameraManager.LeftHand.transform.localRotation = SteamVR_Actions._default.LeftHandPose.localRotation;
		}
	}
	private void RightHandUpdate(SteamVR_Action_Pose fromAction, SteamVR_Input_Sources fromSource)
	{
		if (VRCameraManager.RightHand)
		{
			VRCameraManager.RightHand.transform.localPosition = SteamVR_Actions._default.RightHandPose.localPosition;
			VRCameraManager.RightHand.transform.localRotation = SteamVR_Actions._default.RightHandPose.localRotation;
		}
	}

	private void RightThumbStickUpdate(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
	{
		RightThumbStick = axis;
	}
}
