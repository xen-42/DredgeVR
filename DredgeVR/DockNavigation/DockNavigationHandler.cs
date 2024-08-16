using DredgeVR.Helpers;
using DredgeVR.VRCamera;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static DredgeVR.DockNavigation.DockNavigationHandler;

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

    private DockPosition _dockPosition = DockPosition.Unknown;

    public void Awake()
    {
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
        switch (_dockPosition)
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

    private GameObject _tirElevatorButton, _tirDockButton;

    private void OnPlayerDockedToggled(Dock dock)
    {
        try
        {
            if (dock == null)
            {
                // Reset anchor transform
                VRCameraManager.Instance.ResetAnchorToBoat();
                _dockPosition = DockPosition.Unknown;
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
        _dockPosition = DockPosition.IronRigDock;
        VRCameraManager.MoveCameraTo(new Vector3(-13.5945f, 1.1673f, 690.1805f), Quaternion.identity);
        ShowIronRigDockUI();
    }

    private void ShowIronRigDockUI()
    {
        // Hide upper deck buttons, only show elevator and fleet services
        foreach (Transform button in _dockUI.destinationButtonContainer.transform)
        {
            button.gameObject.SetActive(false);
        }

        _tirElevatorButton?.SetActive(true);
        _dockUI.destinationButtonContainer?.Find("DestinationButton: FleetServices")?.gameObject?.SetActive(true);
    }

    private void GoToIronRigDeck()
    {
        _dockPosition = DockPosition.IronRigDeck;
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

            return gameObject;
        }
        catch (Exception e)
        {
            DredgeVRLogger.Error($"Couldn't make button - {e}");
            throw;
        }
    }

}
