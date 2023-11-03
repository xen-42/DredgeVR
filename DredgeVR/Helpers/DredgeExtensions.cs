using DredgeVR.VRInput;
using InControl;

namespace DredgeVR.Helpers;

public static class DredgeExtensions
{
	/// <summary>
	/// Returns the hand the VR binding is on, else returns the default hand
	/// </summary>
	/// <param name="action"></param>
	/// <returns></returns>
	public static string GetHand(this PlayerAction action, bool defaultToDominant = false)
	{
		if (action != null && VRInputManager.PlayerActionBindings.TryGetValue(action, out var vrBinding))
		{
			var handName = vrBinding.action.GetHandName();
			if (!string.IsNullOrEmpty(handName))
			{
				return vrBinding.action.GetHandName();
			}
		}

		var dominantIsLeft = VRInputModule.Instance.DominantHand.hand == Valve.VR.SteamVR_Input_Sources.LeftHand;
		return dominantIsLeft == defaultToDominant ? "left" : "right";
	}
}
