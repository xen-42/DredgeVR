using UnityEngine;

namespace DredgeVR.World;

public class LODChildCuller : MonoBehaviour
{
	private bool _hidden;

	public void Update()
	{
		if (GameManager.Instance?.Player == null)
			return;

		var shouldHide = (GameManager.Instance.Player.transform.position - transform.position).sqrMagnitude > 1000;
		if (shouldHide != _hidden)
		{
			foreach (Transform transform in gameObject.transform)
			{
				transform.gameObject.SetActive(!shouldHide);
			}
			_hidden = shouldHide;
		}
	}
}
