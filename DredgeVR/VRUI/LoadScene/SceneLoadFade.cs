using DredgeVR.Helpers;
using DredgeVR.VRCamera;
using UnityEngine;

namespace DredgeVR.VRUI;


public class VRLoadingScene : MonoBehaviour
{
	private MeshRenderer _meshRenderer;
	private int _cameraLayerCache;

	private bool _isInLoadingScreen;

	private LoadingScreen _loadingScreen;

	public void Awake()
	{
		_loadingScreen = GetComponent<LoadingScreen>();

		gameObject.layer = LayerHelper.UI;

		var sphere = new GameObject("OcclusionSphere").SetParent(transform);

		var mf = sphere.AddComponent<MeshFilter>();
		mf.mesh = GeometryHelper.MakeMeshDoubleFaced(AssetLoader.PrimitiveSphere);

		_meshRenderer = sphere.AddComponent<MeshRenderer>();
		_meshRenderer.material = new Material(AssetLoader.UnlitShader)
		{
			mainTexture = Texture2D.blackTexture
		};

		sphere.transform.localScale = Vector3.one * 10f;

		HideLoadScreen();
	}

	public void Update()
	{
		// Hacky but works
		// Tried patching the fade method but it threw tons of weird errors
		var shouldShow = _loadingScreen.loadingScreenCanvasGroup.alpha == 1f;
		if (shouldShow != _isInLoadingScreen)
		{
			if (shouldShow)
			{
				ShowLoadScreen();
			}
			else
			{
				HideLoadScreen();
			}
		}
	}

	public void ShowLoadScreen()
	{
		if (!_isInLoadingScreen)
		{
			_isInLoadingScreen = true;
			_cameraLayerCache = VRCameraManager.LeftEye.Camera.cullingMask;
			VRCameraManager.LeftEye.Camera.cullingMask = 1 << LayerHelper.UI;
			VRCameraManager.RightEye.Camera.cullingMask = 1 << LayerHelper.UI;

			VRCameraManager.LeftEye.Camera.clearFlags = CameraClearFlags.Depth;
			VRCameraManager.RightEye.Camera.clearFlags = CameraClearFlags.Depth;

			_meshRenderer.forceRenderingOff = false;
		}
	}

	public void HideLoadScreen()
	{
		if (_isInLoadingScreen)
		{
			_isInLoadingScreen = false;
			VRCameraManager.LeftEye.Camera.cullingMask = _cameraLayerCache;
			VRCameraManager.RightEye.Camera.cullingMask = _cameraLayerCache;

			VRCameraManager.LeftEye.Camera.clearFlags = CameraClearFlags.Skybox;
			VRCameraManager.RightEye.Camera.clearFlags = CameraClearFlags.Skybox;

			_meshRenderer.forceRenderingOff = true;
		}
	}
}
