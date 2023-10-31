using DredgeVR.Helpers;
using System;
using UnityEngine;
using UnityEngine.XR;

namespace DredgeVR.VRCamera;

public class RenderToScreen : MonoBehaviour
{
	private XRDisplaySubsystem _displaySubsystem;
	private Material _flipYAxisMaterial;

	public void Awake()
	{
		_displaySubsystem = SteamVRHelper.GetSubSystem<XRDisplaySubsystem>();

		// VR render texture is upsidedown so we have to put a material on the texture
		_flipYAxisMaterial = new Material(AssetLoader.FlipYAxisShader);
	}

	public void LateUpdate()
	{
		var texture = _displaySubsystem.GetRenderTextureForRenderPass(0);
		Graphics.DrawTexture(FitToScreen(Screen.width, Screen.height, texture.width, texture.height), texture, _flipYAxisMaterial);
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
