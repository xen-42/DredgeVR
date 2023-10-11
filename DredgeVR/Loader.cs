using HarmonyLib;
using System;
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
			new Harmony("DredgeVR").PatchAll();

			var gameObject = new GameObject(nameof(DredgeVRCore));
			gameObject.AddComponent<DredgeVRCore>();
			GameObject.DontDestroyOnLoad(gameObject);

			SetUpXr();
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
		}
	}
}