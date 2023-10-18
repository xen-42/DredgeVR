using InControl;
using System;
using System.IO;
using Valve.VR;

namespace DredgeVR.VRInput;

public class VRBindingSource : BindingSource
{
	private VRInputManager.VRBinding _binding;

	public VRBindingSource(VRInputManager.VRBinding binding)
	{
		_binding = binding;
	}

	public override string Name => $"{_binding.action.GetShortName()} ({Enum.GetName(typeof(SteamVR_Input_Sources), _binding.hand)})";

	public override string DeviceName => "VR";

	public override InputDeviceClass DeviceClass => InputDeviceClass.Controller;

	public override InputDeviceStyle DeviceStyle => InputDeviceStyle.Oculus;

	public override BindingSourceType BindingSourceType => BindingSourceType.UnknownDeviceBindingSource;

	public override bool Equals(BindingSource other)
	{
		return other is VRBindingSource otherVR && otherVR._binding.Equals(_binding);
	}

	public override bool GetState(InputDevice inputDevice)
	{
		return VRInputManager.State[_binding];
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
