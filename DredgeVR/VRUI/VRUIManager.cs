using DredgeVR.Helpers;
using DredgeVR.VRCamera;
using DredgeVR.VRInput;
using FluffyUnderware.DevTools.Extensions;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
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
		DredgeVRCore.SceneStart += OnSceneStart;
		DredgeVRCore.TitleSceneStart += OnTitleSceneStart;
		DredgeVRCore.GameSceneStart += OnGameSceneStart;
		DredgeVRCore.IntroCutsceneStart += OnIntroCutsceneStart;

		VRInputModule.Instance.DominantHandChanged += OnDominantHandChanged;
	}

	public void OnDestroy()
	{
		DredgeVRCore.SceneStart -= OnSceneStart;
		DredgeVRCore.TitleSceneStart -= OnTitleSceneStart;
		DredgeVRCore.GameSceneStart -= OnGameSceneStart;
		DredgeVRCore.IntroCutsceneStart -= OnIntroCutsceneStart;

		VRInputModule.Instance.DominantHandChanged -= OnDominantHandChanged;
	}

	private void OnDominantHandChanged(SteamVR_Input_Sources _)
	{
		foreach (var canvas in GameObject.FindObjectsOfType<Canvas>())
		{
			canvas.worldCamera = VRInputModule.Instance.RaycastCamera;
		}
	}

	private void OnSceneStart(string scene)
	{
		// On new save files theres a prompt they need to press here
		if (scene == "Startup")
		{
			GameObject.FindObjectsOfType<Canvas>().FirstOrDefault(x => x.name == "Canvas" && x.gameObject.scene.name == "Startup").gameObject.AddComponent<GameCanvasFixer>();
		}

		if (scene == "Splash")
		{
			var splashScreen = GameObject.FindObjectOfType<SplashController>();
			MakeCanvasWorldSpace(splashScreen.GetComponent<Canvas>());
			splashScreen.gameObject.AddComponent<GameCanvasFixer>();

			// Fixes it displaying directly on your far clip plane
			var videoImage = splashScreen.gameObject.AddComponent<RawImage>();
			videoImage.texture = new RenderTexture(1920, 1080, 1);
			splashScreen.t17Video.aspectRatio = UnityEngine.Video.VideoAspectRatio.FitVertically;
			splashScreen.t17Video.renderMode = UnityEngine.Video.VideoRenderMode.RenderTexture;
			splashScreen.t17Video.targetTexture = (RenderTexture)videoImage.texture;
		}
		// If we do it on the splash screen we break unity explorer
		else if (scene != "IntroCutscene")
		{
			foreach (var canvas in GameObject.FindObjectsOfType<Canvas>())
			{
				MakeCanvasWorldSpace(canvas);
			}
		}
	}

	private void MakeCanvasWorldSpace(Canvas canvas)
	{
		canvas.renderMode = RenderMode.WorldSpace;
		canvas.worldCamera = VRInputModule.Instance.RaycastCamera;
		canvas.scaleFactor = 1f;
		canvas.planeDistance = 1;
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
				.Init(true, new Vector3(0, 90, 45), new Vector3(0.1f, 0.2f, -0.2f), 1f);

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
			.Init(false, new Vector3(0, 90, 45), new Vector3(0.1f, 0.2f, -0.2f), 1f);

		// Attach inventory tab UI to hand
		GameObject.Find("GameCanvases/GameCanvas/PlayerSlidePanel/SlidePanelTab").AddComponent<UIHandAttachment>()
			.Init(false, new Vector3(0, 90, 45), new Vector3(0.1f, 0.2f, -0.2f), 1f);

		// Cabin slide panel is wrong
		var itemScroller = GameObject.Find("GameCanvases/GameCanvas/PlayerSlidePanel/PlayerTabbedPanelContainer/Panels/CabinPanel/Container/ItemScroller/NonSpatialItemGrid").transform;
		itemScroller.localPosition = new Vector3(itemScroller.localPosition.x, itemScroller.localPosition.y, 0f);
	}

	private void OnIntroCutsceneStart()
	{
		try
		{
			var cutsceneRenderer = new GameObject("CutsceneRenderTexture").gameObject.AddComponent<RawImage>();
			cutsceneRenderer.texture = new RenderTexture(1920, 1080, 1);
			var cutsceneRendererCanvas = cutsceneRenderer.gameObject.AddComponent<Canvas>();
			cutsceneRendererCanvas.scaleFactor = 5f;
			MakeCanvasWorldSpace(cutsceneRendererCanvas);
			cutsceneRenderer.transform.position = new Vector3(0, 1, 2);
			cutsceneRenderer.transform.localScale = Vector3.one * 0.02f;

			var cutscene = GameObject.FindObjectOfType<IntroIllustratedCutscene>();
			cutscene.transform.position = new Vector3(0, 0, -5000);
			var camera = cutscene.transform.Find("Camera").gameObject.AddComponent<Camera>();

			camera.targetTexture = (RenderTexture)cutsceneRenderer.texture;
		}
		catch (Exception e)
		{
			DredgeVRLogger.Error($"Couldn't set up intro cutscene ui: {e}");
		}
	}
}
