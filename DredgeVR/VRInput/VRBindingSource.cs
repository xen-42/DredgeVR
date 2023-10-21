using InControl;
using System;
using System.IO;
using Valve.VR;

namespace DredgeVR.VRInput;

public class VRBindingSource : BindingSource
{
	public VRInputManager.VRBinding binding;

	public VRBindingSource(VRInputManager.VRBinding binding)
	{
		this.binding = binding;
	}

	public override string Name => $"{binding.action.GetShortName()} ({Enum.GetName(typeof(SteamVR_Input_Sources), binding.hand)})";

	public override string DeviceName => "VR";

	public override InputDeviceClass DeviceClass => InputDeviceClass.Controller;

	public override InputDeviceStyle DeviceStyle => InputDeviceStyle.Oculus;

	public static BindingSourceType Source = BindingSourceType.UnknownDeviceBindingSource;
	public override BindingSourceType BindingSourceType => Source;

	public override bool Equals(BindingSource other)
	{
		return other is VRBindingSource otherVR && otherVR.binding.Equals(binding);
	}

	public override bool GetState(InputDevice inputDevice)
	{
		return VRInputManager.State[binding];
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
