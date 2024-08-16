using HarmonyLib;

namespace DredgeVR.VRUI.Minigames;

[HarmonyPatch]
public static class SpiralMinigamePatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SpiralComponent), nameof(SpiralComponent.SetRotation))]
    public static void SpiralComponent_SetRotation(SpiralComponent __instance)
    {
        __instance.transform.localRotation = UnityEngine.Quaternion.Euler(0, 0, __instance.transform.rotation.eulerAngles.z - 90);
    }
}
