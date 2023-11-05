using FluffyUnderware.DevTools.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DredgeVR.Helpers;

/// <summary>
/// Loads assets from external bundles and contains their prefabs
/// </summary>
public class AssetLoader
{
	// Objects
	public static GameObject LeftHandBase { get; private set; }
	public static GameObject RightHandBase { get; private set; }
	public static GameObject Compass { get; private set; }

	// Shaders
	public static Shader LitShader { get; private set; }
	public static Shader UnlitShader { get; private set; }

	public static Shader FlipYAxisShader { get; private set; }
	public static Shader ShowDepthTexture { get; private set; }

	// Materials (just easier access to the shaders)
	public static Material FlipYAxisMaterial { get; private set; }
	public static Material ShowDepthMaterial { get; private set; }

	// Primitive shapes (not actually loaded, just generated when we start)
	public static Mesh PrimitiveQuad { get; private set; }
	public static Mesh DoubleSidedQuad { get; private set; }
	public static Mesh PrimitiveCylinder { get; private set; }
	public static Mesh PrimitiveSphere { get; private set; }

	// All our loaded control icons
	private static readonly Dictionary<string, Texture2D> _icons = new();

	public AssetLoader()
	{
		var bundle = AssetBundle.LoadFromFile(Path.Combine(DredgeVRCore.ModPath, "AssetBundles/dredgevr"));

		FlipYAxisShader = LoadAsset<Shader>(bundle, "FlipYAxis.shader");
		ShowDepthTexture = LoadAsset<Shader>(bundle, "ShowDepthTexture.shader");
		// This shader is taken out of the game and works okay
		// A lot of shaders in game aren't accessible using Shader.Find for some reason, return null even if they obviously exist
		// Also some standard unity shaders are missing, because Dredge uses URP
		LitShader = Shader.Find("Shader Graphs/Lit_Shader");
		UnlitShader = LoadAsset<Shader>(bundle, "Scenes/Unlit.shader");

		PrimitiveQuad = CreatePrimitiveMesh(PrimitiveType.Quad);
		DoubleSidedQuad = GeometryHelper.MakeMeshDoubleFaced(PrimitiveQuad);
		PrimitiveCylinder = CreatePrimitiveMesh(PrimitiveType.Cylinder);
		PrimitiveSphere = CreatePrimitiveMesh(PrimitiveType.Sphere);

		FlipYAxisMaterial = new Material(FlipYAxisShader);
		ShowDepthMaterial = new Material(ShowDepthTexture);

		// Put lit shader on hands
		LeftHandBase = LoadAsset<GameObject>(bundle, "SteamVR/Prefabs/vr_glove_left.prefab");
		RightHandBase = LoadAsset<GameObject>(bundle, "SteamVR/Prefabs/vr_glove_right.prefab");

		// Compass needs materials and stuff
		Compass = LoadAsset<GameObject>(bundle, "Prefabs/Compass/compass.prefab");
		foreach (var meshRenderer in Compass.GetComponentsInChildren<MeshRenderer>())
		{
			SwapMaterialToLit(meshRenderer);
		}
	}

	private void SwapMaterialToLit(MeshRenderer meshRenderer)
	{
		// Swap the unity main texture over and use the lit shader
		var texture = meshRenderer.material.mainTexture;
		meshRenderer.SetMaterial(new Material(LitShader)).material.SetTexture(LitShaderAlbedo, texture);
	}

	public int LitShaderAlbedo = Shader.PropertyToID("Texture2D_9aa7ba2263944b48bbf43c218dc48459");
	public int LitShaderEmission = Shader.PropertyToID("Texture2D_c7b8c5c57d6443a5a9f86b68269754f3");

	private Mesh CreatePrimitiveMesh(PrimitiveType primitiveType)
	{
		var primitive = GameObject.CreatePrimitive(primitiveType);
		var primitiveMesh = primitive.GetComponent<MeshFilter>().mesh;
		GameObject.Destroy(primitive);
		return primitiveMesh;
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

	public static Texture2D CreateTexture(Color colour)
	{
		var texture = new Texture2D(1, 1);
		texture.SetPixel(1, 1, colour);
		texture.Apply();
		return texture;
	}
}
