using EZObjectPools;
using UnityEngine;

public class ParticleEffectCallback : PooledObject
{
	public ParticleSystem particle;

	public void OnParticleSystemStopped()
	{
		Messenger.Broadcast(ParticleSignals.PARTICLE_EFFECT_DONE, particle);
	}
}
