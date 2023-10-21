﻿using DredgeVR.Helpers;
using DredgeVR.VRCamera;
using DredgeVR.VRInput;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;

namespace DredgeVR.VRUI;

/// <summary>
/// This behaviour is responsible for applying all VR ui fixes on each game scene
/// Either by directly modifying components or adding fixers to them
/// </summary>
internal class VRUIManager : MonoBehaviour
{
	private bool _hasInitialized;

	public void Awake()
	{
		SceneManager.activeSceneChanged += OnActiveSceneChanged;
		DredgeVRCore.TitleSceneStart += OnTitleSceneStart;
		DredgeVRCore.GameSceneStart += OnGameSceneStart;
		DredgeVRCore.IntroCutsceneStart += OnIntroCutsceneStart;
	}

	public void OnDestroy()
	{
		SceneManager.activeSceneChanged -= OnActiveSceneChanged;
		DredgeVRCore.TitleSceneStart -= OnTitleSceneStart;
		DredgeVRCore.GameSceneStart -= OnGameSceneStart;
		DredgeVRCore.IntroCutsceneStart -= OnIntroCutsceneStart;
	}

	private void OnActiveSceneChanged(Scene prev, Scene current)
	{
		foreach (var canvas in GameObject.FindObjectsOfType<Canvas>())
		{
			canvas.renderMode = RenderMode.WorldSpace;
			// TODO: If the dominant hand is changed while ingame, we'd have to reset this on all canvases
			canvas.worldCamera = VRInputModule.Instance.RaycastCamera;
			canvas.scaleFactor = 1f;
			canvas.planeDistance = 1;
		}
	}

	private void OnTitleSceneStart()
	{
		// We place the title screen canvas on the beach
		var canvas = GameObject.Find("Canvases");

		canvas.transform.position = new Vector3(-5.4f, 0.35f, 1.6f);
		canvas.transform.rotation = Quaternion.Euler(0, 70, 0);
		canvas.transform.localScale = Vector3.one * 0.002f;

		// These canvases are on the Manager scene and will persist the way they are
		if (!_hasInitialized)
		{
			// Make the loading screen UI show in front of the player too
			GameObject.Find("Canvas").AddComponent<GameCanvasFixer>();

			// Attach right hand button prompts
			GameObject.Find("Canvas/ControlPromptPanel").AddComponent<UIHandAttachment>()
				.Init(SteamVR_Input_Sources.RightHand, new Vector3(0, 90, 45), new Vector3(0.1f, 0.2f, -0.2f), 1f);

			_hasInitialized = true;
		}
	}

	private void OnGameSceneStart()
	{
		// Add a component to orient game UI relative to the player camera
		foreach (var canvas in GameObject.FindObjectsOfType<Canvas>())
		{
			// When finding objects of type make sure they are in the game scene, could be on Manager or DontDestroyOnLoad or whatever
			// Also ignore canvases that have parent canvases, since they will inherit their position
			if (canvas.gameObject.scene.name == "Game" && canvas.transform.parent?.GetComponentInParent<Canvas>() == null)
			{
				canvas.gameObject.AddComponent<GameCanvasFixer>();
			}
		}

		// The slide panel still shows when "off screen"
		GameObject.Find("GameCanvases/GameCanvas/PlayerSlidePanel").AddComponent<SlidePanelFixer>().targets = new string[] { "Funds", "Backplate" };
		GameObject.Find("GameCanvases/GameCanvas/DestinationUI/MarketDestinationUI/MarketSlidePanel").AddComponent<SlidePanelFixer>().targets = new string[] { "TitleContainer", "Backplate" };

		// Make it easier to target
		GameObject.Find("GameCanvases/GameCanvas/DockUI/SpeakersContainer").transform.localScale = Vector3.one * 1.5f;

		// Attach activity UI to hand
		GameObject.Find("GameCanvases/GameCanvas/Abilities/ActiveAbility").AddComponent<UIHandAttachment>()
			.Init(SteamVR_Input_Sources.LeftHand, new Vector3(0, 90, 45), new Vector3(0.1f, 0.2f, -0.2f), 1f);

		// Attach inventory tab UI to hand
		GameObject.Find("GameCanvases/GameCanvas/PlayerSlidePanel/SlidePanelTab").AddComponent<UIHandAttachment>()
			.Init(SteamVR_Input_Sources.LeftHand, new Vector3(0, 90, 45), new Vector3(0.1f, 0.2f, -0.2f), 1f);

		// Cabin slide panel is wrong
		var itemScroller = GameObject.Find("GameCanvases/GameCanvas/PlayerSlidePanel/PlayerTabbedPanelContainer/Panels/CabinPanel/Container/ItemScroller/NonSpatialItemGrid").transform;
		itemScroller.localPosition = new Vector3(itemScroller.localPosition.x, itemScroller.localPosition.y, 0f);
	}

	private void OnIntroCutsceneStart()
	{
		// Reposition Scene1Container, Scene2Container, Scene3Container
		var cutscene = GameObject.FindObjectOfType<IntroIllustratedCutscene>();

		cutscene.transform.Find("Camera").gameObject.SetActive(false);

		cutscene.transform.position = VRCameraManager.AnchorTransform.position + VRCameraManager.AnchorTransform.forward * 40f - VRCameraManager.AnchorTransform.up * 10f;
		cutscene.transform.rotation = Quaternion.Euler(0, VRCameraManager.AnchorTransform.rotation.y, 0);

		var scenes = new Transform[] { cutscene.transform.Find("Scene1Container"), cutscene.transform.Find("Scene2Container"), cutscene.transform.Find("Scene3Container") };
		foreach (var scene in scenes)
		{
			scene.transform.localPosition = Vector3.zero;
		}
	}
}
