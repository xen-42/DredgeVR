using InControl;
using System.IO;

namespace DredgeVR.VRInput;

public abstract class VRBindingSourceBase : BindingSource
{
	private PlayerAction GetBoundTo => Helpers.TypeExtensions.GetValue<PlayerAction>(this, "BoundTo");
	private InputDevice GetDevice => Helpers.TypeExtensions.GetValue<InputDevice>(GetBoundTo, "Device");

	public override string DeviceName
	{
		get
		{
			if (GetBoundTo == null)
			{
				return "";
			}

			InputDevice device = GetDevice;
			if (device == InputDevice.Null)
			{
				return "VR";
			}

			return device.Name;
		}
	}

	public override InputDeviceClass DeviceClass
	{
		get
		{
			if (GetBoundTo != null)
			{
				return GetDevice.DeviceClass;
			}

			return InputDeviceClass.Unknown;
		}
	}

	public override InputDeviceStyle DeviceStyle
	{
		get
		{
			if (GetBoundTo != null)
			{
				return GetDevice.DeviceStyle;
			}

			return InputDeviceStyle.Unknown;
		}
	}

	public override BindingSourceType BindingSourceType => BindingSourceType.DeviceBindingSource;

	// TODO: Should implement these I bet
	public override void Load(BinaryReader reader, ushort dataFormatVersion) { }

	public override void Save(BinaryWriter writer) { }
}
