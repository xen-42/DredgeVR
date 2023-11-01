using UnityEngine;

namespace DredgeVR.Helpers;

public static class UnityExtensions
{
	public static GameObject SetParent(this GameObject go, Transform parent)
	{
		go.transform.parent = parent;
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.identity;
		return go;
	}
}
