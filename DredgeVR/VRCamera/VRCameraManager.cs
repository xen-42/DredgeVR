using Cinemachine.Utility;
using DG.Tweening;
using UnityEngine;
using Valve.VR;

namespace DredgeVR.VRCamera;

[RequireComponent(typeof(Camera))]
public class VRCameraManager : MonoBehaviour
{
	public static SteamVR_TrackedObject VRPlayer { get; private set; }
	public static Camera Camera { get; private set; }

	private Transform _resetTransform, _pivot;

	public void Start()
	{
		Camera = GetComponent<Camera>();
		VRPlayer = gameObject.AddComponent<SteamVR_TrackedObject>();

		DredgeVRCore.TitleSceneStart += OnTitleSceneStart;
		DredgeVRCore.GameSceneStart += OnGameSceneStart;
	}

	public void OnDestroy()
	{
		DredgeVRCore.TitleSceneStart -= OnGameSceneStart;
		DredgeVRCore.GameSceneStart -= OnGameSceneStart;
	}

	private void OnTitleSceneStart()
	{
		// Make the player look towards the lighthouse
		var lightHouse = GameObject.Find("TheMarrows/Islands/LittleMarrow").transform;
		var worldPos = new Vector3(lightHouse.position.x, 0.5f, lightHouse.position.z);

		_resetTransform = new GameObject("ResetTransform").transform;
		_resetTransform.position = new Vector3(-6.5f, 0.5f, 0);
		_resetTransform.LookAt(worldPos);

		_pivot = new GameObject("VRCameraPivot").transform;
		VRPlayer.origin = _pivot;

		ResetPosition();
	}

	private void OnGameSceneStart()
	{
		// Make the player follow the boat
		_resetTransform = new GameObject("ResetTransform").transform;
		_resetTransform.parent = GameManager.Instance.Player.transform;
		_resetTransform.position = new Vector3(0, 1, -2);
		_resetTransform.rotation = Quaternion.identity;

		_pivot = new GameObject("VRCameraPivot").transform;
		VRPlayer.origin = _pivot;

		ResetPosition();
	}

	public void Update()
	{
		Camera.fieldOfView = SteamVR.instance.fieldOfView;

		if (VRPlayer?.origin?.parent != null)
		{
			// Don't take on origin pitch rotation because that is turbo motion sickness
			var forwardOnPlane = VRPlayer.origin.parent.forward.ProjectOntoPlane(Vector3.up);
			VRPlayer.origin.transform.rotation = Quaternion.FromToRotation(Vector3.back, forwardOnPlane);
		}
	}

	public void ResetPosition()
	{
		var rotationAngleY = _resetTransform.rotation.eulerAngles.y - VRPlayer.transform.rotation.eulerAngles.y;
		_pivot.Rotate(0, rotationAngleY, 0);

		var distanceDiff = _resetTransform.position - _pivot.position;
		_pivot.transform.position += distanceDiff;
	}
}
