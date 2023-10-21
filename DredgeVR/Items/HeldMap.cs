using DredgeVR.VRUI;
using UnityEngine;
using Valve.VR;

namespace DredgeVR.Items;

public class HeldMap : MonoBehaviour
{
	public void Awake()
	{
		gameObject.AddComponent<UIHandAttachment>()
			.Init(SteamVR_Input_Sources.LeftHand, new Vector3(0, 90, 45), new Vector3(-0.05f, -0.15f, 0.15f), 0.5f);
	}

	public void OnEnable()
	{
		transform.Find("Container/Scrim").gameObject.SetActive(false);
		transform.Find("Container/Title").gameObject.SetActive(false);
	}
}
