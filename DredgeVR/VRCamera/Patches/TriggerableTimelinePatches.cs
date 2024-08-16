using Cinemachine;
using Cinemachine.Utility;
using DredgeVR.VRUI;
using HarmonyLib;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace DredgeVR.VRCamera.Patches;

[HarmonyPatch(typeof(TriggerableTimeline))]
public static class TriggerableTimelinePatches
{
	private static bool _playing;
	private static CinemachineVirtualCamera _camera;
	private static CinemachineVirtualCamera[] _cameras;

	[HarmonyPostfix]
	[HarmonyPatch(nameof(TriggerableTimeline.Play))]
	public static void TriggerableTimeline_Play(TriggerableTimeline __instance)
	{
		_playing = true;
		_camera = null;
		_cameras = __instance.GetComponentsInChildren<CinemachineVirtualCamera>(true);

		VRCameraManager.Instance.StartCoroutine(TriggerableTimelineLogic(__instance));
	}

	[HarmonyPostfix]
	[HarmonyPatch(nameof(TriggerableTimeline.OnTimelineComplete))]
	public static void TriggerableTimeline_OnTimelineComplete(TriggerableTimeline __instance)
	{
		_playing = false;
	}

	private static IEnumerator TriggerableTimelineLogic(TriggerableTimeline __instance)
	{
		VRUIManager.HideHeldUI();
		VRCameraManager.Instance.InCutscene = true;

		while (_playing)
		{
			var prevCamera = _camera;
		
			if (_camera == null || !_camera.gameObject.activeInHierarchy)
			{
				// Try to update the camera to an active one
				_camera = _cameras.FirstOrDefault(x => x.gameObject.activeInHierarchy);
				if (prevCamera != _camera)
				{
					if (_camera != null)
					{
						// When the camera changes we fade out the screen first
						yield return ShowLoadingScreen(true);

						// Camera exists, move to it
						VRCameraManager.AnchorTransform.transform.position = _camera.transform.position;
						VRCameraManager.AnchorTransform.transform.LookAt(_camera.transform.position + _camera.transform.forward.ProjectOntoPlane(Vector3.up));

						yield return ShowLoadingScreen(false);
					}
				}
			}

			yield return new WaitForSeconds(0.1f);
		}

		yield return ShowLoadingScreen(true);

		VRCameraManager.Instance.InCutscene = false;

		VRCameraManager.Instance.ResetAnchorToBoat();

		yield return ShowLoadingScreen(false);

		VRUIManager.ShowHeldUI();
	}

	private static bool _showingLoadingScreen;
	private static YieldInstruction ShowLoadingScreen(bool show)
	{
		if (show != _showingLoadingScreen)
		{
			_showingLoadingScreen = show;
			GameManager.Instance.Loader.loadingScreen.Fade(show, true);
			return new WaitForSeconds(1f);
		}
		else
		{
			return new WaitForEndOfFrame();
		}
	}
}
