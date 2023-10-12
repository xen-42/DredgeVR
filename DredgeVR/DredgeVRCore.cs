using DredgeVR.VRCamera;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Winch.Core;

namespace DredgeVR
{
	public class DredgeVRCore : MonoBehaviour
	{
		public static DredgeVRCore Instance { get; private set; }

		public static Action GameSceneStart;

		public void Awake()
		{
			Instance = this;
			WinchCore.Log.Debug($"{nameof(DredgeVRCore)} has loaded!");
			SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
		}

		public void OnDestroy()
		{
			SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
		}

		private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
		{
			// Heightmap terrain causes massive lag in VR
			foreach (var terrain in GameObject.FindObjectsOfType<Terrain>())
			{
				terrain.heightmapMaximumLOD = Mathf.Max(terrain.heightmapMaximumLOD, 1);
			}

			if (arg1.name == "Game")
			{
				GameSceneStart?.Invoke();
			}
		}

		public void Start()
		{
			Camera.main.gameObject.AddComponent<VRCameraManager>();
		}
	}
}
