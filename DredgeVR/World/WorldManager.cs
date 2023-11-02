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

		QualitySettings.vSyncCount = 2;

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
			if (OptionsManager.Options.disableUnderseaDetails) 
			{
				// The "trees and foliage" are actually rocks on the sea floor
				terrain.drawTreesAndFoliage = false;
			}

			terrain.treeLODBiasMultiplier = LOD_BIAS;
		}

		foreach (var particleSystem in GameObject.FindObjectsOfType<ParticleSystem>())
		{
			if (OptionsManager.Options.disableExtraParticleEffects)
			{
				particleSystem.gameObject.SetActive(particleSystem.gameObject.GetComponentInParent<HarvestableParticles>() != null);
			}
		}

		if (OptionsManager.Options.removeTrees)
		{
			foreach (var tree in GameObject.FindObjectsOfType<GameObject>().Where(x => x.name == "Trees"))
			{
				tree.SetActive(false);
			}
			GameObject.Find("TheMarrows/Islands/LittleMarrow/Details")?.SetActive(false);
		}
	}

	private void OnGameSceneStart()
	{
		if (OptionsManager.Options.disableDistantParticleEffects)
		{
			foreach (var harvestable in GameObject.FindObjectsOfType<HarvestableParticles>(true))
			{
				harvestable.gameObject.AddComponent<LODChildCuller>();
			}
		}

		if (OptionsManager.Options.disableCullingBrain)
		{
			// Actually destroying this breaks loading screens
			// Important part is disconnecting all the CullingGroup events which happens in OnDestroy
			var cullingBrain = GameManager.Instance.CullingBrain;
			cullingBrain.OnDestroy();
			// OnDestroy breaks the reference to the culling brain so we reconnect it so other scripts don't NRE
			GameManager.Instance.CullingBrain = cullingBrain;
		}
	}

	public void OnPlayerSpawned()
	{
		// Set up held items
		GameObject.FindObjectOfType<MapWindow>().gameObject.AddComponent<HeldUI>().SetOffset(570, 300);
		GameObject.FindObjectOfType<MessageDetailWindow>().gameObject.AddComponent<HeldUI>().SetOffset(450, 50);

		// This has to happen here else the shader is null
		// Put a giant black square at the bottom of the sea
		// Since ocean depth shading is broken this stops us seeing out of the map
		var seaFloorCover = new GameObject("SeaFloorCover");
		seaFloorCover.transform.position = new Vector3(0, -100, 0);
		seaFloorCover.transform.rotation = Quaternion.Euler(90, 0, 0);
		seaFloorCover.transform.localScale = Vector3.one * 10000;

		seaFloorCover.AddComponent<MeshFilter>().mesh = AssetLoader.PrimitiveQuad;

		var material = new Material(AssetLoader.UnlitShader);
		seaFloorCover.AddComponent<MeshRenderer>().material = material;
		material.mainTexture = Texture2D.blackTexture;
	}
}
