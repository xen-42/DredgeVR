using DredgeVR.VRUI;
using UnityEngine;
using Valve.VR;

namespace DredgeVR.Items;

public class HeldMap : MonoBehaviour
{
	public void Awake()
	{
		var uiHand = gameObject.AddComponent<UIHandAttachment>();
		uiHand.Init(false, new Vector3(0, 90, 45), new Vector3(0.05f, -0.1f, 0f), 0.5f);
		uiHand.smoothPosition = false;
	}

	public void SetOffset(int offsetX, int offsetZ)
	{
		var container = transform.Find("Container").transform;
		container.localPosition = new Vector3(offsetX, 0, offsetZ);
		container.localRotation = Quaternion.Euler(0, -45, 0);
	}

	public void OnEnable()
	{
		transform.Find("Container/Scrim").gameObject.SetActive(false);
		transform.Find("Container/Title")?.gameObject?.SetActive(false);
	}
}
