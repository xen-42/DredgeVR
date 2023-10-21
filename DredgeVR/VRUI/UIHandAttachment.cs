using DredgeVR.Options;
using DredgeVR.VRCamera;
using UnityEngine;
using Valve.VR;

namespace DredgeVR.VRUI;

public class UIHandAttachment : MonoBehaviour
{
	public Vector3 _euler = new(45, 0, 0);
	public Vector3 _offset = new(0.2f, 0, 0);
	public float _scale = 1f;
	public bool _dominantHand;

	public void Init(bool dominantHand, Vector3 euler, Vector3 offset, float scale)
	{
		_dominantHand = dominantHand;
		_euler = euler;
		_offset = offset;
		_scale = scale;
	}

	public void Awake()
	{
		// For some control schemes the user might be better off just being able to target the buttons with their cursor
		if (OptionsManager.Options.useFlatUI)
		{
			enabled = false;
			Component.Destroy(this);
		}
	}

	public void Update()
	{
		var leftHand = _dominantHand == OptionsManager.Options.leftHanded;

		var handGO = leftHand ? VRCameraManager.LeftHand : VRCameraManager.RightHand;
		transform.position = handGO.transform.TransformPoint(new Vector3(_offset.x * (leftHand ? -1 : 1), _offset.y, _offset.z));
		transform.rotation = handGO.transform.rotation * Quaternion.Euler(_euler);
		transform.localScale = new Vector3(leftHand ? -_scale : _scale, _scale, _scale);
	}
}
