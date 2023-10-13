using DredgeVR.VRCamera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;
using Winch.Config;
using Winch.Core;

namespace DredgeVR.VRInput;

internal class VRInputModule : BaseInputModule
{											    
	public static VRInputModule Instance { get; private set; }

	// For raycasting
	public Camera RaycastCamera;
	public SteamVR_Input_Sources TargetSource;
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

		RaycastCamera = VRCameraManager.LeftHand.RaycastCamera;
		TargetSource = SteamVR_Input_Sources.LeftHand;
		UIClickAction = SteamVR_Actions._default.LeftTrigger;

		Data = new PointerEventData(eventSystem);
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

		// Thank you Youtube tutorial https://www.youtube.com/watch?v=vNqHRD4sqPc&ab_channel=Andrew
		if (UIClickAction.GetStateDown(TargetSource))
		{
			Data.pointerPressRaycast = Data.pointerCurrentRaycast;
			var newPointerPress = ExecuteEvents.ExecuteHierarchy(_currentObject, Data, ExecuteEvents.pointerDownHandler) 
				?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(_currentObject);
			Data.pressPosition = Data.position;
			Data.pointerPress = newPointerPress;
			Data.rawPointerPress = _currentObject;

			WinchCore.Log.Info("Clicked");
		}

		if (UIClickAction.GetStateUp(TargetSource))
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
