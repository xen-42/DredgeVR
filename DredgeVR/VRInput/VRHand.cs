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
	private GameObject _lineEnd;
	private bool _graphicsInitialized;
	private static bool _flagTitleSceneAcquire;

	public float defaultLength = 5.0f;
	public SteamVR_Input_Sources hand;

	public bool IsDominantHand { get; private set; }

	public void Start()
	{
		// Loading buoys off the title screen
		// Probably should move this to a bundle but idc
		DredgeVRCore.TitleSceneStart += AddBuoyGraphics;

		var handSkeleton = gameObject.AddComponent<SteamVR_Behaviour_Skeleton>();
		handSkeleton.inputSource = hand;
		handSkeleton.skeletonRoot = transform;
		handSkeleton.enabled = true;

		RaycastCamera = gameObject.AddComponent<Camera>();
		RaycastCamera.nearClipPlane = 0.01f;
		RaycastCamera.farClipPlane = 1000f;
		RaycastCamera.enabled = false;

		_lineEnd = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		Component.DestroyImmediate(_lineEnd.GetComponent<SphereCollider>());
		_lineEnd.transform.parent = transform;
		_lineEnd.transform.localScale = Vector3.one * 0.025f;
		_lineEnd.name = "Dot";

		// Tried using a line renderer for this but it did not behave in VR
		_line = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		Component.DestroyImmediate(_line.GetComponent<Collider>());
		_line.transform.parent = transform;
		_line.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
		_line.name = "Line";

		// Disable rendering until the graphics are initialized
		_lineEnd.SetActive(false);
		_line.SetActive(false);

		VRInputModule.Instance.DominantHandChanged += OnDominantHandChanged;
		OnDominantHandChanged(VRInputModule.Instance.DominantHand);
	}

	private void OnDominantHandChanged(SteamVR_Input_Sources dominantHand)
	{
		IsDominantHand = dominantHand == hand;
	}

	/// <summary>
	/// First time the title scene loads we'll steal some buoys and shaders for our hand graphics
	/// </summary>
	private void AddBuoyGraphics()
	{
		DredgeVRCore.TitleSceneStart -= AddBuoyGraphics;

		var buoy = GameObject.Instantiate(GameObject.Find("TheMarrows/Islands/GreaterMarrow/Buoys/Buoy/LightBuoy"));
		buoy.transform.parent = transform;
		buoy.transform.localPosition = Vector3.zero;
		buoy.transform.localRotation = Quaternion.identity;
		buoy.transform.localScale = Vector3.one * 0.1f;

		// Need a fresh material for our laser pointer
		var material = new Material(buoy.GetComponent<MeshRenderer>().material.shader);
		_line.GetComponent<MeshRenderer>().material = material;
		_lineEnd.GetComponent<MeshRenderer>().material = material;

		_graphicsInitialized = true;

		// Need to keep the addressables loaded else we lose our buoys
		if (!_flagTitleSceneAcquire)
		{
			_flagTitleSceneAcquire = true;
			DredgeVRCore.Instance.StartCoroutine(KeepTitleSceneAddressablesLoaded());
		}
	}

	private IEnumerator KeepTitleSceneAddressablesLoaded()
	{
		var time = 0f;
		var failed = true;
		while (failed)
		{
			if (time > 10f)
			{
				WinchCore.Log.Error($"Couldn't keep title scene addressables loaded, timed out after 10 seconds");
				yield return null;
			}

			try
			{
				new ResourceManager().Acquire(GameManager.Instance._sceneLoader.titleSceneHandle);
				failed = false;
			}
			catch { }

			time += 0.5f;
			yield return new WaitForSeconds(0.5f);
		}
		yield return null;
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
			var endPosition = transform.position + transform.forward * targetLength;

			_lineEnd.transform.position = endPosition;

			_line.transform.position = (transform.position + endPosition) / 2f;
			_line.transform.localScale = new Vector3(0.005f, (transform.position - endPosition).magnitude / 2f, 0.005f);
		}

		// Only show pointers when in use
		if (_lineEnd.activeInHierarchy != IsDominantHand)
		{
			_lineEnd.SetActive(IsDominantHand);
		}
		
		if (_line.activeInHierarchy != IsDominantHand)
		{
			_line.SetActive(IsDominantHand);
		}
	}
}
