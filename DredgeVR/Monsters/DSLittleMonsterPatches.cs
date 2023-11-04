using DredgeVR.Helpers;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DredgeVR.Monsters;

[HarmonyPatch]
public static class DSLittleMonsterPatches
{
	private static Transform _leftAttachPoint, _rightAttachPoint;

	static DSLittleMonsterPatches()
	{
		DredgeVRCore.PlayerSpawned += OnPlayerSpawned;
		if (SceneManager.GetActiveScene().name == "Game" && GameManager.Instance.Player != null)
		{
			OnPlayerSpawned();
		}
	}

	private static void OnPlayerSpawned()
	{
		// Adds two transforms on either side of the boat for the little monsters to attach to
		_leftAttachPoint = new GameObject("LeftAttachPoint").SetParent(GameManager.Instance.Player.transform).transform;
		_leftAttachPoint.transform.localPosition = Vector3.left + Vector3.forward + Vector3.up * 0.5f;

		_rightAttachPoint = new GameObject("RightAttachPoint").SetParent(GameManager.Instance.Player.transform).transform;
		_rightAttachPoint.transform.localPosition = Vector3.right + Vector3.forward + Vector3.up * 0.5f;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(DSLittleMonsterSpawner), nameof(DSLittleMonsterSpawner.SpawnMonster))]
	public static void DSLittleMonsterSpawner_SpawnMonster(DSLittleMonsterSpawner __instance)
	{
		__instance.monsters.Last().playerAnchor = Random.Range(0f, 1f) > 0.5f ? _leftAttachPoint : _rightAttachPoint;
	}
}
