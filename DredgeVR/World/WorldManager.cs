using DredgeVR.Helpers;
using System.Linq;
using UnityEngine;

namespace DredgeVR.World;

internal class WorldManager : MonoBehaviour
{
	private Shader _litShader = Shader.Find("Shader Graphs/Lit_Shader");

	public void Awake()
	{
		DredgeVRCore.SceneStart += OnSceneStart;
		DredgeVRCore.GameSceneStart += OnGameSceneStart;
	}

	private void OnSceneStart(string scene)
	{
		// TODO: Make this a setting
		// Heightmap terrain causes massive lag in VR
		foreach (var terrain in GameObject.FindObjectsOfType<Terrain>())
		{
			terrain.heightmapMaximumLOD = Mathf.Max(terrain.heightmapMaximumLOD, 3);
		}

		// TODO: Make this a setting
		// Force lower all LOD
		foreach (var lodGroup in GameObject.FindObjectsOfType<LODGroup>())
		{
			lodGroup.ForceLOD(lodGroup.GetLODs().Count() - 1);
			lodGroup.enabled = false;
		}
	}

	private void OnGameSceneStart()
	{
		// Ghost rocks only work in one eye, have to change their shader to one that works in both
		// Unfortunately this gets rid of their cool effect and just makes them regular rocks
		foreach (var ghostRock in GameObject.FindObjectsOfType<GhostRock>())
		{
			ghostRock.rockMeshObject.GetComponent<MeshRenderer>().material.shader = _litShader;
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
			}
		);
	}
}
