using DredgeVR.VRInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DredgeVR.VRUI;

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
	}

	private void OnGameSceneStart()
	{
		// Add a component to orient game UI relative to the player camera
		foreach (var canvas in GameObject.FindObjectsOfType<Canvas>())
		{
			if (canvas.gameObject.scene.name == "Game")
			{
				canvas.gameObject.AddComponent<GameSceneUI>();
			}
		}
	}
}
