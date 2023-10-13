using DredgeVR.Helpers;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.XR.Management;
using Valve.VR;

namespace DredgeVR
{
	public class Loader
	{
		public static void Initialize()
		{
			SetUpXr();

			var gameObject = new GameObject(nameof(DredgeVRCore));
			gameObject.AddComponent<DredgeVRCore>();
			GameObject.DontDestroyOnLoad(gameObject);

			new Harmony("DredgeVR").PatchAll();
		}

		private static void SetUpXr()
		{
			SteamVR_Actions.PreInitialize();

			var generalSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
			var managerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();

			var xrLoader = ScriptableObject.CreateInstance<OpenVRLoader>();

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

			// Don't pause because it puts us back on the original camera and kills tracking and becomes turbo sickness
			// SteamVR_Settings.instance.pauseGameWhenDashboardVisible = true;

			// Makes it so that Dredge appears as a VR game in your Steam Home
			var dredgeFolder = Directory.GetParent(Assembly.GetExecutingAssembly().Location).Parent.Parent.FullName;
			var manifestPath = Path.Combine(dredgeFolder, @"DREDGE_Data\StreamingAssets\dredge.vrmanifest");
			ApplicationManifestHelper.UpdateManifest(manifestPath,
													"steam.app.1562430",
													"https://steamcdn-a.akamaihd.net/steam/apps/1562430/header.jpg",
													"Dredge VR",
													"Dredge VR mod",
													steamBuild: SteamManager.Initialized,
													steamAppId: 1562430);
		}
	}
}