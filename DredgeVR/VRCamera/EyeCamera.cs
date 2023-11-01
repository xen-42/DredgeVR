using DredgeVR.Helpers;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;
using Valve.VR;

namespace DredgeVR.VRCamera;

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
		_camera.GetUniversalAdditionalCameraData().antialiasing = AntialiasingMode.None;

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
		_camera.aspect = SteamVR.instance.aspect;
		_camera.fieldOfView = SteamVR.instance.fieldOfView;
		_camera.stereoTargetEye = left ? StereoTargetEyeMask.Left : StereoTargetEyeMask.Right;
		_camera.projectionMatrix = _camera.GetStereoProjectionMatrix(left ? Camera.StereoscopicEye.Left : Camera.StereoscopicEye.Right);
		_camera.targetTexture = _displaySubsystem.GetRenderTextureForRenderPass(left ? 0 : 1);
	}
}
