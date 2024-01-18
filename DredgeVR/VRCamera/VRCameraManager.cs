using Cinemachine;
using Cinemachine.Utility;
using DredgeVR.Helpers;
using DredgeVR.Options;
using DredgeVR.VRInput;
using DredgeVR.VRUI;
using HarmonyLib;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Valve.VR;

namespace DredgeVR.VRCamera;

[HarmonyPatch]
public class VRCameraManager : MonoBehaviour
{
	public static VRCameraManager Instance { get; private set; }
	public static SteamVR_TrackedObject VRPlayer { get; private set; }

	public static VRHand LeftHand { get; private set; }
	public static VRHand RightHand { get; private set; }

	public static EyeCamera LeftEye { get; private set; }
	public static EyeCamera RightEye { get; private set; }

	public static Transform AnchorTransform { get; private set; }
	private Transform _pivot, _rotationPivot, _root;

	public float minX = 0.5f;
	private bool _justTurned;

	public bool InCutscene;

	public void Awake()
	{
		Instance = this;

		var cameras = GetComponentsInChildren<Camera>();

		var leftCamera = cameras[0];
		leftCamera.transform.parent = transform;
		leftCamera.transform.localPosition = Vector3.zero;
		leftCamera.transform.localRotation = Quaternion.identity;
		leftCamera.nearClipPlane *= OptionsManager.Options.playerScale;
		LeftEye = leftCamera.gameObject.AddComponent<EyeCamera>();
		LeftEye.left = true;

		var rightCamera = cameras[1];
		rightCamera.transform.parent = transform;
		rightCamera.transform.localPosition = Vector3.zero;
		rightCamera.transform.localRotation = Quaternion.identity;
		rightCamera.nearClipPlane *= OptionsManager.Options.playerScale;
		RightEye = rightCamera.gameObject.AddComponent<EyeCamera>();
		RightEye.left = false;

		// Adds tracking to the head
		VRPlayer = gameObject.AddComponent<SteamVR_TrackedObject>();

		LeftHand = GameObject.Instantiate(AssetLoader.LeftHandBase).AddComponent<VRHand>();
		RightHand = GameObject.Instantiate(AssetLoader.RightHandBase).AddComponent<VRHand>();

		LeftHand.hand = SteamVR_Input_Sources.LeftHand;
		RightHand.hand = SteamVR_Input_Sources.RightHand;

		// Parent everything to a new "pivot" object
		_root = new GameObject("PlayerRoot").transform;
		_rotationPivot = new GameObject("Rotation").SetParent(_root).transform;
		_pivot = new GameObject("VRCameraPivot").SetParent(_rotationPivot).transform;
		VRPlayer.origin = _pivot;

		transform.parent = _pivot;
		LeftHand.transform.parent = _pivot;
		RightHand.transform.parent = _pivot;

		DredgeVRCore.SceneStart += OnSceneStart;
		DredgeVRCore.TitleSceneStart += OnTitleSceneStart;
		DredgeVRCore.PlayerSpawned += OnPlayerSpawned;

		gameObject.AddComponent<RenderToScreen>();

		var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
		// Disable depth texture since its upsidedown
		urp.supportsCameraDepthTexture = false;

		var dataLists = urp.GetValue<ScriptableRendererData[]>("m_RendererDataList");

		// This makes the camera not upsidedown wtf
		// Other ways of not being upside-down mess with the haste smoke flame effects (and probably others)
		// First in the dataLists is the Forward renderer
		var renderObject = new RenderObjects { name = "Flip" };
		Delay.FireOnNextUpdate(() => renderObject.GetValue<RenderObjectsPass>("renderObjectsPass").renderPassEvent = RenderPassEvent.AfterRendering);
		dataLists.First().rendererFeatures.Insert(0, renderObject);

		// Rescale the character
		_pivot.transform.localScale = Vector3.one * OptionsManager.Options.playerScale;
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
		if (DLCHelper.OwnsThePaleReach())
		{
			var camLookAt = GameObject.Find("DLC1Positions/DLC1CamLookAt").transform.position.ProjectOntoPlane(Vector3.up);

			AnchorTransform.position = new Vector3(-90.6f, 1f, -1337.3f);
			AnchorTransform.LookAt(camLookAt);

			// Move snow particles to the anchor position
			GameObject.Find("VCam/Snow").transform.position = AnchorTransform.position;
		}
		else
		{
			// Make the player look towards the lighthouse
			var lightHouse = GameObject.Find("TheMarrows/Islands/LittleMarrow").transform;
			var worldPos = new Vector3(lightHouse.position.x, 0.5f, lightHouse.position.z);

			AnchorTransform.position = new Vector3(-6.5f, 0.5f, 0);
			AnchorTransform.LookAt(worldPos);
		}
	}

