using DredgeVR.Helpers;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;
using Valve.VR;

namespace DredgeVR.VRCamera;

/// <summary>
/// Using the default camera setup results in a lot of things missing in the right eye
/// Some are shader related that can be worked around easily enough
/// However, shadows are just broken in the right eye. Limitation of OpenVR and URP according to Rai
/// Non-directional lights were also broken
/// Doing it this way breaks the depth buffer though (its upsidedown) but I prefer that over having no lights
/// </summary>
[RequireComponent(typeof(Camera))]
public class EyeCamera : MonoBehaviour
{
	public bool left;

	private Camera _camera;
	private XRDisplaySubsystem _displaySubsystem;

	public void Awake()
	{
		_displaySubsystem = SteamVRHelper.GetSubSystem<XRDisplaySubsystem>();
		_camera = GetComponent<Camera>();

		// This stops the water shader from getting all weird about the depth buffer being inverted
		// Some day I'd like to fix that, but for now this looks decent enough
		_camera.GetUniversalAdditionalCameraData().requiresDepthOption = CameraOverrideOption.Off;

		// Everything I've ever read implies that antialiasing causes a slew of problems so let's just avoid all that
		_camera.GetUniversalAdditionalCameraData().antialiasing = AntialiasingMode.None;

		// Have to prevent the game settings from trying to take over anti-aliasing
		GameObject.Destroy(_camera.GetComponent<AntiAliasingSettingResponder>());

		RenderPipelineManager.beginCameraRendering += RenderPipelineManager_beginCameraRendering;
	}

	private void RenderPipelineManager_beginCameraRendering(ScriptableRenderContext context, Camera camera)
	{
		if (camera == _camera)
		{
			SetUpCamera();
		}
	}

	public void SetUpCamera()
	{
		var targetEyeMask = left ? StereoTargetEyeMask.Left : StereoTargetEyeMask.Right;
		var targetEye = left ? Camera.StereoscopicEye.Left : Camera.StereoscopicEye.Right;

		// Magic number that some reddit thread told me was right
		var eyeDistance = 63 / 1000f; //mm

		transform.localPosition = left ? Vector3.left * eyeDistance / 2f : Vector3.right * eyeDistance / 2f;

		_camera.aspect = SteamVR.instance.aspect;
		_camera.fieldOfView = SteamVR.instance.fieldOfView;
		_camera.stereoTargetEye = targetEyeMask;
		_camera.projectionMatrix = _camera.GetStereoNonJitteredProjectionMatrix(targetEye);

		_camera.targetTexture = _displaySubsystem.GetRenderTextureForRenderPass(left ? 0 : 1);
	}
}
