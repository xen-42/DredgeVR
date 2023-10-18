using DredgeVR.VRInput;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DredgeVR.VRUI;

/// <summary>
/// This behaviour is responsible for applying all VR ui fixes on each game scene
/// Either by directly modifying components or adding fixers to them
/// </summary>
internal class VRUIManager : MonoBehaviour
{
	public void Awake()
	{
		SceneManager.activeSceneChanged += OnActiveSceneChanged;
		DredgeVRCore.TitleSceneStart += OnTitleSceneStart;
		DredgeVRCore.GameSceneStart += OnGameSceneStart;
	}

	public void OnDestroy()
	{
		SceneManager.activeSceneChanged -= OnActiveSceneChanged;
		DredgeVRCore.TitleSceneStart -= OnTitleSceneStart;
		DredgeVRCore.GameSceneStart -= OnGameSceneStart;
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

		// Make the loading screen UI show in front of the player too
		GameObject.Find("Canvas").AddComponent<GameCanvasFixer>();

		// Make these a bit easier to target
		GameObject.Find("Canvas/ControlPromptPanel").transform.localScale = Vector3.one * 1.5f;
		GameObject.Find("Canvas/ControlPromptPanel").transform.localScale = Vector3.one * 1.5f;
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

		// Make the tab button easier to target
		GameObject.Find("GameCanvases/GameCanvas/PlayerSlidePanel/SlidePanelTab").transform.localScale = Vector3.one * 1.5f;

		// Make it easier to target
		GameObject.Find("GameCanvases/GameCanvas/DockUI/SpeakersContainer").transform.localScale = Vector3.one * 1.5f;
	}
}
