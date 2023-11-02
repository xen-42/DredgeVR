using Cinemachine.Utility;
using DredgeVR.Helpers;
using DredgeVR.VRInput;
using DredgeVR.VRUI;
using UnityEngine;

namespace DredgeVR.Items;

public class HeldCompass : MonoBehaviour
{
	private CompassUI _compassUI;
	private Transform _markers;
	private Transform _cover;
	private Transform _compassFace;

	public void Awake()
	{
		var uiHand = gameObject.AddComponent<UIHandAttachment>();

		uiHand.smoothRotation = false;
		uiHand.smoothPosition = false;

		// Hold in off hand
		var inRightHand = VRInputModule.Instance.DominantHandInputSource == Valve.VR.SteamVR_Input_Sources.LeftHand;
		uiHand.Init(inRightHand, new Vector3(0, 90, 48), new Vector3(0.04f, -0.09f, 0.005f), 0.9f);

		// We will be overwriting the base compass behaviour
		_compassUI = GetComponent<CompassUI>();
		_compassUI.enabled = false;

		_compassFace = transform.Find("CompassFace");

		_markers = _compassFace.Find("Markers");
		_markers.transform.localPosition = new Vector3(0, 0, -4);

		_cover = _compassFace.Find("CompassFace (1)");
		_cover.transform.localPosition = new Vector3(0, 0, -2);

		// Add the model
		var compass = GameObject.Instantiate(AssetLoader.Compass).SetParent(transform);
		compass.name = "Compass";
		compass.transform.localScale = Vector3.one * 6300f;
		compass.transform.localRotation = Quaternion.Euler(0, 90, 270);
		compass.transform.localPosition = new Vector3(0, 0, 10);
		compass.transform.Find("compass").localRotation = Quaternion.Euler(0, 300, 0);
	}

	public void OnEnable()
	{
		VRInputModule.Instance.OffHand.HoldCompass();
	}

	public void OnDisable()
	{
		VRInputModule.Instance.OffHand.StopHolding();
	}

	public void Update()
	{
		// Turn the compass face towards north
		// Turn the markers and cover back to offset this
		var north = Vector3.forward; // Is this true?

		// The compass is oriented strangely, basically up is +z
		var northInPlane = north.ProjectOntoPlane(transform.forward);
		var rot = Quaternion.LookRotation(transform.forward, northInPlane);

		_compassFace.rotation = rot;
	}
}
