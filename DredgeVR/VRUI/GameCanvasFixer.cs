using DredgeVR.VRCamera;
using UnityEngine;

namespace DredgeVR.VRUI;

internal class GameCanvasFixer : MonoBehaviour
{
	// Public for testing in UnityExplorer
	public Vector3 Offset = new (0f, 1f, 2f);

	public void Start()
	{

	}

	public void Update()
	{
		var uiParent = VRCameraManager.ResetTransform;

		if (uiParent != null)
		{
			transform.position = uiParent.transform.TransformPoint(Offset);
			transform.rotation = Quaternion.Euler(0f, uiParent.rotation.eulerAngles.y, 0f);
			transform.localScale = Vector3.one * 0.001f;
		}
	}
}