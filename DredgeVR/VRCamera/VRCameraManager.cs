using Cinemachine;
using Cinemachine.Utility;
using DredgeVR.Helpers;
using DredgeVR.VRInput;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;

namespace DredgeVR.VRCamera;

[RequireComponent(typeof(Camera))]
public class VRCameraManager : MonoBehaviour
{
	public static VRCameraManager Instance { get; private set; }
	public static SteamVR_TrackedObject VRPlayer { get; private set; }
	private static Camera _camera;

	public static VRHand LeftHand { get; private set; }
	public static VRHand RightHand { get; private set; }

	public static Transform AnchorTransform { get; private set; }
	private Transform _pivot;

	public void Awake()
	{
		Instance = this;

		_camera = GetComponent<Camera>();

		// This thing tries to take over and breaks our tracking
		GetComponent<CinemachineBrain>().enabled = false;

		// Adds tracking to the head
		VRPlayer = gameObject.AddComponent<SteamVR_TrackedObject>();

		LeftHand = GameObject.Instantiate(AssetLoader.LeftHandBase).AddComponent<VRHand>();
		RightHand = GameObject.Instantiate(AssetLoader.RightHandBase).AddComponent<VRHand>();

		LeftHand.hand = SteamVR_Input_Sources.LeftHand;
		RightHand.hand = SteamVR_Input_Sources.RightHand;

		// Parent everything to a new "pivot" object
		_pivot = new GameObject("VRCameraPivot").transform;
		VRPlayer.origin = _pivot;

		transform.parent = _pivot;
		LeftHand.transform.parent = _pivot;
		RightHand.transform.parent = _pivot;

		DredgeVRCore.SceneStart += OnSceneStart;
		DredgeVRCore.TitleSceneStart += OnTitleSceneStart;
		DredgeVRCore.GameSceneStart += OnGameSceneStart;
	}

	public void OnDestroy()
	{
		DredgeVRCore.SceneStart -= OnSceneStart;
		DredgeVRCore.TitleSceneStart -= OnGameSceneStart;
		DredgeVRCore.GameSceneStart -= OnGameSceneStart;
	}

	private void OnSceneStart(string _)
	{
		// Always have a ResetTransform on each scene, then the other events handle positioning it properly if need be

		// Can persist between scenes if it wasn't parented to a game object
		if (AnchorTransform == null)
		{
			AnchorTransform = new GameObject(nameof(AnchorTransform)).transform;
		}

		AnchorTransform.position = Vector3.zero;
		AnchorTransform.rotation = Quaternion.identity;

		Delay.FireInNUpdates(2, ResetPosition);
	}

	private void OnTitleSceneStart()
	{
		// Make the player look towards the lighthouse
		var lightHouse = GameObject.Find("TheMarrows/Islands/LittleMarrow").transform;
		var worldPos = new Vector3(lightHouse.position.x, 0.5f, lightHouse.position.z);

		AnchorTransform.position = new Vector3(-6.5f, 0.5f, 0);
		AnchorTransform.LookAt(worldPos);
	}

	private void OnGameSceneStart()
	{
		// Boat takes a frame to exist
		Delay.RunWhen(
			() => GameManager.Instance.Player != null,
			() =>
			{
				// Make the player follow the boat
				AnchorTransform.parent = GameManager.Instance.Player.transform;
				AnchorTransform.localPosition = new Vector3(0, 1, -2);
				AnchorTransform.localRotation = Quaternion.identity;

				Delay.FireOnNextUpdate(ResetPosition);
			}
		);
	}

	public void Update()
	{
		_camera.fieldOfView = SteamVR.instance.fieldOfView;
		_camera.aspect = SteamVR.instance.aspect;

		if (AnchorTransform != null)
		{
			// In the game scene force a constant ResetTransform y position
			// Else you bump into something and dear god
			if (SceneManager.GetActiveScene().name == "Game")
			{
				// Don't take on origin pitch rotation because that is turbo motion sickness
				var forwardOnPlane = AnchorTransform.forward.ProjectOntoPlane(Vector3.up);
				VRPlayer.origin.transform.rotation = Quaternion.FromToRotation(Vector3.back, forwardOnPlane);

				AnchorTransform.position = new Vector3(AnchorTransform.position.x, 0.66f, AnchorTransform.position.z);
			}

			VRPlayer.origin.transform.position = AnchorTransform.position;
		}
	}

	public void ResetPosition()
	{
		var rotationAngleY = AnchorTransform.rotation.eulerAngles.y - VRPlayer.transform.rotation.eulerAngles.y;
		_pivot.Rotate(0, rotationAngleY, 0);

		var distanceDiff = AnchorTransform.position - _pivot.position;
		_pivot.transform.position += distanceDiff;
	}
}
