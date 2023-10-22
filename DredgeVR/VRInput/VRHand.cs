using DredgeVR.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.ResourceManagement;
using Valve.VR;
using Winch.Core;

namespace DredgeVR.VRInput;

public class VRHand : MonoBehaviour
{
	public Camera RaycastCamera { get; private set; }
	private GameObject _line;
	public GameObject LaserPointerEnd { get; private set; }
	private bool _graphicsInitialized;

	public float defaultLength = 5.0f;
	public SteamVR_Input_Sources hand;

	public bool IsDominantHand { get; private set; }

	public void Start()
	{
		RaycastCamera = new GameObject("RaycastCamera").AddComponent<Camera>();
		RaycastCamera.transform.parent = transform;
		RaycastCamera.transform.localPosition = Vector3.zero;
		RaycastCamera.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
		RaycastCamera.nearClipPlane = 0.01f;
		RaycastCamera.farClipPlane = 1000f;
		RaycastCamera.enabled = false;

		LaserPointerEnd = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		Component.DestroyImmediate(LaserPointerEnd.GetComponent<SphereCollider>());
		LaserPointerEnd.transform.parent = RaycastCamera.transform;
		LaserPointerEnd.transform.localScale = Vector3.one * 0.025f;
		LaserPointerEnd.name = "Dot";

		// Tried using a line renderer for this but it did not behave in VR
		_line = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		Component.DestroyImmediate(_line.GetComponent<Collider>());
		_line.transform.parent = RaycastCamera.transform;
		_line.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
		_line.name = "Line";

		// Disable rendering until the graphics are initialized
		LaserPointerEnd.SetActive(false);
		_line.SetActive(false);

		VRInputModule.Instance.DominantHandChanged += OnDominantHandChanged;
		OnDominantHandChanged(VRInputModule.Instance.DominantHandInputSource);

		InitGraphics();
	}

	private void OnDominantHandChanged(SteamVR_Input_Sources dominantHand)
	{
		IsDominantHand = dominantHand == hand;
	}

	private void InitGraphics()
	{
		// Need a fresh material for our laser pointer
		var material = new Material(AssetLoader.LitShader);

		var lineMR = _line.GetComponent<MeshRenderer>();
		lineMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		lineMR.sharedMaterial = material;

		var endMR = LaserPointerEnd.GetComponent<MeshRenderer>();
		endMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		endMR.sharedMaterial = material;

		_graphicsInitialized = true;
	}

	public void Update()
	{
		if (!_graphicsInitialized)
		{
			return;
		}

		if (IsDominantHand)
		{
			// Use raycast from input data if it's hitting something, else default then do our own raycast
			var inputRaycastDistance = VRInputModule.Instance?.Data == null
				? 0f : VRInputModule.Instance.Data.pointerCurrentRaycast.distance;

			var targetLength = inputRaycastDistance == 0 ? defaultLength : inputRaycastDistance;

			// Only collide with UI
			var endPosition = RaycastCamera.transform.position + RaycastCamera.transform.forward * targetLength;

			LaserPointerEnd.transform.position = endPosition;

			_line.transform.position = (transform.position + endPosition) / 2f;
			_line.transform.localScale = new Vector3(0.005f, (transform.position - endPosition).magnitude / 2f, 0.005f);
		}

		// Only show pointers when in use
		if (LaserPointerEnd.activeInHierarchy != IsDominantHand)
		{
			LaserPointerEnd.SetActive(IsDominantHand);
		}
		
		if (_line.activeInHierarchy != IsDominantHand)
		{
			_line.SetActive(IsDominantHand);
		}
	}
}
