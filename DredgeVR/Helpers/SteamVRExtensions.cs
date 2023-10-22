using Valve.VR;

namespace DredgeVR.Helpers;

public static class SteamVRExtensions
{
	public static SteamVR_Input_Sources GetHand(this SteamVR_Action_Boolean action)
	{
		var left = action.GetLocalizedOriginPart(SteamVR_Input_Sources.Any, EVRInputStringBits.VRInputString_Hand).Contains("Left");
		return left ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;
	}
}
