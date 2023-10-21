using DredgeVR.Helpers;
using DredgeVR.Options;
using DredgeVR.Tutorial;
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

		/// <summary>
		/// SceneStart is always invoked before the specific scene start actions, so it can be used for general initialization on each scene
		/// </summary>
		public static Action<string> SceneStart;
		public static Action GameSceneStart;
		public static Action TitleSceneStart;
		public static Action IntroCutsceneStart;

		public static string ModPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

		public void Awake()
		{
			Instance = this;
			DredgeVRLogger.Debug($"{nameof(DredgeVRCore)} has loaded!");

			new AssetLoader();
			OptionsManager.Load();

			// Dredge uses one camera for all time and between scenes, which is nice
			Camera.main.gameObject.AddComponent<VRCameraManager>();

			gameObject.AddComponent<VRInputManager>();
			gameObject.AddComponent<VRInputModule>();
			gameObject.AddComponent<VRUIManager>();
			gameObject.AddComponent<WorldManager>();
			gameObject.AddComponent<VRTutorialManager>();

			SceneManager.activeSceneChanged += OnActiveSceneChanged;
			OnActiveSceneChanged(default, SceneManager.GetActiveScene());
		}

		public void OnDestroy()
		{
			SceneManager.activeSceneChanged -= OnActiveSceneChanged;
		}

		private void OnActiveSceneChanged(Scene arg0, Scene arg1)
		{
			// Stop all the coroutines set in Delay
			StopAllCoroutines();

			SceneStart?.Invoke(arg1.name);

			if (arg1.name == "Game")
			{
				GameSceneStart?.Invoke();
			}

			if (arg1.name == "Title")
			{
				TitleSceneStart?.Invoke();
			}

			if (arg1.name == "IntroCutscene")
			{
				IntroCutsceneStart?.Invoke();
			}
		}
	}
}
