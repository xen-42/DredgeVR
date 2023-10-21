using DredgeVR.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace DredgeVR.VRUI;

public static class VRControlIcons
{
	private static readonly Dictionary<string, ControlIconData> _cache = new();

	public static ControlIconData GetControlIconData(SteamVR_Action action)
	{
		if (action == SteamVR_Actions._default.A_Right || action == SteamVR_Actions._default.A_Left)
		{
			return CreateData("AssetBundles/VR Icons/Oculus/Oculus_A.png");
		}
		if (action == SteamVR_Actions._default.B_Right || action == SteamVR_Actions._default.B_Left)
		{
			return CreateData("AssetBundles/VR Icons/Oculus/Oculus_B.png");
		}
		if (action == SteamVR_Actions._default.LeftTrigger)
		{
			return CreateData("AssetBundles/VR Icons/Oculus/Oculus_LT.png");
		}
		if (action == SteamVR_Actions._default.RightTrigger)
		{
			return CreateData("AssetBundles/VR Icons/Oculus/Oculus_RT.png");
		}
		if (action == SteamVR_Actions._default.LeftThumbStickPress)
		{
			return CreateData("AssetBundles/VR Icons/Oculus/Oculus_Left_Stick.png");
		}
		if (action == SteamVR_Actions._default.RightThumbStickPress)
		{
			return CreateData("AssetBundles/VR Icons/Oculus/Oculus_Right_Stick.png");
		}
		return null;
	}

	private static ControlIconData CreateData(string path)
	{
		if (!_cache.ContainsKey(path))
		{
			var texture = AssetLoader.GetTexture(path);
			var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width, texture.height) / 2f);

			return new ControlIconData()
			{
				upSprite = sprite,
				downSprite = sprite,
				upSpriteName = texture.name,
				downSpriteName = texture.name,
			};
		}
		return _cache[path];
	}
}
