using DredgeVR.Options;
using DredgeVR.VRCamera;
using UnityEngine;

namespace DredgeVR.VRUI;

public class UIHandAttachment : MonoBehaviour
{
	public Vector3 _euler = new(45, 0, 0);
	public Vector3 _offset = new(0.2f, 0, 0);
	public float _scale = 1f;
	public bool _leftHand;

	public bool smoothPosition = true;

	public void Init(bool rightHand, Vector3 euler, Vector3 offset, float scale)
	{
		_leftHand = !rightHand;
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

			// Since it'll be flat double the size
			transform.localScale = Vector3.one * 2f;
		}

		// TODO: Fix canvas layers

		// TODO: Allow targeting by touching with hand
	}

	public void Start()
	{
		transform.localScale = Vector3.one * _scale;
	}

	public void Update()
	{
		var handGO = _leftHand ? VRCameraManager.LeftHand : VRCameraManager.RightHand;

		// Don't take into account the middle of the UI because it's very inconsistent
		// var centerOffset = transform.lossyScale.x * new Vector3(_rectTransform.rect.center.x, _rectTransform.rect.center.y, 0f);
		var mirroredOffset = new Vector3(_offset.x, _offset.y, _offset.z);
		var rotatedOffset = Quaternion.Euler(_euler) * (mirroredOffset);

		var targetPosition = handGO.transform.TransformPoint(rotatedOffset);
		var targetRotation = handGO.transform.rotation * Quaternion.Euler(_euler);
		// For the left hand we have to rotate it 180 degrees around the Y axis
		if (_leftHand)
		{
			targetRotation *= Quaternion.Euler(0, 180f, 0);
		}

		// Smoothly move to position/rotation to jitter less
		var t = Time.unscaledDeltaTime * 15f;

		if (smoothPosition)
		{
			transform.position = Vector3.Lerp(transform.position, targetPosition, t);
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, t);
		}
		else																								   
		{
			transform.position = Vector3.Lerp(transform.position, targetPosition, t);
			// Snap angle
			if (Quaternion.Angle(transform.rotation, targetRotation) > 0.5f)
			{
				transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, t);
			}
		}
	}
}
