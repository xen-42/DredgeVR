using Valve.VR;

namespace DredgeVR.Helpers;

public static class SteamVRExtensions
{
	public static string GetDeviceName(this SteamVR_Action_Boolean action)
	{
		return SteamVR.instance.GetStringProperty(ETrackedDeviceProperty.Prop_ControllerType_String, action.trackedDeviceIndex);
	}

	public static string GetButtonName(this SteamVR_Action_Boolean action)
	{
		return action.GetRenderModelComponentName(action.activeDevice);
	}

	public static string GetHandName(this SteamVR_Action_Boolean action)
	{
		return action.activeDevice == SteamVR_Input_Sources.LeftHand ? "left" : "right";
	}
}
