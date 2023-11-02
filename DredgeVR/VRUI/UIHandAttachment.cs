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

	public bool smoothRotation = true;
	public bool smoothPosition = true;

	private RectTransform _rectTransform;

	private bool _hidden;

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

		// For left handed switch the pivot over to be more consistent
		_rectTransform = gameObject.GetComponent<RectTransform>();
		if (_leftHand)
		{
			_rectTransform.pivot = new Vector2(1 - _rectTransform.pivot.x, _rectTransform.pivot.y);
		}

		VRUIManager.HeldUIHidden += OnHeldUIHidden;

		// TODO: Fix canvas layers

		// TODO: Allow targeting by touching with hand
	}

	public void OnDestroy()
	{
		VRUIManager.HeldUIHidden -= OnHeldUIHidden;
	}

	public void Start()
	{
		transform.localScale = Vector3.one * _scale;
	}

	public void Update()
	{
		if (_hidden)
		{
			return;
		}

		var handGO = _leftHand ? VRCameraManager.LeftHand : VRCameraManager.RightHand;

		var rotatedOffset = Quaternion.Euler(_euler) * _offset;

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
		}
		else
		{
			transform.position = targetPosition;
		}

		if (smoothRotation)
		{
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, t);
		}
		else
		{
			// Snap angle
			if (Quaternion.Angle(transform.rotation, targetRotation) > 0.5f)
			{
				transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, t);
			}
		}
	}

	public void OnHeldUIHidden(bool hidden)
	{
		// Some UI elements have logic on them which breaks if we disable them
		// Just want to make them invisible
		_hidden = hidden;
		transform.localScale = Vector3.one * (hidden ? 0 : _scale);
	}
}
