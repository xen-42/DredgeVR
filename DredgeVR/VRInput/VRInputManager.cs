using DredgeVR.Helpers;
using DredgeVR.VRCamera;
using InControl;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;

namespace DredgeVR.VRInput;

public class VRInputManager : MonoBehaviour
{
	public static VRInputManager Instance { get; private set; }

	public static PlayerAction ResetCamera { get; private set; }

	public static Vector2 LeftThumbStick { get; private set; }
	public static Vector2 RightThumbStick { get; private set; }

	private Dictionary<SteamVR_Action_Boolean, bool> _state = new();

	public void Awake()
	{
		Instance = this;

		SteamVR_Actions._default.LeftHandPose.AddOnUpdateListener(SteamVR_Input_Sources.LeftHand, LeftHandUpdate);
		SteamVR_Actions._default.RightHandPose.AddOnUpdateListener(SteamVR_Input_Sources.RightHand, RightHandUpdate);

		SteamVR_Actions._default.Move.AddOnUpdateListener(RightThumbStickUpdate, SteamVR_Input_Sources.Any);
		SteamVR_Actions._default.RadialSelect.AddOnUpdateListener(LeftThumbStickUpdate, SteamVR_Input_Sources.Any);

		DredgeVRLogger.Debug($"Commands are: {string.Join(", ", SteamVR_Actions._default.allActions.Select(x => x.GetShortName()))}");

		foreach (var action in SteamVR_Actions._default.allActions)
		{
			if (action is SteamVR_Action_Boolean boolAction)
			{
				try
				{
					_state.Add(boolAction, false);

					// Have to listen on both hands, doing Any doesn't work
					boolAction.AddOnStateDownListener(VRButtonUpdate_Pressed, SteamVR_Input_Sources.LeftHand);
					boolAction.AddOnStateDownListener(VRButtonUpdate_Pressed, SteamVR_Input_Sources.RightHand);

					boolAction.AddOnStateUpListener(VRButtonUpdate_Released, SteamVR_Input_Sources.LeftHand);
					boolAction.AddOnStateUpListener(VRButtonUpdate_Released, SteamVR_Input_Sources.RightHand);

					DredgeVRLogger.Debug($"Added listener to {action.GetShortName()}");
				}
				catch (Exception e)
				{
					DredgeVRLogger.Error($"Could not add listener to action {action.GetShortName()} : {e}");
				}
			}
		}

		DredgeVRCore.TitleSceneStart += InitControls;
	}

	/// <summary>
	/// Timing on this is so wack
	/// </summary>
	private void InitControls()
	{
		DredgeVRCore.TitleSceneStart -= InitControls;

		var cancel = SteamVR_Actions._default.Cancel;

		Delay.FireInNUpdates(10, () =>
		{
			AddNewBinding(GameManager.Instance.Input.Controls.Undock, cancel); // X
			AddNewBinding(GameManager.Instance.Input.Controls.Back, cancel); // X
			AddNewBinding(GameManager.Instance.Input.Controls.Skip, cancel); // Escape

			AddNewBinding(GameManager.Instance.Input.Controls.RadialSelectShow, SteamVR_Actions._default.RadialSelectShow); // E

			AddNewBinding(GameManager.Instance.Input.Controls.DiscardItem, SteamVR_Actions._default.DiscardItem); // Mouse2
			AddNewBinding(GameManager.Instance.Input.Controls.DoAbility, SteamVR_Actions._default.DoAbility); // Mouse2

			AddNewBinding(GameManager.Instance.Input.Controls.Interact, SteamVR_Actions._default.Interact); // F
			AddNewBinding(GameManager.Instance.Input.Controls.Reel, SteamVR_Actions._default.Reel); // F
			AddNewBinding(GameManager.Instance.Input.Controls.SellItem, SteamVR_Actions._default.SellItem); // F
			AddNewBinding(GameManager.Instance.Input.Controls.BuyItem, SteamVR_Actions._default.BuyItem); // F

			AddNewBinding(GameManager.Instance.Input.Controls.Confirm, SteamVR_Actions._default.Confirm); // Mouse1
			AddNewBinding(GameManager.Instance.Input.Controls.PickUpPlace, SteamVR_Actions._default.Confirm); // Mouse1

			AddNewBinding(GameManager.Instance.Input.Controls.RotateClockwise, SteamVR_Actions._default.RotateClockwise); // One Axis
			AddNewBinding(GameManager.Instance.Input.Controls.RotateCounterClockwise, SteamVR_Actions._default.RotateCounterClockwise); // One Axis

			AddNewBinding(GameManager.Instance.Input.Controls.ToggleCargo, SteamVR_Actions._default.ToggleCargo); // Tab

			AddNewBinding(GameManager.Instance.Input.Controls.Pause, SteamVR_Actions._default.Pause); // Escape
			AddNewBinding(GameManager.Instance.Input.Controls.Unpause, SteamVR_Actions._default.Pause); // Escape

			//CreatePlayerAction("reset-camera", "Reset Camera", OptionsManager.Options.leftHanded ? LeftThumbStickButton : RightThumbStickButton, VRCameraManager.Instance.ResetPosition);
		});
	}

	/*
	private PlayerAction CreatePlayerAction(string id, string name, VRBinding binding, Action onPress)
	{
		// TODO: Why doesnt this work
		try
		{
			var playerAction = new PlayerAction(id, GameManager.Instance.Input.Controls);
			playerAction.AddDefaultBinding(new VRBindingSource(binding));
			playerAction.ResetBindings();

			var dredgePlayerAction = new DredgePlayerActionPress(name, playerAction);
			dredgePlayerAction.OnPressComplete += onPress;
			GameManager.Instance.Input.AddActionListener(new DredgePlayerActionBase[] { dredgePlayerAction }, ActionLayer.PERSISTENT);

			DredgeVRLogger.Info($"Created custom player action {id}");

			return playerAction;
		}
		catch (Exception e)
		{
			DredgeVRLogger.Error($"Couldn't create player action {id} : {e}");
			return null;
		}
	}
	*/

	private void AddNewBinding(PlayerAction action, SteamVR_Action_Boolean vrAction)
	{
		action.AddDefaultBinding(new VRBindingSource(vrAction));
		action.ResetBindings();
	}

	private void VRButtonUpdate_Pressed(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
	{
		DredgeVRLogger.Info($"Pressed button {fromAction.GetShortName()}");
		_state[fromAction] = true;
	}

	private void VRButtonUpdate_Released(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
	{
		DredgeVRLogger.Info($"Released button {fromAction.GetShortName()}");
		_state[fromAction] = false;
	}

	public static bool IsVRActionPressed(SteamVR_Action_Boolean action)
	{
		if (Instance._state.TryGetValue(action, out var result))
		{
			return result;
		}
		return false;
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
