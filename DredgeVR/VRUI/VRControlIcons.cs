using DredgeVR.Helpers;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace DredgeVR.VRUI;

public static class VRControlIcons
{
	private static readonly Dictionary<string, ControlIconData> _cache = new();

	public static ControlIconData GetControlIconData(SteamVR_Action_Boolean action)
	{
		var controllerType = action.GetLocalizedOriginPart(SteamVR_Input_Sources.Any, EVRInputStringBits.VRInputString_ControllerType);
		var inputSource = action.GetLocalizedOriginPart(SteamVR_Input_Sources.Any, EVRInputStringBits.VRInputString_InputSource);
		var hand = action.GetLocalizedOriginPart(SteamVR_Input_Sources.Any, EVRInputStringBits.VRInputString_Hand);

		ControlIconData data;

		if (controllerType.Contains("Index"))
		{
			data = GetIndexControlIconData(inputSource, hand);
		}
		else if (controllerType.Contains("Vive"))
		{
			data = GetViveControlIconData(inputSource, hand);
		}
		// Default to the Oculus control icons since they seem the most general
		else
		{
			data = GetOculusControlIconData(inputSource, hand);
		}

		if (data == null)
		{
			DredgeVRLogger.Error($"Could not find control icon data for [{controllerType}] [{inputSource}] [{hand}]");
		}

		return data;
	}

	private static ControlIconData GetIndexControlIconData(string inputSource, string hand)
	{
		// TODO: Get Valve Index specific control icons

		return GetOculusControlIconData(inputSource, hand);
	}

	private static ControlIconData GetViveControlIconData(string inputSource, string hand)
	{
		if (inputSource.Contains("Menu"))
		{
			return CreateData("AssetBundles/VR Icons/Vive/Vive_Menu.png");
		}
		if (inputSource.Contains("Trigger"))
		{
			if (hand.Contains("Left"))
			{
				return CreateData("AssetBundles/VR Icons/Vive/Vive_LT.png");
			}
			if (hand.Contains("Right"))
			{
				return CreateData("AssetBundles/VR Icons/Vive/Vive_RT.png");
			}
		}
		if (inputSource.Contains("Grip"))
		{
			if (hand.Contains("Left"))
			{
				return CreateData("AssetBundles/VR Icons/Vive/Vive_Grip_Left.png");
			}
			if (hand.Contains("Right"))
			{
				return CreateData("AssetBundles/VR Icons/Vive/Vive_Grip_Right.png");
			}
		}
		return null;
	}

	private static ControlIconData GetOculusControlIconData(string inputSource, string hand)
	{
		if (inputSource.Contains("A Button"))
		{
			return CreateData("AssetBundles/VR Icons/Oculus/Oculus_A.png");
		}
		if (inputSource.Contains("B Button"))
		{
			return CreateData("AssetBundles/VR Icons/Oculus/Oculus_B.png");
		}
		if (inputSource.Contains("X Button"))
		{
			return CreateData("AssetBundles/VR Icons/Oculus/Oculus_X.png");
		}
		if (inputSource.Contains("Y Button"))
		{
			return CreateData("AssetBundles/VR Icons/Oculus/Oculus_Y.png");
		}
		if (inputSource.Contains("Trigger"))
		{
			if (hand.Contains("Left"))
			{
				return CreateData("AssetBundles/VR Icons/Oculus/Oculus_LT.png");
			}
			if (hand.Contains("Right"))
			{
				return CreateData("AssetBundles/VR Icons/Oculus/Oculus_RT.png");
			}
		}
		if (inputSource.Contains("Thumb Stick"))
		{
			if (hand.Contains("Left"))
			{
				return CreateData("AssetBundles/VR Icons/Oculus/Oculus_Left_Stick.png");
			}
			if (hand.Contains("Right"))
			{
				return CreateData("AssetBundles/VR Icons/Oculus/Oculus_Right_Stick.png");
			}
		}
		return null;
	}

	private static ControlIconData CreateData(string path)
	{
		if (!_cache.ContainsKey(path))
		{
			var texture = AssetLoader.GetTexture(path);
			if (texture != null)
			{
				var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width, texture.height) / 2f);

				_cache[path] = new ControlIconData()
				{
					upSprite = sprite,
					downSprite = sprite,
					upSpriteName = texture.name,
					downSpriteName = texture.name,
				};
			}
			else
			{
				_cache[path] = null;
			}
		}
		return _cache[path];
	}
}
