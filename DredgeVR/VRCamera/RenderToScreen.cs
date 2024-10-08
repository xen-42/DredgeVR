﻿using DredgeVR.Helpers;
using DredgeVR.Options;
using UnityEngine;
using UnityEngine.XR;

namespace DredgeVR.VRCamera;

public class RenderToScreen : MonoBehaviour
{
    private XRDisplaySubsystem _displaySubsystem;
    private Texture _blackScreenTexture;

    public void Awake()
    {
        _displaySubsystem = SteamVRHelper.GetSubSystem<XRDisplaySubsystem>();

        _blackScreenTexture = Texture2D.blackTexture;
    }

    // OnGUI drew a second copy of the textures in world space
    // OnBeginContextRendering drew several copies
    // OnEndContextRendering only drew worldspace 
    // LateUpdate worked then broke randomly somehow
    // Update seems to work but we'll see
    public void Update()
    {
        DrawToScreen();
    }

    private void DrawToScreen()
    {
        // Null makes us render to the screen
        RenderTexture.active = null;
        Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _blackScreenTexture, null, pass: 0);

        if (!OptionsManager.Options.disableMonitor)
        {
            var texture = _displaySubsystem.GetRenderTextureForRenderPass(0);
            if (texture != null)
            {
                // Have to flip it because it's upsidedown
                Graphics.DrawTexture(FitToScreen(Screen.width, Screen.height, texture.width, texture.height), texture, AssetLoader.FlipYAxisMaterial, pass: 0);
            }
        }
    }

    private Rect FitToScreen(float screenWidth, float screenHeight, float textureWidth, float textureHeight, bool fitWidth = true)
    {
        var ratioX = screenWidth / textureWidth;
        var ratioY = screenHeight / textureHeight;

        var ratio = fitWidth || ratioX < ratioY ? ratioX : ratioY;

        var newHeight = textureHeight * ratio;
        var newWidth = textureWidth * ratio;

        var posX = (screenWidth - (textureWidth * ratio)) / 2f;
        var posY = (screenHeight - (textureHeight * ratio)) / 2f;

        return new Rect(posX, posY, newWidth, newHeight);
    }
}
