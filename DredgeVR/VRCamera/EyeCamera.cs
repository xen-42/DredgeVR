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
	private UniversalAdditionalCameraData _data;

	private RenderTexture _depthTexture;
	private RenderTexture _eyeTexture;

	private Material _depthMaterial;
	private Material _flipMaterial;
	private Camera _depthCamera;

	public void Awake()
	{
		_displaySubsystem = SteamVRHelper.GetSubSystem<XRDisplaySubsystem>();
		_camera = GetComponent<Camera>();
		_data = _camera.GetUniversalAdditionalCameraData();
		_depthMaterial = new Material(AssetLoader.ShowDepthTexture);
		_flipMaterial = new Material(AssetLoader.FlipYAxisShader);

		/*
		_depthCamera = new GameObject("DepthCamera").SetParent(transform).AddComponent<Camera>();
		_depthCamera.GetUniversalAdditionalCameraData().allowXRRendering = false;
		_depthCamera.GetUniversalAdditionalCameraData().antialiasing = AntialiasingMode.None;
		_depthCamera.clearFlags = CameraClearFlags.Nothing;
		_depthCamera.enabled = false;
		*/

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

		/*
		_depthTexture ??= new RenderTexture(renderTexture);

		_camera.SetTargetBuffers(renderTexture.colorBuffer, renderTexture.depthBuffer);
		*/

		// Something was setting these back so we set them every frame
		// Else there's weird reflections in the water
		_data.antialiasing = AntialiasingMode.None;
		//_data.requiresDepthTexture = false;

		//_data.renderPostProcessing = false;

		GL.invertCulling = true;
		_camera.projectionMatrix *= Matrix4x4.Scale(new Vector3(1, -1, 1));
	}
}
