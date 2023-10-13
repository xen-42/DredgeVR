using DredgeVR.Helpers;
using DredgeVR.VRCamera;
using DredgeVR.VRInput;
using DredgeVR.VRUI;
using DredgeVR.World;
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

		public static Action<string> SceneStart;
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

			gameObject.AddComponent<VRInputManager>();
			gameObject.AddComponent<VRInputModule>();
			gameObject.AddComponent<VRUIManager>();
			gameObject.AddComponent<WorldManager>();

			SceneManager.activeSceneChanged += OnActiveSceneChanged;
			OnActiveSceneChanged(default, SceneManager.GetActiveScene());
		}

		public void OnDestroy()
		{
			SceneManager.activeSceneChanged -= OnActiveSceneChanged;
		}

		private void OnActiveSceneChanged(Scene arg0, Scene arg1)
		{
			SceneStart?.Invoke(arg1.name);

			if (arg1.name == "Game")
			{
				GameSceneStart?.Invoke();
			}

			if (arg1.name == "Title")
			{
				TitleSceneStart?.Invoke();
			}
		}
	}
}
