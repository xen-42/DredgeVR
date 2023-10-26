using DredgeVR.Helpers;
using DredgeVR.Items;
using DredgeVR.Options;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEngine.ParticleSystem;

namespace DredgeVR.World;

/// <summary>
/// This behaviour is responsible for fixing physical objects in the game
/// Mostly by correcting issues with shaders
/// </summary>
internal class WorldManager : MonoBehaviour
{
	// Magic number found online, without this LOD is inconsistent between eyes
	public const float LOD_BIAS = 3.8f;

	public void Awake()
	{
		QualitySettings.lodBias = LOD_BIAS;

		if (OptionsManager.Options.decreaseLOD)
		{
			QualitySettings.maximumLODLevel = 1;
		}

		QualitySettings.vSyncCount = 2;

		var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
		// Shadows do not work in OpenVR with URP
		// urp.SetValue("m_MainLightShadowsSupported", false);

		DredgeVRCore.SceneStart += OnSceneStart;
		DredgeVRCore.GameSceneStart += OnGameSceneStart;
		DredgeVRCore.PlayerSpawned += OnPlayerSpawned;
	}

	public void OnDestroy()
	{
		DredgeVRCore.SceneStart -= OnSceneStart;
		DredgeVRCore.GameSceneStart -= OnGameSceneStart;
		DredgeVRCore.PlayerSpawned -= OnPlayerSpawned;

	}

	private void OnSceneStart(string scene)
	{
		// Heightmap terrain causes massive lag in VR
		foreach (var terrain in GameObject.FindObjectsOfType<Terrain>())
		{
			if (OptionsManager.Options.lowerTerrainLOD)
			{
				terrain.heightmapMaximumLOD = Mathf.Max(terrain.heightmapMaximumLOD, 3);
			}

			terrain.treeLODBiasMultiplier = LOD_BIAS;
		}

		// Reflections look super weird in VR - Make sure its off when we load in
		GameObject.Find("ReflectionCamera")?.gameObject?.SetActive(false);
		// Replace shader reflection texture
		Shader.SetGlobalTexture(Shader.PropertyToID("_PlanarReflectionTexture"), new Texture2D(2, 2));

		var waterMat = GameObject.FindObjectOfType<ReflectionSettingResponder>().waterMat;
		waterMat.DisableKeyword("_REFLECTIONS");
		waterMat.SetFloat("_ReflectionStrength", 0f);

		foreach (var particleSystem in GameObject.FindObjectsOfType<ParticleSystem>())
		{
			if (OptionsManager.Options.disableExtraParticleEffects)
			{
				particleSystem.gameObject.SetActive(particleSystem.gameObject.GetComponentInParent<HarvestableParticles>() != null);
			}
			else
			{
				var emission = particleSystem.emission;
				emission.rateOverTimeMultiplier = 0.5f;
			}
		}

		if (OptionsManager.Options.removeTrees)
		{
			foreach (var tree in GameObject.FindObjectsOfType<GameObject>().Where(x => x.name == "Trees"))
			{
				tree.SetActive(false);
			}
			GameObject.Find("TheMarrows/Islands/LittleMarrow/Details").SetActive(false);
		}
	}

	private void OnGameSceneStart()
	{
		// Ghost rocks only work in one eye, have to change their shader to one that works in both
		// Unfortunately this gets rid of their cool effect and just makes them regular rocks
		foreach (var ghostRock in GameObject.FindObjectsOfType<GhostRock>())
		{
			ghostRock.rockMeshObject.GetComponent<MeshRenderer>().material.shader = AssetLoader.LitShader;
		}

		if (OptionsManager.Options.disableDistantParticleEffects)
		{
			foreach (var harvestable in GameObject.FindObjectsOfType<HarvestableParticles>(true))
			{
				harvestable.gameObject.AddComponent<LODChildCuller>();
			}
		}
	}

	public void OnPlayerSpawned()
	{
		// Smoke columns use line renderers which don't work in VR
		foreach (var smokeColumn in GameObject.FindObjectsOfType<SmokeColumn>(true))
		{
			smokeColumn.gameObject.SetActive(false);
		}

		// Set up held items
		GameObject.FindObjectOfType<MapWindow>().gameObject.AddComponent<HeldUI>().SetOffset(650, 300);
		GameObject.FindObjectOfType<MessageDetailWindow>().gameObject.AddComponent<HeldUI>().SetOffset(450, 50);

		FixAllParticles();
	}

	private void FixAllParticles()
	{
		// ParticleSystemRenderers that don't use RenderMode = Mesh only show in one eye
		// Well, think it's more that alignment View doesn't actually face the right eye when rendering or something

		// TODO: Orient the rain so the particles follow their velocity
		var rain = GameObject.Find("FollowPlayer/Rain");
		FixParticles(rain.GetComponent<ParticleSystemRenderer>(), AssetLoader.PrimitiveCylinder, false);

		var rainDrops = GameObject.Find("FollowPlayer/Rain/SubEmitter_RainSplashes");
		FixParticles(rainDrops.GetComponent<ParticleSystemRenderer>(), AssetLoader.PrimitiveQuad, true);

		foreach (var inspectionPOI in GameObject.FindObjectsOfType<InspectPOI>())
		{
			FixParticles(inspectionPOI.GetComponentInChildren<ParticleSystemRenderer>(), AssetLoader.DoubleSidedQuad, true);
		}

		foreach (var harvestableParticles in GameObject.FindObjectsOfType<HarvestableParticles>())
		{
			var beams = harvestableParticles.transform.Find("Beam")?.GetComponentsInChildren<ParticleSystemRenderer>() ?? new ParticleSystemRenderer[] { };
			var embers = harvestableParticles.transform.Find("Embers")?.GetComponentsInChildren<ParticleSystemRenderer>() ?? new ParticleSystemRenderer[] { };
			foreach (var particle in beams.Concat(embers))
			{
				FixParticles(particle, AssetLoader.DoubleSidedQuad, true);
			}
		}
	}

	private void FixParticles(ParticleSystemRenderer renderer, Mesh mesh, bool lookAtPlayer)
	{
		if (renderer != null)
		{
			renderer.renderMode = ParticleSystemRenderMode.Mesh;
			if (lookAtPlayer)
			{
				renderer.gameObject.AddComponent<LookAtPlayer>();
			}
			renderer.mesh = mesh;
			renderer.alignment = ParticleSystemRenderSpace.Local;
		}
	}
}
