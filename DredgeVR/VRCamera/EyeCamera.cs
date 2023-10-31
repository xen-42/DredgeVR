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
	private UniversalAdditionalCameraData _data;

	public void Awake()
	{
		_displaySubsystem = SteamVRHelper.GetSubSystem<XRDisplaySubsystem>();
		_camera = GetComponent<Camera>();
		_data = _camera.GetUniversalAdditionalCameraData();

		GameObject.Destroy(_camera.GetComponent<AntiAliasingSettingResponder>());
	}

	public void LateUpdate()
	{
		_camera.aspect = SteamVR.instance.aspect;
		_camera.fieldOfView = SteamVR.instance.fieldOfView;
		_camera.stereoTargetEye = left ? StereoTargetEyeMask.Left : StereoTargetEyeMask.Right;
		_camera.projectionMatrix = _camera.GetStereoNonJitteredProjectionMatrix(left ? Camera.StereoscopicEye.Left : Camera.StereoscopicEye.Right);
		_camera.targetTexture = _displaySubsystem.GetRenderTextureForRenderPass(left ? 0 : 1);

		// Something was setting these back so we set them every frame
		// Else there's weird reflections in the water
		//_data.antialiasing = AntialiasingMode.None;
		//_data.requiresDepthTexture = false;

		//_data.renderPostProcessing = false;

		GL.invertCulling = true;
		_camera.projectionMatrix *= Matrix4x4.Scale(new Vector3(1, -1, 1));
	}
}
