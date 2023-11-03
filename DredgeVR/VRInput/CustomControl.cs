using System;
using Valve.VR;

namespace DredgeVR.VRInput;

public class CustomControl
{
	private SteamVR_Action_Boolean _vrAction;
	private Action _action;

	public CustomControl(SteamVR_Action_Boolean vrAction, Action action)
	{
		_vrAction = vrAction;
		_action = action;

		_vrAction.onStateDown += InvokeAction;
	}

	~CustomControl()
	{
		_vrAction.onStateDown -= InvokeAction;
	}

	private void InvokeAction(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
	{
		_action?.Invoke();
	}
}
