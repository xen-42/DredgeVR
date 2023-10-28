using DredgeVR.Helpers;
using UnityEngine;
using Valve.VR;

namespace DredgeVR.VRInput;

public class VRHand : MonoBehaviour
{
	public Camera RaycastCamera { get; private set; }
	private GameObject _line, _fadedLine;
	public GameObject LaserPointerEnd { get; private set; }

	public float defaultLength = 0.5f;
	public SteamVR_Input_Sources hand;

	public bool IsHoveringUI { get; private set; }

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

		_fadedLine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		Component.DestroyImmediate(_fadedLine.GetComponent<Collider>());
		_fadedLine.transform.parent = RaycastCamera.transform;
		_fadedLine.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
		_fadedLine.name = "FadedLine";

		InitGraphics();

		VRInputModule.Instance.DominantHandChanged += OnDominantHandChanged;
		OnDominantHandChanged(VRInputModule.Instance.DominantHandInputSource);
	}

	private void OnDominantHandChanged(SteamVR_Input_Sources dominantHand)
	{
		IsDominantHand = dominantHand == hand;

		if (LaserPointerEnd.activeInHierarchy != IsDominantHand)
		{
			LaserPointerEnd.SetActive(IsDominantHand);
		}

		if (_line.activeInHierarchy != IsDominantHand)
		{
			_line.SetActive(IsDominantHand);
		}

		if (_fadedLine.activeInHierarchy != IsDominantHand)
		{
			_fadedLine.SetActive(IsDominantHand);
		}
	}

	private void InitGraphics()
	{
		// Need a fresh material for our laser pointer
		var material = new Material(AssetLoader.LitShader);

		var lineMR = _line.GetComponent<MeshRenderer>();
		lineMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		lineMR.sharedMaterial = material;

		var fadedLineMR = _fadedLine.GetComponent<MeshRenderer>();
		fadedLineMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		fadedLineMR.material = material;

		var endMR = LaserPointerEnd.GetComponent<MeshRenderer>();
		endMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		endMR.sharedMaterial = material;
	}

	public void Update()
	{
		IsHoveringUI = false;

		if (IsDominantHand)
		{
			// Use raycast from input data if it's hitting something, else default then do our own raycast
			var inputRaycastDistance = VRInputModule.Instance?.Data == null
				? 0f : VRInputModule.Instance.Data.pointerCurrentRaycast.distance;

			IsHoveringUI = inputRaycastDistance > 0;

			var targetLength = IsHoveringUI ? inputRaycastDistance : defaultLength;

			// Only collide with UI
			var endPosition = RaycastCamera.transform.position + RaycastCamera.transform.forward * targetLength;

			if (IsHoveringUI)
			{
				_line.transform.position = (transform.position + endPosition) / 2f;
				_line.transform.localScale = new Vector3(0.005f, (transform.position - endPosition).magnitude / 2f, 0.005f);

				LaserPointerEnd.transform.position = endPosition;
			}
			else
			{
				_fadedLine.transform.position = (transform.position + endPosition) / 2f;
				_fadedLine.transform.localScale = new Vector3(0.001f, (transform.position - endPosition).magnitude / 2f, 0.001f);
			}

			// Should show when colliding with UI
			if (_line.activeInHierarchy != IsHoveringUI)
			{
				_line.gameObject.SetActive(IsHoveringUI);
			}
			if (LaserPointerEnd.activeInHierarchy != IsHoveringUI)
			{
				LaserPointerEnd.gameObject.SetActive(IsHoveringUI);
			}

			// Should show when not colliding with UI
			if (_fadedLine.activeInHierarchy == IsHoveringUI)
			{
				_fadedLine.gameObject.SetActive(!IsHoveringUI);
			}
		}
	}
}
