using Cinemachine.Utility;
using DredgeVR.VRInput;
using DredgeVR.VRUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		// Hold in off hand
		var inRightHand = VRInputModule.Instance.DominantHandInputSource == Valve.VR.SteamVR_Input_Sources.LeftHand;
		uiHand.Init(inRightHand, new Vector3(0, 90, 45), new Vector3(0.04f, -0.09f, 0.01f), 1f);

		// We will be overwriting the base compass behaviour
		_compassUI = GetComponent<CompassUI>();
		_compassUI.enabled = false;

		_compassFace = transform.Find("CompassFace");

		_markers = _compassFace.Find("Markers");
		_markers.transform.localPosition = new Vector3(0, 0, -4);

		_cover = _compassFace.Find("CompassFace (1)");
		_cover.transform.localPosition = new Vector3(0, 0, -2);
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
