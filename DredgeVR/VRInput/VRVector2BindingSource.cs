using InControl;
using System.IO;
using UnityEngine;
using Valve.VR;

namespace DredgeVR.VRInput;

public class VRVector2BindingSource : BindingSource
{
	public SteamVR_Action_Vector2 action;
	private Vector2 _direction;

	public string GetButtonName()
	{
		// Keep these localized because they will appear in text strings
		var button = action.GetLocalizedOriginPart(SteamVR_Input_Sources.Any, EVRInputStringBits.VRInputString_InputSource);
		var hand = action.GetLocalizedOriginPart(SteamVR_Input_Sources.Any, EVRInputStringBits.VRInputString_Hand);

		return $"{button} ({hand})";
	}

	public VRVector2BindingSource(SteamVR_Action_Vector2 action, Vector2 direction)
	{
		this.action = action;
		this._direction = direction;
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
		// Dot product greater than zero means that some part of this movement is in the direction specified
		return Vector2.Dot(action.axis, _direction) > 0;
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
