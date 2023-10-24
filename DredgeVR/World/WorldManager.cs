using DredgeVR.Helpers;
using DredgeVR.Items;
using DredgeVR.Options;
using System.Linq;
using UnityEngine;

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

		QualitySettings.shadows = ShadowQuality.Disable;
		QualitySettings.vSyncCount = 2;

		DredgeVRCore.SceneStart += OnSceneStart;
		DredgeVRCore.GameSceneStart += OnGameSceneStart;
	}

	public void OnDestroy()
	{
		DredgeVRCore.SceneStart -= OnSceneStart;
		DredgeVRCore.GameSceneStart -= OnGameSceneStart;
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

		// Have to wait a frame for the boat to exist
		Delay.RunWhen(
			() => GameManager.Instance.Player != null,
			() =>
			{
				// Smoke columns use line renderers which don't work in VR
				foreach (var smokeColumn in GameObject.FindObjectsOfType<SmokeColumn>(true))
				{
					smokeColumn.gameObject.SetActive(false);
				}

				// Set up held items
				GameObject.FindObjectOfType<MapWindow>().gameObject.AddComponent<HeldMap>().SetOffset(650, 300);
				GameObject.FindObjectOfType<MessageDetailWindow>().gameObject.AddComponent<HeldMap>().SetOffset(450, 50);
			}
		);

		// Replacing the shaders doesn't fix it, they still show in the wrong eyes
		/*
		var badShaders = new string[]
		{
			"Shader Graphs/Particle_Shader",
			"Shader Graphs/FloatingParticle_Shader",
			"Shader Graphs/ShimmerWarp_Shader"
		};

		foreach (var particles in GameObject.FindObjectsOfType<ParticleSystemRenderer>())
		{
			if (badShaders.Contains(particles.material.shader.name))
			{
				particles.material.shader = _litShader;
			}
		}
		*/
	}
}
