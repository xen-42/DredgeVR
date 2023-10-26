using System;
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
	public static GameObject LeftHandBase { get; private set; }
	public static GameObject RightHandBase { get; private set; }

	public static Shader LitShader
	{
		get
		{
			_litShader ??= Shader.Find("Shader Graphs/Lit_Shader");
			return _litShader;
		}
	}
	private static Shader _litShader;

	public static Mesh PrimitiveQuad { get; private set; }
	public static Mesh DoubleSidedQuad { get; private set; }
	public static Mesh PrimitiveCylinder { get; private set; }

	private static readonly Dictionary<string, Texture2D> _icons = new();

	public AssetLoader()
	{
		var bundle = AssetBundle.LoadFromFile(Path.Combine(DredgeVRCore.ModPath, "AssetBundles/dredgevr"));
		LeftHandBase = LoadAsset<GameObject>(bundle, "SteamVR/Prefabs/vr_glove_left.prefab");
		RightHandBase = LoadAsset<GameObject>(bundle, "SteamVR/Prefabs/vr_glove_right.prefab");

		var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
		PrimitiveQuad = quad.GetComponent<MeshFilter>().mesh;
		GameObject.Destroy(quad);

		DoubleSidedQuad = GeometryHelper.MakeMeshDoubleFaced(PrimitiveQuad);

		var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		PrimitiveCylinder = cylinder.GetComponent<MeshFilter>().mesh;
		GameObject.Destroy(cylinder);
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
			DredgeVRLogger.Error($"Failed to load asset {prefabName}");
			return null;
		}
	}

	public static Texture2D GetTexture(string path)
	{
		try
		{
			if (!_icons.ContainsKey(path))
			{
				var rawData = File.ReadAllBytes(Path.Combine(DredgeVRCore.ModPath, path));
				var tex = new Texture2D(2, 2);
				tex.LoadImage(rawData);
				tex.name = Path.GetFileName(path);
				_icons[path] = tex;
			}
		}
		catch (Exception e)
		{
			DredgeVRLogger.Error($"Could not load texture at {path} : {e}");
			_icons[path] = null;
		}
		return _icons[path];

	}
}
