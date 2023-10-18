using DG.Tweening.Core.Easing;
using DredgeVR.Helpers;
using DredgeVR.VRCamera;
using InControl;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

	public static VRBinding LeftHandA = new(SteamVR_Actions._default.A_Left, SteamVR_Input_Sources.LeftHand);
	public static VRBinding LeftHandB = new(SteamVR_Actions._default.B_Left, SteamVR_Input_Sources.LeftHand);
	public static VRBinding RightHandA = new(SteamVR_Actions._default.A_Right, SteamVR_Input_Sources.RightHand);
	public static VRBinding RightHandB = new(SteamVR_Actions._default.B_Right, SteamVR_Input_Sources.RightHand);
	public static VRBinding LeftTrigger = new(SteamVR_Actions._default.LeftTrigger, SteamVR_Input_Sources.LeftHand);
	public static VRBinding RightTrigger = new(SteamVR_Actions._default.RightTrigger, SteamVR_Input_Sources.RightHand);

	public static VRBinding LeftStart = new(SteamVR_Actions._default.LeftStart, SteamVR_Input_Sources.LeftHand);
	public static VRBinding LeftThumbStickButton = new(SteamVR_Actions._default.LeftThumbStickPress, SteamVR_Input_Sources.LeftHand);
	public static VRBinding RightThumbStickButton = new(SteamVR_Actions._default.RightThumbStickPress, SteamVR_Input_Sources.RightHand);

	public static Dictionary<VRBinding, bool> State = new();

	public Dictionary<VRBinding, ControlIconData> ControlIcons = new();

	public void Awake()
	{
		SteamVR_Actions._default.LeftHandPose.AddOnUpdateListener(SteamVR_Input_Sources.Any, LeftHandUpdate);
		SteamVR_Actions._default.RightHandPose.AddOnUpdateListener(SteamVR_Input_Sources.Any, RightHandUpdate);

		SteamVR_Actions._default.RightThumbStick.AddOnUpdateListener(RightThumbStickUpdate, SteamVR_Input_Sources.RightHand);
		SteamVR_Actions._default.LeftThumbStick.AddOnUpdateListener(LeftThumbStickUpdate, SteamVR_Input_Sources.LeftHand);

		foreach (var vrButton in typeof(VRInputManager).GetFields(BindingFlags.Public | BindingFlags.Static).Where(x => x.FieldType == typeof(VRBinding)).Select(x => (VRBinding)x.GetValue(null)))
		{
			State.Add(vrButton, false);

			vrButton.action.AddOnStateDownListener(VRButtonUpdate_Pressed, vrButton.hand);
			vrButton.action.AddOnStateUpListener(VRButtonUpdate_Released, vrButton.hand);
		}

		DredgeVRCore.TitleSceneStart += InitControls;
	}

	/// <summary>
	/// Timing on this is so wack
	/// </summary>
	private void InitControls()
	{
		DredgeVRCore.TitleSceneStart -= InitControls;

		var defaultCancelBinding = RightHandB;
		var defaultAcceptBinding = RightHandA;

		Delay.FireInNUpdates(10, () =>
		{
			ReplaceBinding(GameManager.Instance.Input.Controls.Undock, defaultCancelBinding); // X
			ReplaceBinding(GameManager.Instance.Input.Controls.Back, defaultCancelBinding); // X
			ReplaceBinding(GameManager.Instance.Input.Controls.Skip, defaultCancelBinding); // Escape

			ReplaceBinding(GameManager.Instance.Input.Controls.RadialSelectShow, LeftTrigger); // E
			ReplaceBinding(GameManager.Instance.Input.Controls.DiscardItem, LeftTrigger); // Mouse2
			ReplaceBinding(GameManager.Instance.Input.Controls.DoAbility, LeftTrigger); // Mouse2

			ReplaceBinding(GameManager.Instance.Input.Controls.Interact, defaultAcceptBinding); // F
			ReplaceBinding(GameManager.Instance.Input.Controls.Reel, defaultAcceptBinding); // F
			ReplaceBinding(GameManager.Instance.Input.Controls.SellItem, defaultAcceptBinding); // F
			ReplaceBinding(GameManager.Instance.Input.Controls.Confirm, defaultAcceptBinding); // Mouse1

			ReplaceBinding(GameManager.Instance.Input.Controls.PickUpPlace, RightTrigger); // Mouse1

			ReplaceBinding(GameManager.Instance.Input.Controls.RotateClockwise, LeftThumbStickButton); // One Axis
			ReplaceBinding(GameManager.Instance.Input.Controls.RotateCounterClockwise, RightThumbStickButton); // One Axis

			ReplaceBinding(GameManager.Instance.Input.Controls.ToggleCargo, LeftHandB); // Tab

			ReplaceBinding(GameManager.Instance.Input.Controls.Pause, LeftHandA); // Escape
			ReplaceBinding(GameManager.Instance.Input.Controls.Unpause, LeftHandA); // Escape
		});
	}

	private void ReplaceBinding(PlayerAction action, VRBinding replacement)
	{
		var defaultBindings = action.GetValue<List<BindingSource>>("defaultBindings");
		defaultBindings.Clear();
		action.AddDefaultBinding(new VRBindingSource(replacement));
		action.ResetBindings();
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
