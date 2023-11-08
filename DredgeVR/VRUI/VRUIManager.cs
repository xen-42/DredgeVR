using DredgeVR.Helpers;
using DredgeVR.Items;
using DredgeVR.VRInput;
using System;
using System.Linq;
using UnityEngine;
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

	// So that if two things request to hide the UI we wait for both to stop before showing them
	private static int _hideUIRequests;
	public static Action<bool> HeldUIHidden;

	public static Transform LeftHandPromptsContainer { get; private set; }
	public static Transform RightHandPromptsContainer { get; private set; }

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
		// Probably safest to reset this between scenes
		_hideUIRequests = 0;

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

		// Some UI has 0 z size - messes up text
		// Delay for CHFB compat since it makes UI a bit late
		Delay.FireInNUpdates(2, () =>
		{
			foreach (var rectTransform in Resources.FindObjectsOfTypeAll(typeof(RectTransform)).Cast<RectTransform>())
			{
				if (rectTransform.localScale.z == 0)
				{
					rectTransform.localScale = new Vector3(rectTransform.localScale.x, rectTransform.localScale.y, 1);
				}
			}
		});
	}

	private void MakeCanvasWorldSpace(Canvas canvas)
	{
		canvas.renderMode = RenderMode.WorldSpace;
		canvas.worldCamera = VRInputModule.Instance.RaycastCamera;
		canvas.scaleFactor = 1f;
		canvas.planeDistance = 1;
		var canvasScaler = canvas.gameObject.GetComponent<CanvasScaler>();
		if (canvasScaler != null)
		{
			Destroy(canvasScaler);
			// Canvas scaler already scaled it before it was world space unfortunately
			canvas.transform.localScale = Vector3.one;
			// Also repositions stuff
		}
	}

	private void OnTitleSceneStart()
	{
		// Delay for CHFB compat since it makes UI a bit late
		Delay.FireInNUpdates(2, () =>
		{
			// We place the title screen canvas on the beach
			var canvas = GameObject.Find("Canvases");

			canvas.transform.position = new Vector3(-5.3f, 0.45f, 2f);
			canvas.transform.rotation = Quaternion.Euler(0, 70, 0);
			canvas.transform.localScale = Vector3.one * 0.002f;

			// Remove controls tab for now since it doesnt work
			RemoveControlsTab(GameObject.Find("Canvases/SettingsDialog/TabbedPanelContainer").GetComponent<TabbedPanelContainer>());

			// These canvases are on the Manager scene and will persist the way they are
			if (!_hasInitialized)
			{
				InitializeManagerScene();
			}

			// Splash screen scene is still around and the canvas is there but invisible blocking our laser pointer
			GameObject.FindObjectsOfType<Canvas>().FirstOrDefault(x => x.gameObject.scene.name == "Splash")?.gameObject?.SetActive(false);
		});
	}

	private void InitializeManagerScene()
	{
		// Make the loading screen UI show in front of the player too
		GameObject.Find("Canvas").AddComponent<GameCanvasFixer>();

		// Create game objects for the left/right hand button prompts
		var controlPromptPanel = GameObject.Find("Canvas/ControlPromptPanel").transform;
		var leftHandPrompts = CreatePromptContainer("LeftHand", controlPromptPanel);
		var rightHandPrompts = CreatePromptContainer("RightHand", controlPromptPanel);

		leftHandPrompts.AddComponent<UIHandAttachment>().Init(false, new Vector3(0, 90, 45), new Vector3(0.3f, 0.05f, 0f), 1f);
		rightHandPrompts.AddComponent<UIHandAttachment>().Init(true, new Vector3(0, 90, 45), new Vector3(0.2f, 0.05f, 0f), 1f);

		LeftHandPromptsContainer = leftHandPrompts.transform.Find("Container");
		RightHandPromptsContainer = rightHandPrompts.transform.Find("Container");

		GameObject.FindObjectOfType<LoadingScreen>().gameObject.AddComponent<VRLoadingScene>();

		_hasInitialized = true;
	}

	private GameObject CreatePromptContainer(string name, Transform parent)
	{
		var obj = new GameObject(name);
		obj.transform.parent = parent;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.identity;

		var container = new GameObject("Container");
		container.transform.parent = obj.transform;
		container.transform.localPosition = Vector3.zero;
		container.transform.localRotation = Quaternion.identity;

		return obj;
	}

	private void OnGameSceneStart()
	{
		// Add a component to orient game UI relative to the player camera
		foreach (var canvas in GameObject.FindObjectsOfType<Canvas>())
		{
			// When finding objects of type make sure they are in the game scene, could be on Manager or DontDestroyOnLoad or whatever
			// Also ignore canvases that have parent canvases, since they will inherit their position
			if (canvas.gameObject.scene.name == "Game" && canvas.isRootCanvas)
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
		// TODO: Split ability select UI from do ability UI
		GameObject.Find("GameCanvases/GameCanvas/Abilities/ActiveAbility").AddComponent<UIHandAttachment>()
			.Init(GameManager.Instance.Input.Controls.RadialSelectShow.GetHand() == "right", new Vector3(0, 90, 45), new Vector3(0.1f, 0.1f, 0f), 1f);

		// Attach inventory tab UI to hand
		GameObject.Find("GameCanvases/GameCanvas/PlayerSlidePanel/SlidePanelTab").AddComponent<UIHandAttachment>()
			.Init(GameManager.Instance.Input.Controls.ToggleCargo.GetHand() == "right", new Vector3(0, 90, 45), new Vector3(0.1f, 0.05f, 0f), 1f);

		// Remove scrims
		GameObject.Find("GameCanvases/PopupCanvas/QuestWindow/Container/Scrim").SetActive(false);
		GameObject.Find("GameCanvases/GameCanvas/UpgradeWindow/Container/Scrim").SetActive(false);
		GameObject.Find("GameCanvases/GameCanvas/UpgradeWindow").transform.localPosition += Vector3.forward * 50f;

		GameObject.Find("GameCanvases/PopupCanvas/QuestDetailWindow").AddComponent<HeldUI>().SetOffset(450, 50);
		GameObject.Find("GameCanvases/PopupCanvas/QuestDetailWindow/Container/Scrim").SetActive(false);

		GameObject.FindObjectOfType<CompassUI>().gameObject.AddComponent<HeldCompass>();

		// Remove controls tab since it doesn't work in UI
		RemoveControlsTab(GameObject.Find("GameCanvases/SettingsDialog/TabbedPanelContainer").GetComponent<TabbedPanelContainer>());
		GameObject.Find("GameCanvases/SettingsDialog/Scrim").transform.localScale = Vector3.zero; // Other things were reactivating it so hacky it is
		GameObject.Find("GameCanvases/SettingsDialog/TabbedPanelContainer").transform.localPosition = Vector3.forward * -100f;
		GameObject.Find("GameCanvases/SettingsDialog/PopupDialogContainer").transform.localPosition = Vector3.forward * -200f;

		// Reposition some character dialogue stuff for fun
		var dialogueContainer = GameObject.Find("GameCanvases/GameCanvas/DialogueView/Container/DialogueTextContainer").transform;
		var nameContainer = GameObject.Find("GameCanvases/GameCanvas/DialogueView/Container/CharacterNameContainer").transform;

		// Remove time pass scrim
		GameObject.Find("GameCanvases/TimePassCanvas/Container/Scrim").SetActive(false);
		GameObject.Find("GameCanvases/TimePassCanvas/Container").transform.localPosition = Vector3.forward * -100f;

		// Remove encyclopedia scrim
		GameObject.Find("GameCanvases/PopupCanvas/EncyclopediaWindow/Container/Scrim").SetActive(false);

		var container = new GameObject("Container").transform;
		container.parent = dialogueContainer.parent;
		container.localPosition = dialogueContainer.localPosition;
		dialogueContainer.parent = container;
		nameContainer.parent = container;
		container.localPosition += Vector3.back * 400f;
		container.localRotation = Quaternion.Euler(45f, 0f, 0f);

		var optionsContainer = GameObject.Find("GameCanvases/GameCanvas/DialogueView/Container/OptionsContainer").transform;
		optionsContainer.localPosition += Vector3.back * 240f;
		optionsContainer.localRotation = Quaternion.Euler(0f, 20f, 0f);
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

			// Disable the post processing
			GameObject.Find("IntroCutscene/PostProcessing").gameObject.SetActive(false);
		}
		catch (Exception e)
		{
			DredgeVRLogger.Error($"Couldn't set up intro cutscene ui: {e}");
		}
	}

	private void RemoveControlsTab(TabbedPanelContainer tabbedPanelContainer)
	{
		var controlTab = tabbedPanelContainer.transform.Find("TopBar/Tabs/ControlTab").GetComponent<TabUI>();
		tabbedPanelContainer.tabbedPanels.Remove(tabbedPanelContainer.tabbedPanels.First(x => x.tab == controlTab));
		tabbedPanelContainer.showablePanelIndexes.RemoveAt(tabbedPanelContainer.showablePanelIndexes.Count() - 1);
		controlTab.gameObject.SetActive(false);
	}

	public static void HideHeldUI()
	{
		if (_hideUIRequests == 0)
		{
			HeldUIHidden?.Invoke(true);
		}
		_hideUIRequests++;
	}

	public static void ShowHeldUI()
	{
		_hideUIRequests--;
		if (_hideUIRequests == 0)
		{
			HeldUIHidden?.Invoke(false);
		}
	}
}
