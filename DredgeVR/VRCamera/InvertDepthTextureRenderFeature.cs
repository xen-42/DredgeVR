using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DredgeVR.VRCamera;

public class InvertDepthTextureRenderFeature : ScriptableRendererFeature
{
	public RenderPassEvent renderPassEvent;
	private InvertDepthTextureRenderPass _renderPass1, _renderPass2;

	public override void Create()
	{
		name = nameof(InvertDepthTextureRenderFeature);
		_renderPass1 = new() { renderPassEvent = RenderPassEvent.BeforeRendering };
		//_renderPass2 = new() { renderPassEvent = RenderPassEvent.AfterRenderingTransparents };
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		renderer.EnqueuePass(_renderPass1);
		//renderer.EnqueuePass(_renderPass2);
	}

	public class InvertDepthTextureRenderPass : ScriptableRenderPass
	{
		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			ref var cameraData = ref renderingData.cameraData;
			var camera = cameraData.camera;

			var cmd = CommandBufferPool.Get("Flip");
			cmd.SetProjectionMatrix(cameraData.GetProjectionMatrix() * Matrix4x4.Scale(new Vector3(1, -1, 1)));
			cmd.SetInvertCulling(false);
			context.ExecuteCommandBuffer(cmd);
		}
	}
}
