using DredgeVR.Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using Valve.VR;

namespace DredgeVR.VRUI;

public static class VRControlIcons
{
	private static readonly Dictionary<string, ControlIconData> _cache = new();

	public static ControlIconData GetControlIconData(SteamVR_Action_Boolean action)
	{
		if (LocalizationSettings.AvailableLocales.GetLocale("en") == null)
		{
			return null;
		}

		var controllerType = action.GetDeviceName();
		var inputSource = action.GetButtonName();
		var hand = action.GetHandName();

		ControlIconData data;

		if (controllerType.Contains("knuckles"))
		{
			data = GetIndexControlIconData(inputSource, hand);
		}
		else if (controllerType.Contains("vive"))
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
		if (inputSource.Contains("menu"))
		{
			return CreateData("AssetBundles/VR Icons/Vive/Vive_Menu.png");
		}
		if (inputSource.Contains("trigger"))
		{
			if (hand.Contains("left"))
			{
				return CreateData("AssetBundles/VR Icons/Vive/Vive_LT.png");
			}
			if (hand.Contains("right"))
			{
				return CreateData("AssetBundles/VR Icons/Vive/Vive_RT.png");
			}
		}
		if (inputSource.Contains("grip"))
		{
			if (hand.Contains("left"))
			{
				return CreateData("AssetBundles/VR Icons/Vive/Vive_Grip_Left.png");
			}
			if (hand.Contains("right"))
			{
				return CreateData("AssetBundles/VR Icons/Vive/Vive_Grip_Right.png");
			}
		}
		return null;
	}

	private static ControlIconData GetOculusControlIconData(string inputSource, string hand)
	{
		if (inputSource.Contains("button_a"))
		{
			return CreateData("AssetBundles/VR Icons/Oculus/Oculus_A.png");
		}
		if (inputSource.Contains("button_b"))
		{
			return CreateData("AssetBundles/VR Icons/Oculus/Oculus_B.png");
		}
		if (inputSource.Contains("button_x"))
		{
			return CreateData("AssetBundles/VR Icons/Oculus/Oculus_X.png");
		}
		if (inputSource.Contains("button_y"))
		{
			return CreateData("AssetBundles/VR Icons/Oculus/Oculus_Y.png");
		}
		if (inputSource.Contains("trigger"))
		{
			if (hand.Contains("left"))
			{
				return CreateData("AssetBundles/VR Icons/Oculus/Oculus_LT.png");
			}
			if (hand.Contains("right"))
			{
				return CreateData("AssetBundles/VR Icons/Oculus/Oculus_RT.png");
			}
		}
		if (inputSource.Contains("thumbstick"))
		{
			if (hand.Contains("left"))
			{
				return CreateData("AssetBundles/VR Icons/Oculus/Oculus_Left_Stick.png");
			}
			if (hand.Contains("right"))
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
