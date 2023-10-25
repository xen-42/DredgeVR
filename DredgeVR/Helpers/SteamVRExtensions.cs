using Valve.VR;

namespace DredgeVR.Helpers;

public static class SteamVRExtensions
{
	// TODO: Don't depend on locale

	public static string GetDeviceName(this SteamVR_Action_Boolean action)
	{
		return action.GetLocalizedOriginPart(SteamVR_Input_Sources.Any, EVRInputStringBits.VRInputString_ControllerType);
	}

	public static string GetButtonName(this SteamVR_Action_Boolean action)
	{
		return action.GetLocalizedOriginPart(SteamVR_Input_Sources.Any, EVRInputStringBits.VRInputString_InputSource);
	}

	public static string GetHandName(this SteamVR_Action_Boolean action)
	{
		return action.GetLocalizedOriginPart(SteamVR_Input_Sources.Any, EVRInputStringBits.VRInputString_Hand);
	}
}
