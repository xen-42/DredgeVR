using DredgeVR.Helpers;
using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DredgeVR.VRUI.Patches;

[HarmonyPatch]
public static class CharacterDialoguePatches
{
	private static GameObject _portraitContainer;
	private static Camera _dialogueCameraHack;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(DredgeDialogueView), nameof(DredgeDialogueView.ShowPortrait), new Type[] { typeof(SpeakerData) })]
	public static void DredgeDialogueView_ShowPortrait(DredgeDialogueView __instance)
	{
		// Bit hacky
		// Move the background back a bit if it exists
		var background = __instance.characterPortraitContainer.GetComponentsInChildren<Transform>().FirstOrDefault(x => x.name.Contains("Background"));
		// Show portrait can be called when it is already showing which then moves it backwards multiple times if we dont check
		if (background != null && background.transform.localPosition.z == 0f)
		{
			background.transform.localPosition += Vector3.forward * 50f;
		}

		// Very hacky
		// The UI particles align to face the worldCamera, which is in your hand for raycasting, which looks awful
		// We want them to just be flat in the plane so we make a disabled camera that looks directly at the UI
		if (_portraitContainer == null)
		{
			_portraitContainer = new GameObject("PortraitContainer");

			_dialogueCameraHack = new GameObject("DialogueCameraHack").SetParent(_portraitContainer.transform).AddComponent<Camera>();
			_dialogueCameraHack.enabled = false;
			_dialogueCameraHack.transform.localPosition = -Vector3.forward;
			_dialogueCameraHack.transform.LookAt(_portraitContainer.transform);

			_portraitContainer.GetAddComponent<CanvasGroup>();
			_portraitContainer.GetAddComponent<GraphicRaycaster>();
			_portraitContainer.GetAddComponent<GameCanvasFixer>();

			var canvas = _portraitContainer.GetAddComponent<Canvas>();
			var name = canvas.name;
			// Had to copy all this else it wouldnt get it right
			var parentCanvas = GameManager.Instance.UI.dialogueView.characterPortraitContainer.GetComponentInParent<Canvas>();
			var rootCanvas = parentCanvas.isRootCanvas ? parentCanvas : parentCanvas.rootCanvas;
			canvas.CopyPropertiesFrom(rootCanvas);
			_portraitContainer.GetComponent<RectTransform>().CopyPropertiesFrom(rootCanvas.GetComponent<RectTransform>());
			// Don't copy the name
			canvas.name = name;
			canvas.worldCamera = _dialogueCameraHack;

			_portraitContainer.transform.parent = rootCanvas.transform.parent;
		}

		GameManager.Instance.UI.dialogueView.characterPortraitContainer.SetParentPreserveLocal(_portraitContainer.transform);
		GameManager.Instance.UI.dialogueView.characterPortraitContainer.transform.localScale = Vector3.one;
	}
}
