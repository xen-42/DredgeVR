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
			if (asset is GameObject go)
			{
				foreach (var meshRenderer in go.GetComponentsInChildren<SkinnedMeshRenderer>())
				{
					meshRenderer.material.shader = LitShader;
				}
			}

			return asset;
		}
		else
		{
			WinchCore.Log.Error($"Failed to load asset {prefabName}");
			return null;
		}
	}
}
