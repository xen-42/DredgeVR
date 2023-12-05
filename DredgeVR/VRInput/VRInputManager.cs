using DredgeVR.Helpers;
using DredgeVR.VRCamera;
using InControl;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;

namespace DredgeVR.VRInput;

public class VRInputManager : MonoBehaviour
{
	public static Vector2 LeftThumbStick { get; private set; }
	public static Vector2 RightThumbStick { get; private set; }

	public void Awake()
	{
		SteamVR_Actions._default.LeftHandPose.AddOnUpdateListener(SteamVR_Input_Sources.LeftHand, LeftHandUpdate);
		SteamVR_Actions._default.RightHandPose.AddOnUpdateListener(SteamVR_Input_Sources.RightHand, RightHandUpdate);

		SteamVR_Actions._default.Move.AddOnUpdateListener(LeftThumbStickUpdate, SteamVR_Input_Sources.Any);
		SteamVR_Actions._default.MoveCamera.AddOnUpdateListener(RightThumbStickUpdate, SteamVR_Input_Sources.Any);

		DredgeVRLogger.Debug($"Commands are: {string.Join(", ", SteamVR_Actions._default.allActions.Select(x => x.GetShortName()))}");

		InitControls();

		// Sometimes the controls just don't work
		DredgeVRCore.TitleSceneStart += GameManager.Instance.Input.ResetAllBindings;
		DredgeVRCore.GameSceneStart += GameManager.Instance.Input.ResetAllBindings;
	}

	public void OnDestroy()
	{
		DredgeVRCore.TitleSceneStart -= GameManager.Instance.Input.ResetAllBindings;
		DredgeVRCore.GameSceneStart -= GameManager.Instance.Input.ResetAllBindings;

		// For some reason next time you play the game flat none of your controls work
		GameManager.Instance.Input.ResetAllBindings();
	}

	public static readonly Dictionary<PlayerAction, VRBindingSource> PlayerActionBindings = new();

	public static void InitControls()
	{
		var cancel = SteamVR_Actions._default.Cancel;

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

		AddNewBinding(GameManager.Instance.Input.Controls.RepairAll, SteamVR_Actions._default.RepairAll);
		AddNewBinding(GameManager.Instance.Input.Controls.RepairItem, SteamVR_Actions._default.RepairItem);
		AddNewBinding(GameManager.Instance.Input.Controls.RepairMode, SteamVR_Actions._default.RepairMode);

		AddNewBinding(GameManager.Instance.Input.Controls.ZoomMapIn, SteamVR_Actions._default.ZoomMapIn);
		AddNewBinding(GameManager.Instance.Input.Controls.ZoomMapOut, SteamVR_Actions._default.ZoomMapOut);

		AddNewBinding(GameManager.Instance.Input.Controls.RemoveMarker, SteamVR_Actions._default.RemoveMarker);

		AddNewBinding(GameManager.Instance.Input.Controls.QuickMove, SteamVR_Actions._default.QuickMove);

		AddNewBinding(GameManager.Instance.Input.Controls.OpenJournal, SteamVR_Actions._default.OpenJournal);
		AddNewBinding(GameManager.Instance.Input.Controls.OpenMap, SteamVR_Actions._default.OpenMap);
		AddNewBinding(GameManager.Instance.Input.Controls.OpenEncyclopedia, SteamVR_Actions._default.OpenEncyclopedia);
		AddNewBinding(GameManager.Instance.Input.Controls.OpenMessages, SteamVR_Actions._default.OpenMessages);

		AddNewBinding(GameManager.Instance.Input.Controls.TabLeft, SteamVR_Actions._default.TabLeft);
		AddNewBinding(GameManager.Instance.Input.Controls.TabRight, SteamVR_Actions._default.TabRight);

		// These bindings are only really used by the tutorial manager to check if the player is moving
		AddNewBinding(GameManager.Instance.Input.controls.MoveForward, SteamVR_Actions._default.Move, Vector2.up);
		AddNewBinding(GameManager.Instance.Input.controls.MoveBack, SteamVR_Actions._default.Move, Vector2.down);
		AddNewBinding(GameManager.Instance.Input.controls.MoveLeft, SteamVR_Actions._default.Move, Vector2.left);
		AddNewBinding(GameManager.Instance.Input.controls.MoveRight, SteamVR_Actions._default.Move, Vector2.right);

		new CustomControl(SteamVR_Actions._default.RecenterCamera, VRCameraManager.Instance.RecenterCamera);
	}

	private static void AddNewBinding(PlayerAction action, SteamVR_Action_Vector2 vrAction, Vector2 direction)
	{
		var vrBindingSource = new VRVector2BindingSource(vrAction, direction);

		action.AddDefaultBinding(vrBindingSource);
		action.AddBinding(vrBindingSource);

		DredgeVRLogger.Debug($"Added new binding for {action.Name} - {vrAction.GetShortName()}");
	}

	private static void AddNewBinding(PlayerAction action, SteamVR_Action_Boolean vrAction)
	{
		var vrBindingSource = new VRBindingSource(vrAction);
		PlayerActionBindings[action] = vrBindingSource;
		action.AddDefaultBinding(vrBindingSource);
		action.AddBinding(vrBindingSource);

		DredgeVRLogger.Debug($"Added new binding for {action.Name} - {vrAction.GetShortName()}");
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
