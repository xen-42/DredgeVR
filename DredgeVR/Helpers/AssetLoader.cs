using System.IO;
using UnityEngine;
using Winch.Core;

namespace DredgeVR.Helpers;

public class AssetLoader
{
	public static GameObject LeftHandBase;
	public static GameObject RightHandBase;

	public AssetLoader()
	{
		var bundle = AssetBundle.LoadFromFile(Path.Combine(DredgeVRCore.ModPath, "AssetBundles/dredgevr"));
		LeftHandBase = LoadAsset<GameObject>(bundle, "SteamVR/Prefabs/vr_glove_left.prefab");
		RightHandBase = LoadAsset<GameObject>(bundle, "SteamVR/Prefabs/vr_glove_right.prefab");
	}

	private T LoadAsset<T>(AssetBundle bundle, string prefabName) where T : UnityEngine.Object
	{
		var asset = bundle.LoadAsset<T>($"Assets/{prefabName}");
		if (asset)
		{
			return asset;
		}
		else
		{
			WinchCore.Log.Error($"Failed to load asset {prefabName}");
			return null;
		}
	}
}
