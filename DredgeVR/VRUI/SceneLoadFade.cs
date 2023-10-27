using DredgeVR.Helpers;
using UnityEngine;

namespace DredgeVR.VRUI;

public class SceneLoadFade : MonoBehaviour
{
	private Material _material;
	private float _targetAlpha;

	public void Awake()
	{
		var mf = gameObject.AddComponent<MeshFilter>();
		mf.mesh = GeometryHelper.MakeMeshDoubleFaced(AssetLoader.PrimitiveSphere);

		var mr = gameObject.AddComponent<MeshRenderer>();
		_material = new Material(AssetLoader.LitShader);
		_material.color = new Color(0f, 0f, 0f, _targetAlpha);
		mr.material = _material;

		DredgeVRCore.SceneStart += OnSceneStart;
		DredgeVRCore.SceneUnloaded += OnSceneUnloaded;
	}

	public void OnDestroy()
	{
		DredgeVRCore.SceneStart -= OnSceneStart;
		DredgeVRCore.SceneUnloaded -= OnSceneUnloaded;
	}

	private void OnSceneStart(string name)
	{
		FadeOut(false);
	}

	private void OnSceneUnloaded(string name)
	{
		FadeIn(true);
	}

	public void Update()
	{
		if (_material.color.a != _targetAlpha)
		{
			var dir = Mathf.Sign(_targetAlpha - _material.color.a);
			var newAlpha = Mathf.Clamp01(_material.color.a + (dir * Time.fixedDeltaTime));
			SetAlpha(newAlpha);
		}
	}

	private void SetAlpha(float alpha)
	{
		_material.color = new Color(_material.color.r, _material.color.g, _material.color.b, alpha);
	}

	public void FadeIn(bool instant)
	{
		_targetAlpha = 1f;
		if (instant)
		{
			SetAlpha(_targetAlpha);
		}
	}
	public void FadeOut(bool instant)
	{
		_targetAlpha = 0f;
		if (instant)
		{
			SetAlpha(_targetAlpha);
		}
	}
}
