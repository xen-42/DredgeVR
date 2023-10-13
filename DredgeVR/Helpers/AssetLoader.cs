using AeLa.EasyFeedback.APIs;
using System.IO;
using UnityEngine;
using Winch.Core;

namespace DredgeVR.Helpers;

public class AssetLoader
{
	private static readonly Shader standardShader = Shader.Find("Standard");

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
			// I live in hell
			// Repair materials
			if (asset is GameObject go)
			{
				foreach (var renderer in go.GetComponentsInChildren<MeshRenderer>())
				{
					foreach (var mat in renderer.materials)
					{
						mat.shader = standardShader;
						mat.renderQueue = 2000;
					}
				}
				foreach (Transform child in go.transform)
				{
					foreach (var renderer in child.GetComponentsInChildren<MeshRenderer>())
					{
						foreach (var mat in renderer.materials)
						{
							mat.shader = standardShader;
							mat.renderQueue = 2000;
						}
					}
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
