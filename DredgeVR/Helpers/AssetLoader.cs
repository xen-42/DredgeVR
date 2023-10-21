using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Valve.VR;
using Winch.Core;

namespace DredgeVR.Helpers;

/// <summary>
/// Loads assets from external bundles and contains their prefabs
/// </summary>
public class AssetLoader
{
	public static GameObject LeftHandBase;
	public static GameObject RightHandBase;

	public static Shader LitShader
	{
		get
		{
			_litShader ??= Shader.Find("Shader Graphs/Lit_Shader");
			return _litShader;
		}
	}
	private static Shader _litShader;

	private static readonly Dictionary<string, Texture2D> _icons = new();

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

	public static Texture2D GetTexture(string path)
	{
		if (!_icons.ContainsKey(path))
		{
			var rawData = File.ReadAllBytes(Path.Combine(DredgeVRCore.ModPath, path));
			var tex = new Texture2D(2, 2);
			tex.LoadImage(rawData);
			tex.name = Path.GetFileName(path);
			_icons[path] = tex;
		}
		return _icons[path];

	}
}
