using Cinemachine.Utility;
using DredgeVR.Helpers;
using DredgeVR.Options;
using DredgeVR.VRCamera;
using HarmonyLib;
using UnityEngine;

namespace DredgeVR.TitleScreen;

[HarmonyPatch]
public static class TitleScreenPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(TitleScreenView), nameof(TitleScreenView.SetNoDLC))]
    public static void TitleScreenView_SetNoDLC()
    {
        DredgeVRLogger.Info("Default title screen");

        // Make the player look towards the lighthouse
        var lightHouse = GameObject.Find("TheMarrows/Islands/LittleMarrow").transform;
        var camLookAt = new Vector3(lightHouse.position.x, 0.5f, lightHouse.position.z);
        var camPos = new Vector3(-6.5f, 0.5f, 0);

        SetUpTitleScreen(camPos, camLookAt);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TitleScreenView), nameof(TitleScreenView.SetDLC1))]
    public static void TitleScreenView_SetDLC1(TitleScreenView __instance)
    {
        DredgeVRLogger.Info("DLC 1 title screen");

        var camLookAt = __instance.dlc1LookAt.transform.position.ProjectOntoPlane(Vector3.up);
        var camPos = new Vector3(-90.6f, 1f, -1337.3f);

        SetUpTitleScreen(camPos, camLookAt);

        // Move snow particles to the anchor position
        GameObject.Find("VCam/Snow").transform.position = VRCameraManager.AnchorTransform.position;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TitleScreenView), nameof(TitleScreenView.SetDLC2))]
    public static void TitleScreenView_SetDLC2(TitleScreenView __instance)
    {
        DredgeVRLogger.Info("DLC 2 title screen");

        var camLookAt = __instance.dlc2LookAt.transform.position.ProjectOntoPlane(Vector3.up);
        var camPos = __instance.dlc2Camera.position;

        SetUpTitleScreen(camPos, camLookAt);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TitleScreenView), nameof(TitleScreenView.SetAllDLC))]
    public static void TitleScreenView_SetAllDLC(TitleScreenView __instance)
    {
        DredgeVRLogger.Info("All DLC title screen");

        var camLookAt = new Vector3(86.9563f, 2f, -119.7114f);
        var camPos = new Vector3(80.7999f, 2f, -115.8387f);

        SetUpTitleScreen(camPos, camLookAt);
    }

    private static void SetUpTitleScreen(Vector3 playerPos, Vector3 lookAtPos)
    {
        // Never want to tilt a VR camera
        lookAtPos.y = playerPos.y;

        VRCameraManager.AnchorTransform.position = playerPos;
        VRCameraManager.AnchorTransform.LookAt(lookAtPos);

        var canvas = GameObject.Find("Canvases");

        canvas.transform.localScale = Vector3.one * 0.002f * OptionsManager.Options.playerScale;

        var lookAtDir = (lookAtPos - playerPos).ProjectOntoPlane(Vector3.up).normalized;
        var canvasPos = playerPos + (lookAtDir * 1.5f) + (lookAtDir * (OptionsManager.Options.playerScale - 1f));
        canvasPos.y = playerPos.y + 1f * (OptionsManager.Options.playerScale);

        canvas.transform.position = canvasPos;

        foreach (Transform child in canvas.transform)
        {
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
        }

        var angle = Quaternion.FromToRotation(Vector3.forward, lookAtDir);
        canvas.transform.rotation = angle;
    }
}
