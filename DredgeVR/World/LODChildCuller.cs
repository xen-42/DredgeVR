using UnityEngine;

namespace DredgeVR.World;

public class LODChildCuller : MonoBehaviour
{
	private bool _hidden;
	private ItemPOI _poi;

	private float _checkTimer;
	private float _timeBetweenChecks = 0.5f;

	public void Awake()
	{
		_poi = GetComponentInParent<ItemPOI>();

		_checkTimer = Random.Range(0, _timeBetweenChecks);
		CheckShouldCull();
	}

	public void Update()
	{
		// Instead of doing it every frame spread it out more
		_checkTimer += Time.deltaTime;
		if (_checkTimer > _timeBetweenChecks )
		{
			_checkTimer -= _timeBetweenChecks;
			CheckShouldCull();
		}
	}

	public void CheckShouldCull()
	{
		if (GameManager.Instance?.Player == null)
			return;

		// If we don't check the stock we might re-enable particles that aren't meant to be seen
		var shouldHide = (_poi != null && _poi.Stock == 0) || (GameManager.Instance.Player.transform.position - transform.position).sqrMagnitude > 1000;
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
