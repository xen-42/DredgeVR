﻿using Cinemachine;
using DredgeVR.Helpers;
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
		public static Action PlayerSpawned;

		public static string ModPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

		public void Awake()
		{
			Instance = this;
			DredgeVRLogger.Debug($"{nameof(DredgeVRCore)} has loaded!");

			new AssetLoader();

			// This thing tries to take over and breaks our tracking
			Component.DestroyImmediate(Camera.main.GetComponent<CinemachineBrain>());

			// Dredge uses one camera for all time which is nice
			var head = new GameObject("Head");
			Camera.main.gameObject.transform.parent = head.transform;
			GameObject.Instantiate(Camera.main.gameObject, head.transform);
			head.AddComponent<VRCameraManager>();

			gameObject.AddComponent<VRInputManager>();
			gameObject.AddComponent<VRInputModule>();
			gameObject.AddComponent<VRUIManager>();
			gameObject.AddComponent<WorldManager>();
			gameObject.AddComponent<VRTutorialManager>();

			Delay.FireOnNextUpdate(() =>
			{
				SceneManager.activeSceneChanged += OnActiveSceneChanged;
				OnActiveSceneChanged(default, SceneManager.GetActiveScene());
			});
		}

		public void OnDestroy()
		{
			SceneManager.activeSceneChanged -= OnActiveSceneChanged;
		}

		private void OnActiveSceneChanged(Scene arg0, Scene arg1)
		{
			DredgeVRLogger.Debug($"Loading into scene {arg1.name}");

			// Stop all the coroutines set in Delay
			StopAllCoroutines();

			SceneStart?.Invoke(arg1.name);

			switch (arg1.name)
			{
				case "Game":
					GameSceneStart?.Invoke();
					Delay.RunWhen(() => GameManager.Instance.Player != null, () => PlayerSpawned?.Invoke());
					break;
				case "Title":
					TitleSceneStart?.Invoke();
					break;
				case "IntroCutscene":
					IntroCutsceneStart?.Invoke();
					break;
			}
		}
	}
}
