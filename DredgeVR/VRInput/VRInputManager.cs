using DredgeVR.Helpers;
using DredgeVR.VRCamera;
using DredgeVR.VRInput.VRBindingSource;
using InControl;
using System;
using System.Collections.Generic;
using System.Linq;
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

		public VRBinding(SteamVR_Action_Boolean vrAction, SteamVR_Input_Sources hand)
		{
			this.vrAction = vrAction;
			this.hand = hand;
		}
	}

	public static Vector2 RightThumbStick { get; private set; }

	private Dictionary<Key, VRBinding> _keyMapping = new()
	{
		{ Key.F, new VRBinding(SteamVR_Actions._default.A, SteamVR_Input_Sources.LeftHand) },
		{ Key.X, new VRBinding(SteamVR_Actions._default.B, SteamVR_Input_Sources.LeftHand) },
		{ Key.E, new VRBinding(SteamVR_Actions._default.A, SteamVR_Input_Sources.RightHand) },
		{ Key.Q, new VRBinding(SteamVR_Actions._default.B, SteamVR_Input_Sources.RightHand) }
	};

	private Dictionary<Key, List<PlayerAction>> _keyboardControls = new();


	public void Awake()
	{
		SteamVR_Actions._default.LeftTrigger.AddOnStateDownListener(ResetPositionButton, SteamVR_Input_Sources.Any);

		SteamVR_Actions._default.LeftHandPose.AddOnUpdateListener(SteamVR_Input_Sources.Any, LeftHandUpdate);
		SteamVR_Actions._default.RightHandPose.AddOnUpdateListener(SteamVR_Input_Sources.Any, RightHandUpdate);

		SteamVR_Actions._default.RightThumbStick.AddOnUpdateListener(RightThumbStickUpdate, SteamVR_Input_Sources.RightHand);

		// Modify input bindings
		foreach (var fieldInfo in typeof(DredgeControlBindings).GetFields().Where(x => x.FieldType.IsAssignableFrom(typeof(PlayerAction))))
		{
			try
			{
				var playerAction = fieldInfo.GetValue(GameManager.Instance.Input.Controls) as PlayerAction;

				var key = ((playerAction.GetValue<List<BindingSource>>("defaultBindings")).FirstOrDefault(x => x is KeyBindingSource) as KeyBindingSource).Control.GetInclude(0);

				if (_keyMapping.TryGetValue(key, out var vrBinding))
				{
					var vrBindingSource = new VRBindingSourceBoolean(vrBinding.vrAction, vrBinding.hand);
					playerAction.AddDefaultBinding(vrBindingSource);
					playerAction.AddBinding(vrBindingSource);

					playerAction.ClearBindings();
					playerAction.AddBinding(new KeyBindingSource(Key.P));

					WinchCore.Log.Info($"Added new VR control binding {key} to {vrBindingSource.Name}");
				}
			}
			catch { }
		}
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
