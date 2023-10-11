using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;
using Winch.Core;

namespace DredgeVR
{
	public class DredgeVRCore : MonoBehaviour
	{
		public static XRDisplaySubsystem VRDisplay { get; private set; }

		public static Camera LeftCamera { get; private set; }
		public static Camera RightCamera { get; private set; }

		public static DredgeVRCore Instance { get; private set; }

		public void Awake()
		{
			Instance = this;
			WinchCore.Log.Debug($"{nameof(DredgeVRCore)} has loaded!");
		}

		public void Start()
		{
			var displays = new List<XRDisplaySubsystem>();
			SubsystemManager.GetInstances(displays);
			VRDisplay = displays[0];

			/*
			var vrCamera = new GameObject("VRCamera");
			vrCamera.AddComponent<SteamVR_TrackedObject>();

			var leftCamera = new GameObject("LeftCamera");
			LeftCamera = leftCamera.AddComponent<Camera>();
			leftCamera.transform.parent = vrCamera.transform;

			var rightCamera = new GameObject("RightCamera");
			RightCamera = rightCamera.AddComponent<Camera>();
			rightCamera.transform.parent = vrCamera.transform;
			*/

			LeftCamera = Camera.main;
			var rightCamera = new GameObject("SecondCamera");
			RightCamera = rightCamera.AddComponent<Camera>();
			RightCamera.CopyFrom(LeftCamera);
			rightCamera.transform.parent = LeftCamera.transform;

			//LeftCamera.gameObject.AddComponent<SteamVR_TrackedObject>();
			//RightCamera.gameObject.AddComponent<SteamVR_TrackedObject>();
		}

		public void Update()
		{
			LeftCamera.fieldOfView = SteamVR.instance.fieldOfView;
			LeftCamera.stereoTargetEye = StereoTargetEyeMask.Left;
			LeftCamera.projectionMatrix = LeftCamera.GetStereoNonJitteredProjectionMatrix(Camera.StereoscopicEye.Left);
			LeftCamera.targetTexture = VRDisplay.GetRenderTextureForRenderPass(0);
			LeftCamera.enabled = true;

			RightCamera.fieldOfView = SteamVR.instance.fieldOfView;
			RightCamera.stereoTargetEye = StereoTargetEyeMask.Right;
			RightCamera.projectionMatrix = RightCamera.GetStereoNonJitteredProjectionMatrix(Camera.StereoscopicEye.Right);
			RightCamera.targetTexture = VRDisplay.GetRenderTextureForRenderPass(1);
			RightCamera.enabled = true;
		}
	}
}
