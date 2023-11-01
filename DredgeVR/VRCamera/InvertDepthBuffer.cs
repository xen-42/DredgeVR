using DredgeVR.Helpers;
using FluffyUnderware.DevTools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DredgeVR.VRCamera;

public class InvertDepthBuffer : MonoBehaviour
{
	public void Awake()
	{
		// Add render feature for fixing the depth texture
		var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
		// The list has ForwardRenderer, WaveHeightMaskRenderer, and SteppedDepthMaskRenderer. Maybe the invert should happen in a different one or all of them
		// ForwardRenderer has "Water" as a rendererFeature, so that could also be something
		// Put the depth render feature first so it goes before water
		var dataLists = urp.GetValue<ScriptableRendererData[]>("m_RendererDataList");
		//var water = dataLists.First().rendererFeatures.First() as RenderObjects;

		// This makes the camera not upsidedown wtf
		// Other ways of not being upside-down mess with the haste smoke flame effects (and probably others)
		var renderObject = new RenderObjects { name = "Flip" };
		Delay.FireOnNextUpdate(() => renderObject.GetValue<RenderObjectsPass>("renderObjectsPass").renderPassEvent = RenderPassEvent.AfterRendering);
		dataLists.First().rendererFeatures.Add(renderObject);

		// Also want to add our depth buffer inverter
		//dataLists.ForEach(x => x.rendererFeatures.Add(ScriptableObject.CreateInstance<CustomDepthBufferModifier>()));
		//dataLists.First().rendererFeatures.Add(ScriptableObject.CreateInstance<CustomDepthBufferModifier>());

		RenderPipelineManager.beginFrameRendering += RenderPipelineManager_beginFrameRendering;
	}

	public void OnDestroy()
	{
		RenderPipelineManager.beginFrameRendering -= RenderPipelineManager_beginFrameRendering;
	}

	private void RenderPipelineManager_beginFrameRendering(ScriptableRenderContext ctx, Camera[] cameras)
	{

		foreach (var camera in cameras)
		{
			if (camera.actualRenderingPath == RenderingPath.DeferredShading)
			{

			}
		}
	}

	public class CustomDepthBufferModifier : ScriptableRendererFeature
	{
		private CustomDepthPass _pass;

		public override void Create()
		{
			_pass = new();
			_pass.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			renderer.EnqueuePass(_pass);
		}
	}

	public class CustomDepthPass : ScriptableRenderPass
	{
		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			var camera = renderingData.cameraData.camera;
			//DredgeVRLogger.Debug($"GO! {camera.name}");
			CommandBuffer cmd = CommandBufferPool.Get(nameof(CustomDepthPass));

			cmd.Blit(camera.targetTexture, camera.targetTexture, AssetLoader.FlipYAxisMaterial);

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}
	}
}
