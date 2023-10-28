using DredgeVR.VRInput;
using HarmonyLib;
using UnityEngine;

namespace DredgeVR.VRUI.Patches;

[HarmonyPatch(typeof(CursorProxy))]
public static class CursorProxyPatches
{
	private static Canvas _canvas;

	static CursorProxyPatches()
	{
		DredgeVRCore.SceneStart += OnSceneStart;
	}

	private static void OnSceneStart(string _)
	{
		_canvas = null;
	}

	/// <summary>
	/// Cursor proxy position is used for navigating grid inventory menus
	/// </summary>
	/// <param name="__instance"></param>
	/// <param name="__result"></param>
	/// <returns></returns>
	[HarmonyPostfix]
	[HarmonyPatch(nameof(CursorProxy.Update))]
	public static void CursorProxy_Update(CursorProxy __instance)
	{
		_canvas ??= __instance.GetComponentInParent<Canvas>();
		if (VRInputModule.Instance?.DominantHand?.LaserPointerEnd?.transform?.position is Vector3 laserPointerPos)
		{
			if (VRInputModule.Instance.DominantHand.IsHoveringUI)
			{
				var pixelHalfSize = new Vector3(_canvas.pixelRect.size.x, _canvas.pixelRect.size.y, 0f) / 2f;
				__instance.cursorSquare.transform.position = laserPointerPos;
				var localLaserPosition = __instance.cursorSquare.transform.localPosition;
				__instance.cursorPos = localLaserPosition + pixelHalfSize;
			}
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(CursorProxy.UpdateShouldShowCursor))]
	public static bool CursorProxy_UpdateShouldShowCursor(CursorProxy __instance)
	{
		if (GameManager.Instance.GridManager.IsShowingGrid 
			&& (GameManager.Instance.Input.IsUsingController || GameManager.Instance.GridManager.CurrentlyHeldObject)
			// Modified original method to add this check
			&& (VRInputModule.Instance?.DominantHand?.IsHoveringUI ?? false))
		{
			__instance.Show();
		}
		else
		{
			__instance.Hide();
		}

		return false;
	}
}