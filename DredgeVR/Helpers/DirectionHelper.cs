using DredgeVR.VRCamera;
using UnityEngine;

namespace DredgeVR.Helpers;

public static class DirectionHelper
{
	/// <summary>
	/// Staying flat in the x-z plane, look towards the player with y-axis rotation
	/// </summary>
	/// <param name="transform"></param>
	public static void LookAtPlayerInPlane(Transform transform, bool flipDirection)
	{
		var playerPos = VRCameraManager.VRPlayer.transform.position;
		var worldPosInPlane = new Vector3(playerPos.x, transform.position.y, playerPos.z);
		var direction = worldPosInPlane - transform.position;
		transform.rotation = Quaternion.LookRotation(flipDirection ? -direction : direction, Vector3.up);
	}
}
