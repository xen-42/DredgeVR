using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DredgeVR.VRUI;

/// <summary>
/// The funds and backplate game objects aren't hidden when it's offscreen
/// These can still be seen in VR since there's no "offscreen" anymore
/// </summary>
[RequireComponent(typeof(SlidePanel))]
public class SlidePanelFixer : MonoBehaviour
{
	private SlidePanel _slidePanel;
	private GameObject[] _targetGameObjects;
	private Image _image;

	public string[] targets;

	public void Start()
	{
		_slidePanel = GetComponent<SlidePanel>();

		_targetGameObjects = targets.Select(x => transform.Find(x).gameObject).Where(x => x != null).ToArray();

		_image = GetComponent<Image>();

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
		foreach (var target in _targetGameObjects)
		{
			target.SetActive(false);
		}
		if (_image != null)
		{
			_image.enabled = false;
		}
	}

	private void Show()
	{
		foreach (var target in _targetGameObjects)
		{
			target.SetActive(true);
		}
		if (_image != null)
		{
			_image.enabled = true;
		}
	}
}
