using DredgeVR.VRCamera;
using UnityEngine;

namespace DredgeVR.World;

public class LookAtPlayer : MonoBehaviour
{
	private ParticleSystem _particles;

	public void Awake()
	{
		_particles = GetComponent<ParticleSystem>();
	}

	public void Update()
	{
		if (_particles != null && !_particles.isEmitting)
		{
			return;
		}

		transform.LookAt(VRCameraManager.Instance.transform);
	}
}
