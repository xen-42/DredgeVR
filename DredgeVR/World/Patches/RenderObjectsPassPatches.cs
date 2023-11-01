using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System.Security.Policy;
using DredgeVR.Helpers;
using UnityEngine;
using System.Drawing.Drawing2D;

namespace DredgeVR.World.Patches;

[HarmonyPatch(typeof(RenderObjectsPass))]
public static class RenderObjectsPassPatches
{
	private static RenderObjectsPass _target;
	private static RenderTexture _texture;

	static RenderObjectsPassPatches()
	{
		var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
		var dataLists = urp.GetValue<ScriptableRendererData[]>("m_RendererDataList");
		var water = dataLists.First().rendererFeatures.First() as RenderObjects;
		_target = water.GetValue<RenderObjectsPass>("renderObjectsPass");
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(RenderObjectsPass.Execute))]
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
				var matrix = cameraData.GetProjectionMatrix();
				cmd.SetProjectionMatrix(matrix * Matrix4x4.Scale(new Vector3(1, -1, 1)));
				cmd.SetInvertCulling(false);

				context.ExecuteCommandBuffer(cmd);
				cmd.Clear();

				// Draws the water but its in the sky
				context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings, ref m_RenderStateBlock);

				context.ExecuteCommandBuffer(cmd);
				cmd.Clear();

				// Put it back
				cmd.SetProjectionMatrix(matrix);
				cmd.SetInvertCulling(true);
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
