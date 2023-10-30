using Cinemachine.Utility;
using DredgeVR.Helpers;
using DredgeVR.Options;
using DredgeVR.VRInput;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using Valve.VR;

namespace DredgeVR.VRCamera;

public class VRCameraManager : MonoBehaviour
{
	public static VRCameraManager Instance { get; private set; }
	public static SteamVR_TrackedObject VRPlayer { get; private set; }
	private static Camera _leftCamera, _rightCamera;

	public static VRHand LeftHand { get; private set; }
	public static VRHand RightHand { get; private set; }

	public static Transform AnchorTransform { get; private set; }
	private Transform _pivot, _root;

	private float _gameAnchorYPosition = 0.8f;

	private XRDisplaySubsystem _displaySubsystem;

	public void Awake()
	{
		Instance = this;

		var cameras = GetComponentsInChildren<Camera>();

		_leftCamera = cameras[0];
		_leftCamera.transform.parent = transform;
		_leftCamera.transform.localPosition = Vector3.zero;
		_leftCamera.transform.localRotation = Quaternion.identity;

		_rightCamera = cameras[1];
		_rightCamera.transform.parent = transform;
		_rightCamera.transform.localPosition = Vector3.zero;
		_rightCamera.transform.localRotation = Quaternion.identity;

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

		_displaySubsystem = SteamVRHelper.GetSubSystem<XRDisplaySubsystem>();

		// Cameras have to be inverted
		GL.invertCulling = true;
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

		// Bloom is weirdly mirrored
		foreach (var volume in GameObject.FindObjectsOfType<Volume>())
		{
			if (volume.profile.components.FirstOrDefault(x => x is Bloom) is Bloom bloom)
			{
				bloom.intensity.value = 0f;
			}
		}

		// Weird timing on this
		Delay.FireInNUpdates(2, RecenterCamera);
	}

	private void OnTitleSceneStart()
	{
		// Reflections look super weird in VR - Make sure its off when we load in
		// GameObject.Find("ReflectionCamera")?.gameObject?.SetActive(false);

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

	public void LateUpdate()
	{
		// Have to set target texture in late update else it lags 
		_leftCamera.aspect = SteamVR.instance.aspect;
		_leftCamera.fieldOfView = SteamVR.instance.fieldOfView;
		_leftCamera.stereoTargetEye = StereoTargetEyeMask.Left;
		_leftCamera.projectionMatrix = _leftCamera.GetStereoNonJitteredProjectionMatrix(Camera.StereoscopicEye.Left);
		_leftCamera.targetTexture = _displaySubsystem.GetRenderTextureForRenderPass(0);

		_rightCamera.aspect = SteamVR.instance.aspect;
		_rightCamera.fieldOfView = SteamVR.instance.fieldOfView;
		_rightCamera.stereoTargetEye = StereoTargetEyeMask.Right;
		_rightCamera.projectionMatrix = _rightCamera.GetStereoNonJitteredProjectionMatrix(Camera.StereoscopicEye.Right);
		_rightCamera.targetTexture = _displaySubsystem.GetRenderTextureForRenderPass(1);

		// Idk why but when doing target texture everything is backwards
		_leftCamera.projectionMatrix *= Matrix4x4.Scale(new Vector3(1, -1, 1));
		_rightCamera.projectionMatrix *= Matrix4x4.Scale(new Vector3(1, -1, 1));
	}

	public void Update()
	{
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