	private void OnPlayerSpawned()
	{
		InCutscene = false;

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
				if (InCutscene)
				{

				}
				else
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
			}

			_root.transform.position = AnchorTransform.position;
			_root.transform.rotation = AnchorTransform.rotation;

			UpdateCameraRotation();
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

	public void ResetAnchorToBoat()
	{
		if (InCutscene) return;

		AnchorTransform.localPosition = OptionsManager.Options.PlayerPosition;

		// Helps when you ram into stuff to not bounce around
		if (OptionsManager.Options.lockCameraYPosition)
		{
			// Subtract 0.33 from the local height to go from local to global
			var lockedY = OptionsManager.Options.PlayerPosition.y - 0.33f;
			AnchorTransform.position = new Vector3(AnchorTransform.position.x, lockedY, AnchorTransform.position.z);
		}
	}

	private void UpdateCameraRotation()
	{
		var x = VRInputManager.RightThumbStick.x;
		var sign = Mathf.Sign(x);
		var magnitude = Mathf.Clamp01((Mathf.Abs(x) - minX) / (1f - minX));

		if (OptionsManager.Options.smoothRotation)
		{
			if (magnitude > 0)
			{
				_rotationPivot.Rotate(0, sign * magnitude * 180f * Time.unscaledDeltaTime, 0);
			}
		}
		else
		{
			if (magnitude > 0)
			{
				if (!_justTurned)
				{
					_rotationPivot.Rotate(0, sign * 45f, 0);
					_justTurned = true;
				}
			}
			else
			{
				_justTurned = false;
			}
		}
	}

	private void OnCutToCredits()
	{
		InCutscene = true;
		AnchorTransform.parent = null;
		AnchorTransform.transform.position = new Vector3(18, 7, 4);
		AnchorTransform.transform.rotation = Quaternion.Euler(0, 270, 0);

		// Rain only falls over the player, move it to our camera
		foreach (var followPlayer in GameObject.FindObjectsOfType<FollowPlayerInWorld>())
		{
			followPlayer.playerRef = AnchorTransform;
		}

		// The post processing here is way too intense
		// Will also want to tone down/disable lightning too
		foreach (var volume in GameObject.FindObjectsOfType<Volume>())
		{
			volume.enabled = false;
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(FinaleCutsceneLogic), nameof(FinaleCutsceneLogic.CutToCredits))]
	public static void FinaleCutsceneLogic_CutToCredits() => Instance.OnCutToCredits();

	/*
	[HarmonyPostfix]
	[HarmonyPatch(typeof(CinematicCamera), nameof(CinematicCamera.Play))]
	public static void CinematicCamera_Play(CinematicCamera __instance)
	{
		// Final cutscene has its own separate logic
		// This is for TPR
		if (!Instance._inFinaleCutscene)
		{
			Instance.StartCoroutine(Instance.WaitForCinematicToFinish(__instance.virtualCamera));
			VRUIManager.HideHeldUI();
		}
	}

	private IEnumerator WaitForCinematicToFinish(CinemachineVirtualCamera camera)
	{
		while(camera.enabled)
		{
			AnchorTransform.position = camera.transform.position;
			yield return new WaitForFixedUpdate();
		}

		// Go back to regular camera
		ResetAnchorToBoat();
		VRUIManager.ShowHeldUI();
	}
	*/


}
