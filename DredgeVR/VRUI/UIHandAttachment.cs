using DredgeVR.VRCamera;
using UnityEngine;
using Valve.VR;

namespace DredgeVR.VRUI;

public class UIHandAttachment : MonoBehaviour
{
	public Vector3 _euler = new(45, 0, 0);
	public Vector3 _offset = new(0.2f, 0, 0);
	public float _scale = 1f;
	public SteamVR_Input_Sources _hand;

	public void Init(SteamVR_Input_Sources hand, Vector3 euler, Vector3 offset, float scale)
	{
		_hand = hand;
		_euler = euler;
		_offset = offset;
		_scale = scale;
	}

	public void Update()
	{
		var isLeft = _hand == SteamVR_Input_Sources.LeftHand;
		var handGO = isLeft ? VRCameraManager.LeftHand : VRCameraManager.RightHand;
		transform.position = handGO.transform.TransformPoint(new Vector3(_offset.x * (isLeft ? -1 : 1), _offset.y, _offset.z));
		transform.rotation = handGO.transform.rotation * Quaternion.Euler(_euler);
		transform.localScale = new Vector3(isLeft ? -_scale : _scale, _scale, _scale);
	}
}
