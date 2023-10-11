using Cinemachine.Utility;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;
using Winch.Core;

namespace DredgeVR
{
	public class DredgeVRCore : MonoBehaviour
	{
		public static DredgeVRCore Instance { get; private set; }

		public static Action GameSceneStart;

		public static SteamVR_TrackedObject VRPlayer { get; private set; }

		public void Awake()
		{
			Instance = this;
			WinchCore.Log.Debug($"{nameof(DredgeVRCore)} has loaded!");
			SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
		}

		private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
		{
			// Heightmap terrain causes massive lag in VR
			foreach (var terrain in GameObject.FindObjectsOfType<Terrain>())
			{
				terrain.heightmapMaximumLOD = Mathf.Max(terrain.heightmapMaximumLOD, 1);
			}

			foreach (var particle in GameObject.FindObjectsOfType<ParticleSystem>())
			{
				particle.enableEmission = false;
			}

			if (arg1.name == "Game")
			{
				GameSceneStart?.Invoke();
			}
		}

		public void Start()
		{
			var mainCamera = Camera.main;
			VRPlayer = mainCamera.gameObject.AddComponent<SteamVR_TrackedObject>();
		}

		public void Update()
		{
			Camera.main.fieldOfView = SteamVR.instance.fieldOfView;

			if (GameManager.Instance?.Player != null && VRPlayer.origin == null)
			{
				var pivot = new GameObject("Pivot");
				pivot.transform.parent = GameManager.Instance.Player.transform;
				pivot.transform.localPosition = new Vector3(0, 1, -2);
				pivot.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
				VRPlayer.origin = pivot.transform;
			}

			if (VRPlayer.origin != null)
			{
				// Don't take boat pitch rotation because that is turbo motion sickness
				var forwardOnPlane = GameManager.Instance.Player.transform.forward.ProjectOntoPlane(Vector3.up);
				VRPlayer.origin.transform.rotation = Quaternion.FromToRotation(Vector3.back, forwardOnPlane);
			}
		}
	}
}
