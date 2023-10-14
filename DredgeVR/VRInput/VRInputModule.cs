using DredgeVR.VRCamera;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;
using Winch.Core;

namespace DredgeVR.VRInput;

/// <summary>
/// Based on Youtube tutorial https://www.youtube.com/watch?v=vNqHRD4sqPc&ab_channel=Andrew
/// </summary>
internal class VRInputModule : BaseInputModule
{											    
	public static VRInputModule Instance { get; private set; }

	// For raycasting
	public Camera RaycastCamera;
	public SteamVR_Input_Sources DominantHand { get; private set; }
	public Action<SteamVR_Input_Sources> DominantHandChanged;

	public SteamVR_Action_Boolean UIClickAction;

	private GameObject _currentObject;
	public PointerEventData Data { get; private set; }

	protected override void Awake()
	{
		base.Awake();

		Instance = this;
	}

	protected override void Start()
	{
		base.Start();

		SetDominantHand(false);

		Data = new PointerEventData(eventSystem);
	}		 
	
	public void SetDominantHand(bool left)
	{
		RaycastCamera = left ? VRCameraManager.LeftHand.RaycastCamera : VRCameraManager.RightHand.RaycastCamera;
		DominantHand = left ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;
		UIClickAction = left ? SteamVR_Actions._default.LeftTrigger : SteamVR_Actions._default.RightTrigger;

		DominantHandChanged?.Invoke(DominantHand);
	}

	public override void Process()
	{
		// Screen raycast using camera
		Data.Reset();
		Data.position = new Vector2(RaycastCamera.pixelWidth / 2, RaycastCamera.pixelHeight / 2);

		eventSystem.RaycastAll(Data, m_RaycastResultCache);
		Data.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
		_currentObject = Data.pointerCurrentRaycast.gameObject;

		m_RaycastResultCache.Clear();

		HandlePointerExitAndEnter(Data, _currentObject);

		if (UIClickAction.GetStateDown(DominantHand))
		{
			Data.pointerPressRaycast = Data.pointerCurrentRaycast;
			var newPointerPress = ExecuteEvents.ExecuteHierarchy(_currentObject, Data, ExecuteEvents.pointerDownHandler) 
				?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(_currentObject);
			Data.pressPosition = Data.position;
			Data.pointerPress = newPointerPress;
			Data.rawPointerPress = _currentObject;

			WinchCore.Log.Info("Clicked");
		}

		if (UIClickAction.GetStateUp(DominantHand))
		{
			ExecuteEvents.Execute(Data.pointerPress, Data, ExecuteEvents.pointerUpHandler);

			var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(_currentObject);
			if (Data.pointerPress == pointerUpHandler)
			{
				ExecuteEvents.Execute(Data.pointerPress, Data, ExecuteEvents.pointerClickHandler);
			}

			eventSystem.SetSelectedGameObject(null);

			Data.pressPosition = Vector2.zero;
			Data.pointerPress = null;
			Data.rawPointerPress = null;

			WinchCore.Log.Info("Released");
		}
	}
}
