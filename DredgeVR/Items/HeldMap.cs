﻿using DredgeVR.VRCamera;
using DredgeVR.VRInput;
using DredgeVR.VRUI;
using UnityEngine;

namespace DredgeVR.Items;

public class HeldUI : MonoBehaviour
{
	private Vector2 _offset;
	private Transform _container;

	public void Awake()
	{
		var uiHand = gameObject.AddComponent<UIHandAttachment>();
		// Hold in off hand
		uiHand.Init(VRInputModule.Instance.DominantHandInputSource == Valve.VR.SteamVR_Input_Sources.LeftHand, new Vector3(0, 90, 45), new Vector3(0.05f, -0.1f, 0f), 0.5f);
		uiHand.smoothPosition = false;
		_container = transform.Find("Container").transform;
		_container.localRotation = Quaternion.Euler(0, 45, 0);
	}

	public void SetOffset(int offsetX, int offsetY)
	{
		_offset = new Vector2(offsetX, offsetY);
	}

	public void Update()
	{
		_container.localPosition = _container.localRotation * new Vector3(_offset.x, _offset.y, 0f);
	}

	public void OnEnable()
	{
		transform.Find("Container/Scrim")?.gameObject.SetActive(false);
		transform.Find("Container/Title")?.gameObject?.SetActive(false);
	}
}
