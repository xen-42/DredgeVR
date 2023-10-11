using DredgeVR.Helpers;
using HarmonyLib;
using InControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR;
using Valve.VR;
using Winch.Core;

namespace DredgeVR
{
	public class Loader
	{
		/// <summary>
		/// This method is run by Winch to initialize your mod
		/// </summary>
		public static void Initialize()
		{
			new Harmony("DredgeVR").PatchAll();

			var gameObject = new GameObject(nameof(DredgeVRCore));
			gameObject.AddComponent<DredgeVRCore>();
			GameObject.DontDestroyOnLoad(gameObject);

			SetUpXr();
			//DredgeVRCore.Instance.StartCoroutine(SetUpXREnumerable());
		}

		// Thank you PinkMilkProductions https://github.com/PinkMilkProductions/VRath/tree/master
		private static IEnumerator SetUpXREnumerable()
		{
			SteamVR_Actions.PreInitialize();

			var generalSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
			var managerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
			var xrLoader = ScriptableObject.CreateInstance<OpenVRLoader>();

			generalSettings.SetValue("m_InitManagerOnStart", true);

			var settings = OpenVRSettings.GetSettings();
			settings.StereoRenderingMode = OpenVRSettings.StereoRenderingModes.MultiPass;

			generalSettings.Manager = managerSettings;

#pragma warning disable CS0618 // Type or member is obsolete
			managerSettings.loaders.Clear();
			managerSettings.loaders.Add(xrLoader);
#pragma warning restore CS0618 // Type or member is obsolete

			managerSettings.InitializeLoaderSync();

			typeof(XRGeneralSettings)
			.GetMethod("AttemptInitializeXRSDKOnLoad", BindingFlags.Static | BindingFlags.NonPublic)
			.Invoke(null, new object[] { });

			typeof(XRGeneralSettings)
			.GetMethod("AttemptStartXRSDKOnBeforeSplashScreen", BindingFlags.Static | BindingFlags.NonPublic)
			.Invoke(null, new object[] { });

			SteamVR.Initialize(true);
			WinchCore.Log.Info("SteamVR Initialized");

			WinchCore.Log.Info("SteamVR hmd modelnumber: " + SteamVR.instance.hmd_ModelNumber);

			var displays = new List<XRDisplaySubsystem>();
			SubsystemManager.GetInstances(displays);
			var myDisplay = displays[0];
			myDisplay.Start();
			WinchCore.Log.Info("Display started");

			yield return null;

		}

		private static void SetUpXr()
		{
			SteamVR_Actions.PreInitialize();

			var generalSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
			generalSettings.SetValue("m_InitManagerOnStart", true);
			var managerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
			var features = new OpenXRInteractionFeature[]
			{
			ScriptableObject.CreateInstance<HTCViveControllerProfile>(),
			ScriptableObject.CreateInstance<OculusTouchControllerProfile>(),
			ScriptableObject.CreateInstance<MicrosoftMotionControllerProfile>(),
			ScriptableObject.CreateInstance<ValveIndexControllerProfile>()
			};
			var xrLoader = ScriptableObject.CreateInstance<OpenVRLoader>();
			OpenXRSettings.Instance.renderMode = OpenXRSettings.RenderMode.MultiPass;
			OpenXRSettings.Instance.SetValue("features", features);
			foreach (var feature in features) feature.enabled = true;

			generalSettings.Manager = managerSettings;
#pragma warning disable CS0618
			/*
			 * ManagerSettings.loaders is deprecated but very useful, allows me to add the xr loader without reflection.
			 * Should be fine unless the game's Unity version gets majorly updated, in which case the whole mod will be
			 * broken, so I'll have to update it anyway.
			 */
			managerSettings.loaders.Add(xrLoader);
#pragma warning restore CS0618

			managerSettings.InitializeLoaderSync();
			if (managerSettings.activeLoader == null) throw new Exception("Cannot initialize OpenVR Loader");

			managerSettings.StartSubsystems();
		}
	}
}