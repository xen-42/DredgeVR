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

	private RenderTexture _eyeTexture, _depthTexture;
	private Camera _depthCamera;


	public void Awake()
	{
		_displaySubsystem = SteamVRHelper.GetSubSystem<XRDisplaySubsystem>();
		_camera = GetComponent<Camera>();
		_camera.enabled = false;

		GameObject.Destroy(_camera.GetComponent<AntiAliasingSettingResponder>());

		_depthCamera = new GameObject("DepthCamera").SetParent(transform).AddComponent<Camera>();
		_depthCamera.enabled = false;

		RenderPipelineManager.beginContextRendering += RenderPipelineManager_beginContextRendering;
	}

	private void RenderPipelineManager_beginContextRendering(ScriptableRenderContext arg1, System.Collections.Generic.List<Camera> arg2)
	{
		// Render depth
		GL.invertCulling = true;
		UniversalRenderPipeline.RenderSingleCamera(arg1, _depthCamera);
		GL.invertCulling = false;

		var texture = _eyeTexture;
		Graphics.DrawTexture(RenderToScreen.FitToScreen(Screen.width, Screen.height, texture.width, texture.height), texture, AssetLoader.ShowDepthMaterial);
	}

	public void LateUpdate()
	{
		_camera.aspect = SteamVR.instance.aspect;
		_camera.fieldOfView = SteamVR.instance.fieldOfView;
		_camera.stereoTargetEye = left ? StereoTargetEyeMask.Left : StereoTargetEyeMask.Right;
		_camera.projectionMatrix = _camera.GetStereoNonJitteredProjectionMatrix(left ? Camera.StereoscopicEye.Left : Camera.StereoscopicEye.Right);
		_camera.clearFlags = CameraClearFlags.Depth;
		_camera.depthTextureMode = DepthTextureMode.Depth;
		_eyeTexture ??= new RenderTexture(_displaySubsystem.GetRenderTextureForRenderPass(left ? 0 : 1));
		_camera.targetTexture = _eyeTexture;

		_depthCamera.CopyFrom(_camera);
		_depthTexture ??= new RenderTexture(_eyeTexture);
		_depthCamera.targetTexture = _depthTexture;
		_depthCamera.projectionMatrix = _camera.projectionMatrix * Matrix4x4.Scale(new Vector3(1, -1, 1));
		_depthCamera.GetUniversalAdditionalCameraData().allowXRRendering = false;
		_depthCamera.clearFlags = CameraClearFlags.Depth;
	}
}
