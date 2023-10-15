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
		public SteamVR_Action_Boolean action;
		public SteamVR_Input_Sources hand;

		public bool state;

		public VRBinding(SteamVR_Action_Boolean action, SteamVR_Input_Sources hand)
		{
			this.action = action;
			this.hand = hand;
		}

		public override bool Equals(object obj)
		{
			if (obj is VRBinding otherVRBinding)
			{
				return otherVRBinding.action == action && otherVRBinding.hand == hand;
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return hand.GetHashCode() + action.GetHashCode() * 15;
		}
	}

	public static Vector2 LeftThumbStick { get; private set; }
	public static Vector2 RightThumbStick { get; private set; }

	public static Dictionary<Key, VRBinding> KeyMapping = new()
	{
		{ Key.F, new VRBinding(SteamVR_Actions._default.A_Left, SteamVR_Input_Sources.LeftHand) },
		{ Key.X, new VRBinding(SteamVR_Actions._default.B_Left, SteamVR_Input_Sources.LeftHand) },
		{ Key.E, new VRBinding(SteamVR_Actions._default.A_Right, SteamVR_Input_Sources.RightHand) },
		{ Key.Q, new VRBinding(SteamVR_Actions._default.B_Right, SteamVR_Input_Sources.RightHand) },
		{ Key.Z, new VRBinding(SteamVR_Actions._default.LeftTrigger, SteamVR_Input_Sources.LeftHand) },
		{ Key.T, new VRBinding(SteamVR_Actions._default.RightTrigger, SteamVR_Input_Sources.RightHand) },
		// TODO: need to do mouse wheel to triggers
		// Should map controller stuff instead
	};

	public static Dictionary<VRBinding, bool> State = new();

	public void Awake()
	{
		SteamVR_Actions._default.LeftTrigger.AddOnStateDownListener(ResetPositionButton, SteamVR_Input_Sources.Any);

		SteamVR_Actions._default.LeftHandPose.AddOnUpdateListener(SteamVR_Input_Sources.Any, LeftHandUpdate);
		SteamVR_Actions._default.RightHandPose.AddOnUpdateListener(SteamVR_Input_Sources.Any, RightHandUpdate);

		SteamVR_Actions._default.RightThumbStick.AddOnUpdateListener(RightThumbStickUpdate, SteamVR_Input_Sources.RightHand);
		SteamVR_Actions._default.LeftThumbStick.AddOnUpdateListener(LeftThumbStickUpdate, SteamVR_Input_Sources.LeftHand);

		foreach (var vrButton in KeyMapping.Values)
		{
			State.Add(vrButton, false);

			vrButton.action.AddOnStateDownListener(VRButtonUpdate_Pressed, vrButton.hand);
			vrButton.action.AddOnStateUpListener(VRButtonUpdate_Released, vrButton.hand);
		}
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
	
	private void LeftThumbStickUpdate(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
	{
		LeftThumbStick = axis;
	}
}
