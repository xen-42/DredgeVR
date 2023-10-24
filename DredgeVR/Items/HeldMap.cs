﻿using DredgeVR.VRUI;
using UnityEngine;
using Valve.VR;
using static UnityEngine.Rendering.DebugUI.Table;

namespace DredgeVR.Items;

public class HeldMap : MonoBehaviour
{
	private Vector2 _offset;
	private Transform _container;

	public void Awake()
	{
		var uiHand = gameObject.AddComponent<UIHandAttachment>();
		uiHand.Init(false, new Vector3(0, 90, 45), new Vector3(0.05f, -0.1f, 0f), 0.5f);
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
		transform.Find("Container/Scrim").gameObject.SetActive(false);
		transform.Find("Container/Title")?.gameObject?.SetActive(false);
	}
}
