using UnityEngine;

namespace DredgeVR.VRUI;

/// <summary>
/// The funds and backplate game objects aren't hidden when it's offscreen
/// These can still be seen in VR since there's no "offscreen" anymore
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
		transform.Find("SlidePanelTab").transform.localScale = Vector3.one * 1.5f;

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
