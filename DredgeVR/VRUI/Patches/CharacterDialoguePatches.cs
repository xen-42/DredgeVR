using DredgeVR.Helpers;
using FluffyUnderware.DevTools.Extensions;
using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DredgeVR.VRUI.Patches;

[HarmonyPatch]
public static class CharacterDialoguePatches
{
	private static Camera _dialogueCameraHack;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(DredgeDialogueView), nameof(DredgeDialogueView.ShowPortrait), new Type[] { typeof(SpeakerData) })]
	public static void DredgeDialogueView_ShowPortrait(DredgeDialogueView __instance)
	{
		// Bit hacky
		// Move the background back a bit if it exists
		var background = __instance.characterPortraitContainer.GetComponentsInChildren<Transform>().FirstOrDefault(x => x.name.Contains("Background"));
		if (background != null)
		{
			background.transform.localPosition += Vector3.forward * 50f;
		}

		// Very hacky
		// The UI particles align to face the worldCamera, which is in your hand for raycasting, which looks awful
		// We want them to just be flat in the plane so we make a disabled camera that looks directly at the UI
		if (_dialogueCameraHack == null)
		{
			_dialogueCameraHack = new GameObject("DialogueCameraHack").SetParent(__instance.transform).AddComponent<Camera>();
			_dialogueCameraHack.enabled = false;
			_dialogueCameraHack.transform.localPosition = Vector3.forward;
		}

		CharacterPortraitFixer.Setup();
	}

	public class CharacterPortraitFixer : MonoBehaviour
	{
		private static GameObject _portraitContainer;
		private static Canvas _originalCanvas;

		internal static void Setup()
		{
			_originalCanvas = GameManager.Instance.UI.dialogueView.characterPortraitContainer.GetComponentInChildren<Canvas>().rootCanvas;

			if (_portraitContainer == null)
			{
				_portraitContainer = new GameObject("PortraitContainer").AddComponent<CharacterPortraitFixer>().gameObject;
			}

			GameManager.Instance.UI.dialogueView.characterPortraitContainer.SetParentPreserveLocal(_portraitContainer.transform);
			GameManager.Instance.UI.dialogueView.characterPortraitContainer.transform.localScale = Vector3.one;
		}

		public void Awake()
		{
			// Very hacky
			// The UI particles align to face the worldCamera, which is in your hand for raycasting, which looks awful
			// We want them to just be flat in the plane so we make a disabled camera that looks directly at the UI

			transform.parent = null;

			var dialogueCameraHack = new GameObject("DialogueCameraHack").SetParent(transform).AddComponent<Camera>();
			dialogueCameraHack.transform.LookAt(transform);
			dialogueCameraHack.enabled = false;
			dialogueCameraHack.transform.localPosition = Vector3.forward;

			gameObject.GetAddComponent<CanvasGroup>();
			gameObject.GetAddComponent<GraphicRaycaster>();

			gameObject.GetAddComponent<GameCanvasFixer>();

			var canvas = gameObject.GetAddComponent<Canvas>();
			var name = canvas.name;
			// Had to copy all this else it wouldnt get it right
			canvas.CopyPropertiesFrom(_originalCanvas);
			gameObject.GetComponent<RectTransform>().CopyPropertiesFrom(_originalCanvas.GetComponent<RectTransform>());
			// Don't copy the name
			canvas.name = name;
			canvas.worldCamera = dialogueCameraHack;
		}
	}
}
