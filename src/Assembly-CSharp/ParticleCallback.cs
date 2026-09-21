using System;
using UnityEngine;

public class ParticleCallback : MonoBehaviour
{
	private Action _onParticleStopped;

	public void SetAction(Action onParticleStopped)
	{
		_onParticleStopped = onParticleStopped;
	}

	private void OnParticleSystemStopped()
	{
		_onParticleStopped?.Invoke();
	}
}
