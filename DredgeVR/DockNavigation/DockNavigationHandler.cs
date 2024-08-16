using DredgeVR.Helpers;
using DredgeVR.VRCamera;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DredgeVR.DockNavigation;

public class DockNavigationHandler : MonoBehaviour
{
	private DockUI _dockUI;
	private List<GameObject> _customButtons = new();

	public enum DockPosition
	{
		IronRigDock,
		IronRigDeck,
		Unknown
	}

	public DockPosition CurrentDockPosition { get; private set; } = DockPosition.Unknown;

	public static DockNavigationHandler Instance { get; private set; }

	public void Awake()
	{
		Instance = this;

		_dockUI = GameObject.FindObjectOfType<DockUI>();

		GameEvents.Instance.OnPlayerDockedToggled += OnPlayerDockedToggled;
		GameEvents.Instance.OnDialogueStarted += RefreshUI;
		GameEvents.Instance.OnDialogueCompleted += RefreshUI;
		ApplicationEvents.Instance.OnUIWindowToggled += OnUIWindowToggled;
		OnPlayerDockedToggled(GameManager.Instance.Player.CurrentDock);
	}

	public void OnDestroy()
	{
		GameEvents.Instance.OnPlayerDockedToggled -= OnPlayerDockedToggled;
		GameEvents.Instance.OnDialogueStarted -= RefreshUI;
		GameEvents.Instance.OnDialogueCompleted -= RefreshUI;
		ApplicationEvents.Instance.OnUIWindowToggled -= OnUIWindowToggled;
	}


	private void OnUIWindowToggled(UIWindowType windowType, bool show)
	{
		if (windowType == UIWindowType.DOCK && show)
		{
			Delay.FireOnNextUpdate(RefreshUI);
		}
	}

	private void RefreshUI()
	{
		switch (CurrentDockPosition)
		{
			case DockPosition.IronRigDock:
				ShowIronRigDockUI();
				break;
			case DockPosition.IronRigDeck:
				ShowIronRigDeckUI();
				break;
			default:
				break;
		}

		if (GameManager.Instance.DialogueRunner.Dialogue?.IsActive ?? false)
		{
			foreach (var button in _customButtons) button.SetActive(false);
		}
	}

	public void RefreshPosition()
	{
		switch (CurrentDockPosition)
		{
			case DockPosition.IronRigDock:
				GoToIronRigDock();
				break;
			case DockPosition.IronRigDeck:
				GoToIronRigDeck();
				break;
			default:
				break;
		}

		RefreshUI();
	}

	private GameObject _tirElevatorButton, _tirDockButton;

	private void OnPlayerDockedToggled(Dock dock)
	{
		try
		{
			if (dock == null)
			{
				// Reset anchor transform
				VRCameraManager.Instance.ResetAnchorToBoat();
				CurrentDockPosition = DockPosition.Unknown;
				foreach (var button in _customButtons)
				{
					GameObject.Destroy(button);
				}
				_customButtons.Clear();
			}
			if (dock.name.Contains("Iron Rig"))
			{
				_tirElevatorButton = MakeButton("Go up elevator", GoToIronRigDeck, new Vector2(0, 0));
				_tirDockButton = MakeButton("Return to dock", GoToIronRigDock, new Vector2(0, 0));

				// Standing on the dock
				GoToIronRigDock();
			}
		}
		catch (Exception e)
		{
			DredgeVRLogger.Error($"Something went wrong when docking/undocking - {e}");
		}
	}

	private void GoToIronRigDock()
	{
		CurrentDockPosition = DockPosition.IronRigDock;
		VRCameraManager.MoveCameraTo(new Vector3(-13.5945f, 1.1673f, 690.1805f), Quaternion.identity);
		ShowIronRigDockUI();
	}

	private void ShowIronRigDockUI()
	{
		// Hide upper deck buttons, only show elevator and fleet services
		foreach (Transform button in _dockUI.destinationButtonContainer.transform)
		{
			if (button.name.Contains("BoatActionsDestinationUI")) continue;
			button.gameObject.SetActive(false);
		}

		var cargoElevatorButton = _dockUI.destinationButtonContainer?.Find("DestinationButton: RigBase");

		_tirElevatorButton?.SetActive(cargoElevatorButton == null);
		_dockUI.destinationButtonContainer?.Find("DestinationButton: FleetServices")?.gameObject?.SetActive(true);
		_dockUI.destinationButtonContainer?.Find("DestinationButton: StorageOnLowerPlatform")?.gameObject?.SetActive(true);
		_dockUI.destinationButtonContainer?.Find("DestinationButton: RigBase")?.gameObject?.SetActive(true);
	}

	private void GoToIronRigDeck()
	{
		CurrentDockPosition = DockPosition.IronRigDeck;
		VRCameraManager.MoveCameraTo(new Vector3(-5.0297f, 15.2771f, 684.2171f), Quaternion.identity);
		ShowIronRigDeckUI();
	}

	private void ShowIronRigDeckUI()
	{
		// Show upper deck buttons, only hide elevator and fleet services
		foreach (Transform button in _dockUI.destinationButtonContainer.transform)
		{
			button.gameObject.SetActive(true);
		}

		_tirElevatorButton?.SetActive(false);
		_dockUI.destinationButtonContainer?.Find("DestinationButton: FleetServices")?.gameObject?.SetActive(false);
		_dockUI.destinationButtonContainer?.Find("DestinationButton: StorageOnLowerPlatform")?.gameObject?.SetActive(false);
		_dockUI.destinationButtonContainer?.Find("DestinationButton: RigBase")?.gameObject?.SetActive(false);
	}

	private GameObject MakeButton(string text, Action action, Vector2 position)
	{
		try
		{
			var gameObject = UnityEngine.Object.Instantiate<GameObject>(_dockUI.destinationButtonPrefab, _dockUI.destinationButtonContainer);
			Component.Destroy(gameObject.GetComponent<DestinationButton>());
			gameObject.name = text;
			gameObject.transform.localPosition = new Vector3(position.x, position.y, 0);
			gameObject.transform.Find("AttentionCallout").gameObject.SetActive(false);
			var button = gameObject.transform.Find("ButtonWithIcon");
			button.transform.Find("Icon").gameObject.SetActive(false);
			button.GetComponent<BasicButtonWrapper>().OnClick = action;
			button.GetComponentInChildren<TextMeshProUGUI>().text = text;

			_customButtons.Add(gameObject);
			gameObject.SetActive(false);

			return gameObject;
		}
		catch (Exception e)
		{
			DredgeVRLogger.Error($"Couldn't make button - {e}");
			throw;
		}
	}

}
