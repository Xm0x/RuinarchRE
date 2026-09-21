using UnityEngine;

[RequireComponent(typeof(BaseParticleEffect))]
public abstract class ParticleCollisionListener : MonoBehaviour
{
	[SerializeField]
	protected BaseParticleEffect _baseParticleEffect;

	protected abstract void OnParticleCollision(GameObject other);
}
