using UnityEngine;

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
}
