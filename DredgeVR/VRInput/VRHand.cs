using UnityEngine;
using Valve.VR;

namespace DredgeVR.VRInput;

internal class VRHand : MonoBehaviour
{
	private Camera _camera;
	private LineRenderer _lineRenderer;
	private GameObject _lineEnd;

	public float defaultLength = 5.0f;
	public SteamVR_Input_Sources hand;

	public void Start()
	{
		// Loading buoys off the title screen
		// Probably should move this to a bundle but idc
		DredgeVRCore.TitleSceneStart += AddBuoyGraphics;

		var handSkeleton = gameObject.AddComponent<SteamVR_Behaviour_Skeleton>();
		handSkeleton.inputSource = hand;
		handSkeleton.skeletonRoot = transform;
		handSkeleton.enabled = true;

		_camera = gameObject.AddComponent<Camera>();
		_camera.nearClipPlane = 0.01f;
		_camera.farClipPlane = 1000f;
		_camera.enabled = false;

		_lineRenderer = gameObject.AddComponent<LineRenderer>();
		_lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		_lineRenderer.allowOcclusionWhenDynamic = false;
		_lineRenderer.endColor = Color.white;
		_lineRenderer.startWidth = 0.01f;
		_lineRenderer.alignment = LineAlignment.TransformZ;

		_lineEnd = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		Component.DestroyImmediate(_lineEnd.GetComponent<SphereCollider>());
		_lineEnd.transform.parent = transform;
		_lineRenderer.transform.localPosition = Vector3.zero;
		_lineEnd.transform.localScale = Vector3.one * 0.025f;
		_lineEnd.name = "Dot";
	}

	private void AddBuoyGraphics()
	{
		DredgeVRCore.TitleSceneStart -= AddBuoyGraphics;

		var buoy = GameObject.Instantiate(GameObject.Find("TheMarrows/Islands/GreaterMarrow/Buoys/Buoy/LightBuoy"));
		buoy.transform.parent = transform;
		buoy.transform.localPosition = Vector3.zero;
		buoy.transform.localRotation = Quaternion.identity;
		buoy.transform.localScale = Vector3.one * 0.1f;

		var material = new Material(buoy.GetComponentInChildren<MeshRenderer>().material.shader);
		_lineRenderer.material = material;
		_lineEnd.GetComponent<MeshRenderer>().material = material;
	}

	public void Update()
	{
		var targetLength = defaultLength;

		var hit = CreateRaycast(targetLength);

		var endPosition = hit.collider != null ? hit.point : transform.position + (transform.forward * (targetLength - 0.1f));

		_lineEnd.transform.position = endPosition;

		_lineRenderer.SetPosition(0, transform.position);
		_lineRenderer.SetPosition(1, endPosition);
	}

	private RaycastHit CreateRaycast(float targetLength)
	{
		var ray = new Ray(transform.position, transform.forward);
		Physics.Raycast(ray, out var hit, targetLength);

		return hit;
	}

}
