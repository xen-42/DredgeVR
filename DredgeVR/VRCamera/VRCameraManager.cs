using Cinemachine.Utility;
using DredgeVR.Helpers;
using DredgeVR.Options;
using DredgeVR.VRInput;
using DredgeVR.VRUI;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Valve.VR;

namespace DredgeVR.VRCamera;

public class VRCameraManager : MonoBehaviour
{
	public static VRCameraManager Instance { get; private set; }
	public static SteamVR_TrackedObject VRPlayer { get; private set; }

	public static VRHand LeftHand { get; private set; }
	public static VRHand RightHand { get; private set; }

	public static Transform AnchorTransform { get; private set; }
	private Transform _pivot, _root;

	private float _gameAnchorYPosition = 0.8f;

	public void Awake()
	{
		Instance = this;

		var cameras = GetComponentsInChildren<Camera>();

		var leftCamera = cameras[0];
		leftCamera.transform.parent = transform;
		leftCamera.transform.localPosition = Vector3.zero;
		leftCamera.transform.localRotation = Quaternion.identity;
		leftCamera.gameObject.AddComponent<EyeCamera>().left = true;

		var rightCamera = cameras[1];
		rightCamera.transform.parent = transform;
		rightCamera.transform.localPosition = Vector3.zero;
		rightCamera.transform.localRotation = Quaternion.identity;
		rightCamera.gameObject.AddComponent<EyeCamera>().left = false;

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

		//new GameObject(nameof(SceneLoadFade), new Type[] { typeof(SceneLoadFade) }).SetParent(transform);

		DredgeVRCore.SceneStart += OnSceneStart;
		DredgeVRCore.TitleSceneStart += OnTitleSceneStart;
		DredgeVRCore.PlayerSpawned += OnPlayerSpawned;

		gameObject.AddComponent<RenderToScreen>();

		var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
		var dataLists = urp.GetValue<ScriptableRendererData[]>("m_RendererDataList");

		// This makes the camera not upsidedown wtf
		// Other ways of not being upside-down mess with the haste smoke flame effects (and probably others)
		// First in the dataLists is the Forward renderer
		var renderObject = new RenderObjects { name = "Flip" };
		Delay.FireOnNextUpdate(() => renderObject.GetValue<RenderObjectsPass>("renderObjectsPass").renderPassEvent = RenderPassEvent.AfterRendering);
		dataLists.First().rendererFeatures.Insert(0, renderObject);
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

		// Bloom is weirdly mirrored since we switched away from using the default camera setup
		// Disable it in every scene
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
		if (AnchorTransform != null)
		{
			// There's also a VR control binding for this, but in case they don't have enough buttons I put it on space
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
