using DredgeVR.Helpers;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SocialPlatforms;
using UnityEngine.XR;
using Valve.VR;

namespace DredgeVR.VRCamera;

[RequireComponent(typeof(Camera))]
public class EyeCamera : MonoBehaviour
{
	public bool LeftEye;

	private Camera _camera;
	private XRDisplaySubsystem _displaySubsystem;

	private RenderTexture _leftEyeTexture, _rightEyeTexture;

	public void Awake()
	{
		_displaySubsystem = SteamVRHelper.GetSubSystem<XRDisplaySubsystem>();
		_camera = GetComponent<Camera>();

		GameObject.Destroy(_camera.GetComponent<AntiAliasingSettingResponder>());

		RenderPipelineManager.beginCameraRendering += RenderPipelineManager_beginCameraRendering;
		RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
	}

	private void RenderPipelineManager_beginCameraRendering(ScriptableRenderContext arg1, Camera arg2)
	{
		if (arg2 != _camera) return;

		if (ignore)
		{
			DredgeVRLogger.Debug("it acxtually called render!");
			return;
		}

		SetEye(false);
	}

	private bool ignore;

	private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext context, Camera camera)
	{
		if (camera != _camera) return;

		if (ignore) return;

		var textureA = _displaySubsystem.GetRenderTextureForRenderPass(0);
		_rightEyeTexture ??= new(textureA);
		var textureB = _displaySubsystem.GetRenderTextureForRenderPass(1);

		CommandBuffer cmd = new CommandBuffer();
		cmd.Blit(textureA, _rightEyeTexture);
		context.ExecuteCommandBuffer(cmd);
		cmd.Release();

		SetEye(true);
		ignore = true;
		UniversalRenderPipeline.RenderSingleCamera(context, _camera);
		ignore = false;

		cmd = new CommandBuffer();
		cmd.Blit(_rightEyeTexture, textureB);
		context.ExecuteCommandBuffer(cmd);
		cmd.Release();
	}

	private void SetEye(bool left)
	{
		_camera.aspect = SteamVR.instance.aspect;
		_camera.fieldOfView = SteamVR.instance.fieldOfView;
		_camera.stereoTargetEye = left ? StereoTargetEyeMask.Left : StereoTargetEyeMask.Right;
		_camera.projectionMatrix = _camera.GetStereoNonJitteredProjectionMatrix(left ? Camera.StereoscopicEye.Left : Camera.StereoscopicEye.Right);
		//_eyeTexture ??= new RenderTexture(_displaySubsystem.GetRenderTextureForRenderPass(left ? 0 : 1));
		//_camera.targetTexture = _eyeTexture;
	}
}
