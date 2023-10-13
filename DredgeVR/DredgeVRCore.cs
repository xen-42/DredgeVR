using DredgeVR.Helpers;
using DredgeVR.VRCamera;
using DredgeVR.VRInput;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using Winch.Core;

namespace DredgeVR
{
	public class DredgeVRCore : MonoBehaviour
	{
		public static DredgeVRCore Instance { get; private set; }

		public static Action GameSceneStart;
		public static Action TitleSceneStart;

		public static string ModPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

		public void Awake()
		{
			Instance = this;
			WinchCore.Log.Debug($"{nameof(DredgeVRCore)} has loaded!");

			new AssetLoader();

			// Dredge uses one camera for all time and between scenes, which is nice
			Camera.main.gameObject.AddComponent<VRCameraManager>();
			Camera.main.gameObject.AddComponent<VRInputManager>();

			SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
			SceneManager_activeSceneChanged(default, SceneManager.GetActiveScene());
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

			if (arg1.name == "Title")
			{
				TitleSceneStart?.Invoke();

				var canvas = GameObject.Find("Canvases");

				canvas.transform.position = new Vector3(-5.4f, 0.35f, 1.6f);
				canvas.transform.rotation = Quaternion.Euler(0, 70, 0);
				canvas.transform.localScale = Vector3.one * 0.002f;
			}

			foreach (var canvas in GameObject.FindObjectsOfType<Canvas>())
			{
				canvas.renderMode = RenderMode.WorldSpace;
				canvas.worldCamera = VRCameraManager.Camera;
				canvas.scaleFactor = 1f;
				canvas.planeDistance = 1;
			}
		}
	}
}
