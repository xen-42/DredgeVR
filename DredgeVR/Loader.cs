using DredgeVR.Helpers;
using DredgeVR.Options;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.XR.Management;
using Valve.VR;
using System.Runtime.CompilerServices;

namespace DredgeVR
{
	public class Loader
	{
		// Confuses me that the mod dll counts as the executing assembly
		// Might change if we were using a different mod loader
		// Should update Winch to provide these paths if it doesn't already
		public static string DredgeVRFolder => Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
		
		// Bit hacky but eh
		public static string DredgeFolder => Directory.GetParent(Assembly.GetExecutingAssembly().Location).Parent.Parent.FullName;

		public static void Initialize()
		{
			OptionsManager.Load();

			SetUpXr();

			var gameObject = new GameObject(nameof(DredgeVRCore));
			gameObject.AddComponent<DredgeVRCore>();
			GameObject.DontDestroyOnLoad(gameObject);

			new Harmony("DredgeVR").PatchAll();
		}

		/// <summary>
		/// This runs when the assembly is loaded
		/// </summary>
		public static void Preload()
		{
			FileHelper.Copy(Path.Combine(DredgeVRFolder, "CopyToGame"), Path.Combine(DredgeFolder, "DREDGE_Data"));
		}

		private static void SetUpXr()
		{
			SteamVR_Actions.PreInitialize();

			var generalSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
			var managerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();

			var vrLoader = ScriptableObject.CreateInstance<OpenVRLoader>();

			generalSettings.Manager = managerSettings;
#pragma warning disable CS0618
			/*
			 * ManagerSettings.loaders is deprecated but very useful, allows me to add the xr loader without reflection.
			 * Should be fine unless the game's Unity version gets majorly updated, in which case the whole mod will be
			 * broken, so I'll have to update it anyway.
			 */
			managerSettings.loaders.Add(vrLoader);
#pragma warning restore CS0618

			managerSettings.InitializeLoaderSync();
			if (managerSettings.activeLoader == null) throw new Exception("Cannot initialize OpenVR Loader");

			managerSettings.StartSubsystems();

			// Don't pause because it puts us back on the original camera and kills tracking and becomes turbo sickness
			// SteamVR_Settings.instance.pauseGameWhenDashboardVisible = true;

			// Makes it so that Dredge appears as a VR game in your Steam Home
			var manifestPath = Path.Combine(DredgeFolder, @"DREDGE_Data\StreamingAssets\dredge.vrmanifest");
			ApplicationManifestHelper.UpdateManifest(manifestPath,
													"steam.app.1562430",
													"https://steamcdn-a.akamaihd.net/steam/apps/1562430/header.jpg",
													"Dredge VR",
													"Dredge VR mod",
													steamBuild: SteamManager.Initialized,
													steamAppId: 1562430);

			

			// Improves frames by about 10ms
			SteamVR_Settings.instance.lockPhysicsUpdateRateToRenderFrequency = OptionsManager.Options.lockPhysicsUpdateRateToRenderFrequency;
		}
	}
}