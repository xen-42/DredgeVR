using DredgeVR.VRCamera;
using InControl;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Winch.Core;

namespace DredgeVR.VRInput;

public class VRInputManager : MonoBehaviour
{
	public struct VRBinding
	{
		public SteamVR_Action_Boolean vrAction;
		public SteamVR_Input_Sources hand;

		public bool state;

		public VRBinding(SteamVR_Action_Boolean vrAction, SteamVR_Input_Sources hand)
		{
			this.vrAction = vrAction;
			this.hand = hand;
		}
	}

	public static Vector2 RightThumbStick { get; private set; }

	public static Dictionary<Key, VRBinding> KeyMapping = new()
	{
		{ Key.F, new VRBinding(SteamVR_Actions._default.A, SteamVR_Input_Sources.LeftHand) },
		{ Key.X, new VRBinding(SteamVR_Actions._default.B, SteamVR_Input_Sources.LeftHand) },
		{ Key.E, new VRBinding(SteamVR_Actions._default.A, SteamVR_Input_Sources.RightHand) },
		{ Key.Q, new VRBinding(SteamVR_Actions._default.B, SteamVR_Input_Sources.RightHand) }
	};

	public static Dictionary<VRBinding, bool> State = new();

	public void Awake()
	{
		SteamVR_Actions._default.LeftTrigger.AddOnStateDownListener(ResetPositionButton, SteamVR_Input_Sources.Any);

		SteamVR_Actions._default.LeftHandPose.AddOnUpdateListener(SteamVR_Input_Sources.Any, LeftHandUpdate);
		SteamVR_Actions._default.RightHandPose.AddOnUpdateListener(SteamVR_Input_Sources.Any, RightHandUpdate);

		SteamVR_Actions._default.RightThumbStick.AddOnUpdateListener(RightThumbStickUpdate, SteamVR_Input_Sources.RightHand);

		foreach (var vrButton in KeyMapping.Values)
		{
			State.Add(vrButton, false);
			vrButton.vrAction.AddOnStateDownListener(VRButtonUpdate_Pressed, SteamVR_Input_Sources.Any);
			vrButton.vrAction.AddOnStateUpListener(VRButtonUpdate_Released, SteamVR_Input_Sources.Any);
		}

		SteamVR_Actions._default.LeftTrigger.AddOnStateDownListener(VRButtonUpdate_Pressed, SteamVR_Input_Sources.Any);
	}

	private void VRButtonUpdate_Pressed(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
	{
		WinchCore.Log.Info($"Pressed button {fromAction.GetShortName()}");
		State[new VRBinding(fromAction, fromSource)] = true;
	}

	private void VRButtonUpdate_Released(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
	{
		WinchCore.Log.Info($"Released button {fromAction.GetShortName()}");
		State[new VRBinding(fromAction, fromSource)] = false;
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
