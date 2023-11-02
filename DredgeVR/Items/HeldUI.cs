using DredgeVR.VRInput;
using DredgeVR.VRUI;
using UnityEngine;

namespace DredgeVR.Items;

/// <summary>
/// Attaches a flat UI element to the players hand instead
/// </summary>
public class HeldUI : MonoBehaviour
{
	private Vector2 _offset;
	private Transform _container;

	private bool _inRightHand;

	public void Awake()
	{
		var uiHand = gameObject.AddComponent<UIHandAttachment>();
		// Hold in off hand
		_inRightHand = VRInputModule.Instance.DominantHandInputSource == Valve.VR.SteamVR_Input_Sources.LeftHand;
		uiHand.Init(_inRightHand, new Vector3(0, 90, 45), new Vector3(0.05f, -0.1f, 0f), 0.5f);
		uiHand.smoothRotation = false;
		_container = transform.Find("Container").transform;
		_container.localRotation = Quaternion.Euler(0, _inRightHand ? -45 : 45, 0);
	}

	/// <summary>
	/// Offset is more or less relative to the direction your index finger points
	/// X being in the direction of the index, Y being the direction of the thumb when doing finger guns
	/// </summary>
	/// <param name="offsetX"></param>
	/// <param name="offsetY"></param>
	public void SetOffset(int offsetX, int offsetY)
	{
		_offset = new Vector2(offsetX, offsetY);
	}

	public void Update()
	{
		_container.localPosition = _container.localRotation * new Vector3(_inRightHand ? -_offset.x : _offset.x, _offset.y, 0f);
	}

	public void OnEnable()
	{
		// Some elements of the flat UI looks really off in VR, will have different names on each object so have to null check
		transform.Find("Container/Scrim")?.gameObject?.SetActive(false);
		transform.Find("Container/Title")?.gameObject?.SetActive(false);
	}
}
