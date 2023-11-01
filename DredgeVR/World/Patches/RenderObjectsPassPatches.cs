using DredgeVR.Helpers;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DredgeVR.World.Patches;

[HarmonyPatch]
public static class RenderObjectsPassPatches
{
	private static RenderObjectsPass _target;
	private static RenderTexture _texture;
	private static int _depthTextureID = Shader.PropertyToID("_CameraDepthTexture");

	static RenderObjectsPassPatches()
	{
		var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
		var dataLists = urp.GetValue<ScriptableRendererData[]>("m_RendererDataList");
		var water = dataLists.First().rendererFeatures.First() as RenderObjects;
		_target = water.GetValue<RenderObjectsPass>("renderObjectsPass");
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(RenderObjectsPass), nameof(RenderObjectsPass.Execute))]
	public static bool RenderObjectsPass_Execute_Pre(RenderObjectsPass __instance, ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (__instance != _target) return true;

		try
		{
			var sortingCriteria = SortingCriteria.CommonTransparent;

			var m_ShaderTagIdList = __instance.GetValue<List<ShaderTagId>>("m_ShaderTagIdList");
			var drawingSettings = __instance.CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortingCriteria);
			drawingSettings.overrideMaterial = __instance.overrideMaterial;
			drawingSettings.overrideMaterialPassIndex = __instance.overrideMaterialPassIndex;

			var m_CameraSettings = __instance.GetValue<RenderObjects.CustomCameraSettings>("m_CameraSettings");
			var m_FilteringSettings = __instance.GetValue<FilteringSettings>("m_FilteringSettings");
			var m_RenderStateBlock = __instance.GetValue<RenderStateBlock>("m_RenderStateBlock");

			var m_ProfilerTag = __instance.GetValue<string>("m_ProfilerTag");
			var m_ProfilingSampler = __instance.GetValue<ProfilingSampler>("m_ProfilingSampler");

			ref var cameraData = ref renderingData.cameraData;
			var camera = cameraData.camera;

			var cmd = CommandBufferPool.Get(m_ProfilerTag);

			// Reset all to defaults
			//m_FilteringSettings = new FilteringSettings();
			//m_RenderStateBlock = new RenderStateBlock();
			//drawingSettings = new DrawingSettings();

			using (new ProfilingScope(cmd, m_ProfilingSampler))
			{
				_texture ??= new RenderTexture(new RenderTextureDescriptor());

				cmd.Blit(renderingData.cameraData.renderer.cameraDepthTarget, _texture, AssetLoader.ShowDepthMaterial);
				context.ExecuteCommandBuffer(cmd);
				cmd.Clear();

				//cmd.Blit(_texture, _texture, AssetLoader.FlipYAxisMaterial);
				context.ExecuteCommandBuffer(cmd);
				cmd.Clear();

				cmd.SetGlobalTexture(_depthTextureID, _texture);

				context.ExecuteCommandBuffer(cmd);
				cmd.Clear();

				camera.ResetCullingMatrix();
				camera.TryGetCullingParameters(true, out var cullingParameters);
				var cullResults = context.Cull(ref cullingParameters);

				// Draws the water but its in the sky
				context.DrawRenderers(cullResults, ref drawingSettings, ref m_FilteringSettings, ref m_RenderStateBlock);
			}

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);

		}
		catch (Exception e)
		{
			DredgeVRLogger.Debug(e);
		}

		return false;
	}
}
