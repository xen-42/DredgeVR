using DredgeVR.Helpers;
using System.Collections;
using UnityAsyncAwaitUtil;
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

	private Animator _animator;
	private SteamVR_Behaviour_Skeleton _skeleton;

	public bool IsHoveringUI { get; private set; }

	public bool IsDominantHand { get; private set; }

	public void Start()
	{
		_animator = gameObject.GetComponentInChildren<Animator>();
		_skeleton = gameObject.GetComponent<SteamVR_Behaviour_Skeleton>();

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

	public void StopHolding()
	{
		_animator.enabled = true;
		_skeleton.enabled = true;
	}

	public void HoldCompass()
	{
		StartCoroutine(HoldCompassCoroutine());
	}

	private IEnumerator HoldCompassCoroutine()
	{
		// Skeleton has to go first so it can set the hand back to its default pose
		_skeleton.enabled = false;

		// Wait a frame for it to do that
		yield return null;

		_animator.enabled = false;

		var root = transform.Find("vr_glove_model/Root");

		var thumb = root.Find("wrist_r/finger_thumb_0_r");
		thumb.localRotation = Quaternion.Euler(357.9772f, 262.6017f, 45.038f);
		var thumb1 = thumb.Find("finger_thumb_1_r");
		thumb1.localRotation = Quaternion.Euler(8.0801f, 357.4013f, 352.7511f);
		var thumb2 = thumb1.Find("finger_thumb_2_r");
		thumb2.localRotation = Quaternion.Euler(0.1066f, 357.7797f, 308.3767f);

		var index = root.Find("wrist_r/finger_index_meta_r");
		index.localRotation = Quaternion.Euler(289.9999f, 109.2893f, 166.4915f);
		var index1 = index.Find("finger_index_0_r/finger_index_1_r");
		index1.localRotation = Quaternion.Euler(5.2787f, 344.9173f, 325.3578f);
		var index2 = index1.Find("finger_index_2_r");
		index2.localRotation = Quaternion.Euler(0.2266f, 2.6098f, 305.5387f);

		var middle = root.Find("wrist_r/finger_middle_meta_r/finger_middle_0_r");
		middle.localRotation = Quaternion.Euler(345.483f, 10.3437f, 328.0566f);
		var middle1 = middle.Find("finger_middle_1_r");
		middle1.localRotation = Quaternion.Euler(1.9814f, 358.5891f, 341.1627f);
		var middle2 = middle1.Find("finger_middle_2_r");
		middle2.localRotation = Quaternion.Euler(359.5896f, 2.748f, 306.5181f);

		var ring1 = root.Find("wrist_r/finger_ring_meta_r/finger_ring_0_r/finger_ring_1_r");
		ring1.localRotation = Quaternion.Euler(359.7631f, 0.0196f, 341.575f);
		var ring2 = ring1.Find("finger_ring_2_r");
		ring2.localRotation = Quaternion.Euler(0.0271f, 1.4612f, 314.5654f);

		var pinky = root.Find("wrist_r/finger_pinky_meta_r");
		pinky.localRotation = Quaternion.Euler(295.442f, 39.8577f, 228.2713f);
		var pinky2 = pinky.Find("finger_pinky_0_r/finger_pinky_1_r/finger_pinky_2_r");
		pinky2.localRotation = Quaternion.Euler(0.5303f, 5.913f, 303.5219f);
	}

	public void HoldPaper()
	{
		StartCoroutine(HoldPaperCoroutine());
	}

	private IEnumerator HoldPaperCoroutine()
	{
		// Skeleton has to go first so it can set the hand back to its default pose
		_skeleton.enabled = false;

		// Wait a frame for it to do that
		yield return null;

		_animator.enabled = false;

		var root = transform.Find("vr_glove_model/Root");

		var wrist = root.Find("wrist_r");
		wrist.localRotation = Quaternion.Euler(329.2117f, 183.1089f, 44.982f);
	}
}
