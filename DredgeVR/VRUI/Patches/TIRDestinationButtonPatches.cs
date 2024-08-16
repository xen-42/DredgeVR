using HarmonyLib;

namespace DredgeVR.VRUI.Patches;

[HarmonyPatch(typeof(TIRDestinationButton))]
public static class TIRDestinationButtonPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(TIRDestinationButton.Init))]
    public static void TIRDestinationButton_Init(TIRDestinationButton __instance)
    {
        __instance.uiLineRenderer.gameObject.SetActive(false);
    }
}
