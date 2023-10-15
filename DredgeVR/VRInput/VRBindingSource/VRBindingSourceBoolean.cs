using InControl;
using System;
using Valve.VR;

namespace DredgeVR.VRInput.VRBindingSource;

public class VRBindingSourceBoolean : VRBindingSourceBase
{
	public SteamVR_Action_Boolean SteamVRAction { get; private set; }
	public SteamVR_Input_Sources InputSource { get; private set; }

	public override string Name => $"{SteamVRAction.GetShortName()} ({InputSource})";

	public VRBindingSourceBoolean(SteamVR_Action_Boolean steamVRAction, SteamVR_Input_Sources inputSource)
	{
		SteamVRAction = steamVRAction;
		InputSource = inputSource;
	}

	public override bool Equals(BindingSource other)
	{
		var vrBindingSource = other as VRBindingSourceBoolean;
		if (vrBindingSource == null)
		{
			return false;
		}
		else
		{
			return vrBindingSource.SteamVRAction == SteamVRAction && vrBindingSource.InputSource == InputSource;
		}
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(SteamVRAction.GetHashCode(), InputSource.GetHashCode());
	}

	public override bool GetState(InputDevice inputDevice)
	{
		return SteamVRAction.GetState(InputSource);
	}

	public override float GetValue(InputDevice inputDevice)
	{
		return GetState(inputDevice) ? 1f : 0f;
	}
}
