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

namespace DredgeVR.World.Patches;

[HarmonyPatch(typeof(RenderObjectsPass))]
public static class RenderObjectsPassPatches
{
	private static RenderObjectsPass _target;

	static RenderObjectsPassPatches()
	{
		var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
		var dataLists = urp.GetValue<ScriptableRendererData[]>("m_RendererDataList");
		var water = dataLists.First().rendererFeatures.First() as RenderObjects;
		_target = water.GetValue<RenderObjectsPass>("renderObjectsPass");
	}

	/*
	[HarmonyPrefix]
	[HarmonyPatch(nameof(RenderObjectsPass.Execute))]
	public static void RenderObjectsPass_Execute_Pre(RenderObjectsPass __instance, ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (__instance != _target) return;

		var camera = renderingData.cameraData.camera;
		var t = camera.transform;
		t.LookAt(t.transform.position - t.forward, -t.up);
	}

	[HarmonyPostfix]
	[HarmonyPatch(nameof(RenderObjectsPass.Execute))]
	public static void RenderObjectsPass_Execute_Post(RenderObjectsPass __instance, ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (__instance != _target) return;

		var camera = renderingData.cameraData.camera;
		var t = camera.transform;
		t.LookAt(t.transform.position - t.forward, -t.up);
	}
	*/


	[HarmonyPrefix]
	[HarmonyPatch(nameof(RenderObjectsPass.Execute))]
	public static bool RenderObjectsPass_Execute_Pre(RenderObjectsPass __instance, ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (__instance != _target) return true;

		try
		{
			SortingCriteria sortingCriteria = (__instance.GetValue<RenderQueueType>("renderQueueType") == RenderQueueType.Transparent)
							? SortingCriteria.CommonTransparent
							: renderingData.cameraData.defaultOpaqueSortFlags;

			DrawingSettings drawingSettings = __instance.CreateDrawingSettings(__instance.GetValue<List<ShaderTagId>>("m_ShaderTagIdList"), ref renderingData, sortingCriteria);
			drawingSettings.overrideMaterial = __instance.overrideMaterial;
			drawingSettings.overrideMaterialPassIndex = __instance.overrideMaterialPassIndex;

			ref CameraData cameraData = ref renderingData.cameraData;
			Camera camera = cameraData.camera;

			Rect pixelRect = renderingData.cameraData.GetValue<Rect>("pixelRect");
			float cameraAspect = (float)pixelRect.width / (float)pixelRect.height;
			CommandBuffer cmd = CommandBufferPool.Get(__instance.GetValue<string>("m_ProfilerTag"));
			using (new ProfilingScope(cmd, __instance.GetValue<ProfilingSampler>("m_ProfilingSampler")))
			{
				context.ExecuteCommandBuffer(cmd);
				cmd.Clear();

				var m_CameraSettings = __instance.GetValue<RenderObjects.CustomCameraSettings>("m_CameraSettings");
				var m_FilteringSettings = __instance.GetValue<FilteringSettings>("m_FilteringSettings");
				var m_RenderStateBlock = __instance.GetValue<RenderStateBlock>("m_RenderStateBlock");

				if (m_CameraSettings.overrideCamera)
				{
					Matrix4x4 projectionMatrix = Matrix4x4.Perspective(m_CameraSettings.cameraFieldOfView, cameraAspect,
						camera.nearClipPlane, camera.farClipPlane);

					Matrix4x4 viewMatrix = camera.worldToCameraMatrix;
					Vector4 cameraTranslation = viewMatrix.GetColumn(3);
					viewMatrix.SetColumn(3, cameraTranslation + m_CameraSettings.offset);

					cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
					context.ExecuteCommandBuffer(cmd);
				}

				context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings,
					ref m_RenderStateBlock);

				if (m_CameraSettings.overrideCamera && m_CameraSettings.restoreCamera)
				{
					cmd.Clear();
					cmd.SetViewProjectionMatrices(cameraData.GetValue<Matrix4x4>("viewMatrix"), cameraData.GetValue<Matrix4x4>("projectionMatrix"));
				}
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
