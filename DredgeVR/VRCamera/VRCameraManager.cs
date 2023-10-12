using Cinemachine.Utility;
using UnityEngine;
using Valve.VR;

namespace DredgeVR.VRCamera;

[RequireComponent(typeof(Camera))]
public class VRCameraManager : MonoBehaviour
{
	public static SteamVR_TrackedObject VRPlayer { get; private set; }
	private static Camera _camera;

	public void Start()
	{
		_camera = GetComponent<Camera>();
		VRPlayer = gameObject.AddComponent<SteamVR_TrackedObject>();

		DredgeVRCore.GameSceneStart += OnGameSceneStart;
	}

	public void OnDestroy()
	{
		DredgeVRCore.GameSceneStart -= OnGameSceneStart;
	}

	private void OnGameSceneStart()
	{
		var pivot = new GameObject("VRCameraPivot");
		pivot.transform.parent = GameManager.Instance.Player.transform;
		pivot.transform.localPosition = new Vector3(0, 1, -2);
		pivot.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
		VRPlayer.origin = pivot.transform;
	}

	public void Update()
	{
		_camera.fieldOfView = SteamVR.instance.fieldOfView;

		if (VRPlayer?.origin?.parent != null)
		{
			// Don't take on origin pitch rotation because that is turbo motion sickness
			var forwardOnPlane = VRPlayer.origin.parent.forward.ProjectOntoPlane(Vector3.up);
			VRPlayer.origin.transform.rotation = Quaternion.FromToRotation(Vector3.back, forwardOnPlane);
		}
	}
}
