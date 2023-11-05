using DG.Tweening;
using DG.Tweening.Plugins.Options;
using HarmonyLib;
using UnityEngine;

namespace DredgeVR.VRUI.Minigames;

[HarmonyPatch]
internal class DiamondMinigamePatches
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(DiamondMinigameTarget), nameof(DiamondMinigameTarget.Init))]
	public static bool DiamondMinigameTarget_Init(DiamondMinigameTarget __instance, float timeToHitFullScale, bool isSpecial, float scaleLimit, float angleFrom)
	{
		// From the original method
		__instance.transform.localScale = Vector3.zero;
		__instance.timeToHitFullScale = timeToHitFullScale;
		__instance.isSpecial = isSpecial;
		__instance.scaleLimit = scaleLimit;
		__instance.isInPlay = true;
		__instance.image.color = GameManager.Instance.LanguageManager.GetColor(isSpecial ? DredgeColorTypeEnum.VALUABLE : DredgeColorTypeEnum.POSITIVE);

		// Restart it but in local space
		// Tried killing the tween before but it didnt seem to work
		__instance.transform.DOLocalRotate(Vector3.zero, __instance.timeToHitFullScale, RotateMode.FastBeyond360)
			.From(new Vector3(0, 0, -angleFrom));

		return false;
	}
}
