using Cinemachine.Utility;
using DredgeVR.VRCamera;
using UnityEngine;

namespace DredgeVR.VRUI;

internal class GameSceneUI : MonoBehaviour
{
	public Vector3 Offset = new (0.5f, -0.25f, 2.5f);
	public Vector3 RotationEuler = Quaternion.identity.eulerAngles;
	public bool doRotation = true;

	public void Start()
	{
	}

	public void Update()
	{
		var localDirection = VRCameraManager.VRPlayer.transform.TransformDirection(Offset);
		var localOffset = localDirection.ProjectOntoPlane(VRCameraManager.VRPlayer.transform.up);
		this.transform.position = VRCameraManager.VRPlayer.transform.position + localOffset;
		//this.transform.rotation = doRotation ? Quaternion.identity : VRCameraManager.VRPlayer.transform.rotation * Quaternion.Euler(RotationEuler);
		this.transform.rotation = Quaternion.Euler(0f, VRCameraManager.VRPlayer.transform.rotation.eulerAngles.y, 0f);
		this.transform.localScale = Vector3.one * 0.002f;
	}
}