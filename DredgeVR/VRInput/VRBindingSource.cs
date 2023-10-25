using InControl;
using System.IO;
using Valve.VR;

namespace DredgeVR.VRInput;

public class VRBindingSource : BindingSource
{
	public SteamVR_Action_Boolean action;

	public string GetButtonName()
	{
		// Keep these localized because they will appear in text strings
		var button = action.GetLocalizedOriginPart(SteamVR_Input_Sources.Any, EVRInputStringBits.VRInputString_InputSource);
		var hand = action.GetLocalizedOriginPart(SteamVR_Input_Sources.Any, EVRInputStringBits.VRInputString_Hand);

		return $"{button} ({hand})";
	}

	public VRBindingSource(SteamVR_Action_Boolean action)
	{
		this.action = action;
	}

	public override string Name => $"{action.GetShortName()} - {action.GetLocalizedOrigin(SteamVR_Input_Sources.Any)}";

	public override string DeviceName => "VR";

	public override InputDeviceClass DeviceClass => InputDeviceClass.Controller;

	public override InputDeviceStyle DeviceStyle => InputDeviceStyle.Oculus;

	public static BindingSourceType Source = BindingSourceType.UnknownDeviceBindingSource;
	public override BindingSourceType BindingSourceType => Source;

	public override bool Equals(BindingSource other)
	{
		return other is VRBindingSource otherVR && otherVR.action.Equals(action);
	}

	public override bool GetState(InputDevice inputDevice)
	{
		return action.state;
	}

	public override float GetValue(InputDevice inputDevice)
	{
		return GetState(inputDevice) ? 1f : 0f;
	}

	public override void Load(BinaryReader reader, ushort dataFormatVersion)
	{

	}

	public override void Save(BinaryWriter writer)
	{

	}
}
