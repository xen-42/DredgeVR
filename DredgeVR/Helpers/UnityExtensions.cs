using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace DredgeVR.Helpers;

public static class UnityExtensions
{
	/// <summary>
	/// Sets the parent of the game object and takes on its position and rotation
	/// </summary>
	/// <param name="go"></param>
	/// <param name="parent"></param>
	/// <returns></returns>
	public static GameObject SetParent(this GameObject go, Transform parent)
	{
		go.transform.parent = parent;
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.identity;
		return go;
	}


	public static GameObject SetParentPreserveLocal(this GameObject go, Transform parent)
	{
		var localPos = go.transform.localPosition;
		var localRot = go.transform.localRotation;
		go.transform.parent = parent;
		go.transform.localPosition = localPos;
		go.transform.localRotation = localRot;
		return go;
	}

	public static Renderer SetMaterial(this Renderer renderer, Material material)
	{
		renderer.material = material;
		return renderer;
	}

	public static T GetAddComponent<T>(this GameObject go) where T : Component
	{
		return go.GetComponent<T>() ?? go.AddComponent<T>();
	}
}
