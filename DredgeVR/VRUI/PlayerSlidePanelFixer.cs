﻿using UnityEngine;

namespace DredgeVR.VRUI;

/// <summary>
/// The funds and backplate game objects aren't hidden when it's offscreen
/// These can still be seen in VR
/// </summary>
[RequireComponent(typeof(SlidePanel))]
public class PlayerSlidePanelFixer : MonoBehaviour
{
	private SlidePanel _slidePanel;
	private GameObject _funds, _backplate;

	public void Awake()
	{
		_slidePanel = GetComponent<SlidePanel>();
		_funds = transform.Find("Funds").gameObject;
		_backplate = transform.Find("Backplate").gameObject;

		_slidePanel.OnHideFinish.AddListener(Hide);
		_slidePanel.OnShowStart.AddListener(Show);

		Hide();
	}

	public void OnDestroy()
	{
		_slidePanel.OnHideStart.RemoveListener(Hide);
		_slidePanel.OnShowFinish.RemoveListener(Show);
	}

	private void Hide()
	{
		_funds.SetActive(false);
		_backplate.SetActive(false);
	}

	private void Show()
	{
		_funds.SetActive(true);
		_backplate.SetActive(true);
	}
}