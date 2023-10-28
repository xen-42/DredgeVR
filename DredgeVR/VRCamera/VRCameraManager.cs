using Cinemachine.Utility;
using DredgeVR.Helpers;
using DredgeVR.Options;
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
	private Transform _pivot, _root;

	private float _gameAnchorYPosition = 0.8f;

	public void Awake()
	{
		Instance = this;

		_camera = GetComponent<Camera>();

		// Adds tracking to the head
		VRPlayer = gameObject.AddComponent<SteamVR_TrackedObject>();

		LeftHand = GameObject.Instantiate(AssetLoader.LeftHandBase).AddComponent<VRHand>();
		RightHand = GameObject.Instantiate(AssetLoader.RightHandBase).AddComponent<VRHand>();

		LeftHand.hand = SteamVR_Input_Sources.LeftHand;
		RightHand.hand = SteamVR_Input_Sources.RightHand;

		// Parent everything to a new "pivot" object
		_root = new GameObject("PlayerRoot").transform;
		_pivot = new GameObject("VRCameraPivot").transform;
		_pivot.parent = _root;
		VRPlayer.origin = _pivot;

		transform.parent = _pivot;
		LeftHand.transform.parent = _pivot;
		RightHand.transform.parent = _pivot;

		DredgeVRCore.SceneStart += OnSceneStart;
		DredgeVRCore.TitleSceneStart += OnTitleSceneStart;
		DredgeVRCore.PlayerSpawned += OnPlayerSpawned;
	}

	public void OnDestroy()
	{
		DredgeVRCore.SceneStart -= OnSceneStart;
		DredgeVRCore.TitleSceneStart -= OnTitleSceneStart;
		DredgeVRCore.PlayerSpawned -= OnPlayerSpawned;
	}

	private void OnSceneStart(string _)
	{
		// Always have a AnchorTransform on each scene, then the other events handle positioning it properly if need be

		// Can persist between scenes if it wasn't parented to a game object
		if (AnchorTransform == null)
		{
			AnchorTransform = new GameObject(nameof(AnchorTransform)).transform;
		}

		AnchorTransform.position = Vector3.zero;
		AnchorTransform.rotation = Quaternion.identity;

		// Weird timing on this
		Delay.FireInNUpdates(2, RecenterCamera);
	}

	private void OnTitleSceneStart()
	{
		// Make the player look towards the lighthouse
		var lightHouse = GameObject.Find("TheMarrows/Islands/LittleMarrow").transform;
		var worldPos = new Vector3(lightHouse.position.x, 0.5f, lightHouse.position.z);

		AnchorTransform.position = new Vector3(-6.5f, 0.5f, 0);
		AnchorTransform.LookAt(worldPos);
	}

	private void OnPlayerSpawned()
	{
		// Make the player follow the boat
		AnchorTransform.parent = GameManager.Instance.Player.transform;
		ResetAnchorToBoat();
		AnchorTransform.localRotation = Quaternion.identity;

		Delay.FireOnNextUpdate(RecenterCamera);
	}

	public void Update()
	{
		_camera.fieldOfView = SteamVR.instance.fieldOfView;
		_camera.aspect = SteamVR.instance.aspect;

		if (AnchorTransform != null)
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				RecenterCamera();
			}

			// In the game scene force a constant ResetTransform y position
			// Else you bump into something and dear god
			if (SceneManager.GetActiveScene().name == "Game")
			{
				if (OptionsManager.Options.lockViewToHorizon && AnchorTransform.parent != null)
				{
					// Don't take on origin pitch rotation because that is turbo motion sickness
					var forwardOnPlane = AnchorTransform.parent.forward.ProjectOntoPlane(Vector3.up);
					AnchorTransform.transform.rotation = Quaternion.LookRotation(forwardOnPlane, Vector3.up);
				}

				// If the camera y is locked but the boat is moving around the anchor point gets offset
				if (OptionsManager.Options.lockCameraYPosition && !OptionsManager.Options.removeWaves)
				{
					ResetAnchorToBoat();
				}
			}

			_root.transform.position = AnchorTransform.position;
			_root.transform.rotation = AnchorTransform.rotation;
		}
	}

	public void RecenterCamera()
	{
		if (AnchorTransform != null)
		{
			// Can be moved around by the waves
			if (SceneManager.GetActiveScene().name == "Game")
			{
				ResetAnchorToBoat();
			}

			var rotationAngleY = VRPlayer.transform.rotation.eulerAngles.y - AnchorTransform.rotation.eulerAngles.y;
			_pivot.Rotate(0, -rotationAngleY, 0);

			var distanceDiff = VRPlayer.transform.position - AnchorTransform.position;
			_pivot.position -= new Vector3(distanceDiff.x, 0f, distanceDiff.z);
		}
	}

	private void ResetAnchorToBoat()
	{
		AnchorTransform.localPosition = new Vector3(0, _gameAnchorYPosition + 0.33f, -1.5f);

		// Helps when you ram into stuff to not bounce around
		if (OptionsManager.Options.lockCameraYPosition)
		{
			AnchorTransform.position = new Vector3(AnchorTransform.position.x, _gameAnchorYPosition, AnchorTransform.position.z);
		}
	}
}
