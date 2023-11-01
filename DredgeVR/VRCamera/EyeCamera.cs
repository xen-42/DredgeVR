using DredgeVR.Helpers;
using UnityEngine;
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

	private RenderTexture _eyeTexture;


	public void Awake()
	{
		_displaySubsystem = SteamVRHelper.GetSubSystem<XRDisplaySubsystem>();
		_camera = GetComponent<Camera>();

		// This stops the water shader from getting all weird about the depth buffer being inverted
		// Some day I'd like to fix that, but for now this looks decent enough
		_camera.GetUniversalAdditionalCameraData().requiresDepthOption = CameraOverrideOption.Off;

		GameObject.Destroy(_camera.GetComponent<AntiAliasingSettingResponder>());
	}

	public void LateUpdate()
	{
		_camera.aspect = SteamVR.instance.aspect;
		_camera.fieldOfView = SteamVR.instance.fieldOfView;
		_camera.stereoTargetEye = left ? StereoTargetEyeMask.Left : StereoTargetEyeMask.Right;
		_camera.projectionMatrix = _camera.GetStereoNonJitteredProjectionMatrix(left ? Camera.StereoscopicEye.Left : Camera.StereoscopicEye.Right);
		_eyeTexture = _displaySubsystem.GetRenderTextureForRenderPass(left ? 0 : 1);
		_camera.targetTexture = _eyeTexture;
	}
}
