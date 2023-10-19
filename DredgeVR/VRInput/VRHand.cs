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
	private static bool _flagTitleSceneAcquire;

	public float defaultLength = 5.0f;
	public SteamVR_Input_Sources hand;

	public bool IsDominantHand { get; private set; }

	public static VRHand DominantHand { get; private set; }

	public void Start()
	{
		// Loading buoys off the title screen
		// Probably should move this to a bundle but idc
		DredgeVRCore.TitleSceneStart += InitGraphics;

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
		OnDominantHandChanged(VRInputModule.Instance.DominantHand);
	}

	private void OnDominantHandChanged(SteamVR_Input_Sources dominantHand)
	{
		IsDominantHand = dominantHand == hand;
		DominantHand = this;
	}

	/// <summary>
	/// First time the title scene loads we'll steal some buoys and shaders for our hand graphics
	/// </summary>
	private void InitGraphics()
	{
		DredgeVRCore.TitleSceneStart -= InitGraphics;

		/*
		var buoy = GameObject.Instantiate(GameObject.Find("TheMarrows/Islands/GreaterMarrow/Buoys/Buoy/LightBuoy"));
		buoy.transform.parent = transform;
		buoy.transform.localPosition = Vector3.zero;
		buoy.transform.localRotation = Quaternion.identity;
		buoy.transform.localScale = Vector3.one * 0.1f;

		// Need to keep the addressables loaded else we lose our buoys
		if (!_flagTitleSceneAcquire)
		{
			_flagTitleSceneAcquire = true;
			DredgeVRCore.Instance.StartCoroutine(KeepTitleSceneAddressablesLoaded());
		}
		*/

		// Need a fresh material for our laser pointer
		var material = new Material(AssetLoader.LitShader);

		var lineMR = _line.GetComponent<MeshRenderer>();
		lineMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		lineMR.material = material;

		var endMR = LaserPointerEnd.GetComponent<MeshRenderer>();
		endMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		endMR.material = material;

		_graphicsInitialized = true;
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
