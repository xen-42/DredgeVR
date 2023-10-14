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
	public static Camera Camera { get; private set; }

	public static VRHand LeftHand { get; private set; }
	public static VRHand RightHand { get; private set; }

	public static Transform ResetTransform { get; private set; }
	private Transform _pivot;

	public void Awake()
	{
		Instance = this;

		Camera = GetComponent<Camera>();

		// This thing tries to take over and breaks our tracking
		GetComponent<CinemachineBrain>().enabled = false;

		// Adds tracking to the head
		VRPlayer = gameObject.AddComponent<SteamVR_TrackedObject>();

		// Set up hands
		LeftHand = new GameObject("LeftHand").AddComponent<VRHand>();
		LeftHand.hand = SteamVR_Input_Sources.LeftHand;

		RightHand = new GameObject("RightHand").AddComponent<VRHand>();
		RightHand.hand = SteamVR_Input_Sources.RightHand;

		// Parent everything to a new "pivot" object
		_pivot = new GameObject("VRCameraPivot").transform;
		VRPlayer.origin = _pivot;

		transform.parent = _pivot;
		LeftHand.transform.parent = _pivot;
		RightHand.transform.parent = _pivot;

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

		ResetTransform = new GameObject("ResetTransform").transform;
		ResetTransform.position = new Vector3(-6.5f, 0.5f, 0);
		ResetTransform.LookAt(worldPos);

		Delay.FireOnNextUpdate(ResetPosition);
	}

	private void OnGameSceneStart()
	{
		// Boat takes a frame to exist
		Delay.RunWhen(
			() => GameManager.Instance.Player != null,
			() =>
			{
				// Make the player follow the boat
				ResetTransform = new GameObject("ResetTransform").transform;
				ResetTransform.parent = GameManager.Instance.Player.transform;
				ResetTransform.localPosition = new Vector3(0, 1, -2);
				ResetTransform.localRotation = Quaternion.identity;

				Delay.FireOnNextUpdate(ResetPosition);
			}
		);
	}

	public void Update()
	{
		Camera.fieldOfView = SteamVR.instance.fieldOfView;

		if (ResetTransform != null)
		{
			// Don't take on origin pitch rotation because that is turbo motion sickness
			var forwardOnPlane = ResetTransform.forward.ProjectOntoPlane(Vector3.up);
			VRPlayer.origin.transform.rotation = Quaternion.FromToRotation(Vector3.back, forwardOnPlane);

			// In the game scene force a constant ResetTransform y position
			// Else you bump into something and dear god
			if (SceneManager.GetActiveScene().name == "Game")
			{
				ResetTransform.position = new Vector3(ResetTransform.position.x, 0.66f, ResetTransform.position.z);
			}

			VRPlayer.origin.transform.position = ResetTransform.position;
		}
	}

	public void ResetPosition()
	{
		var rotationAngleY = ResetTransform.rotation.eulerAngles.y - VRPlayer.transform.rotation.eulerAngles.y;
		_pivot.Rotate(0, rotationAngleY, 0);

		var distanceDiff = ResetTransform.position - _pivot.position;
		_pivot.transform.position += distanceDiff;
	}
}
