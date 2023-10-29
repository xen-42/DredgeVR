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
		var name = action.GetRenderModelComponentName(action.activeDevice);

		// Need to get direction? I.e., for using trackpad up/down/left/right as buttons
		// No idea how to do that

		return name;
	}

	public static string GetHandName(this SteamVR_Action_Boolean action)
	{
		return action.activeDevice switch
		{
			SteamVR_Input_Sources.LeftHand => "left",
			SteamVR_Input_Sources.RightHand => "right",
			_ => null,
		};
	}
}
