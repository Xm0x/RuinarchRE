using UnityEngine;

public class DestroyStructureParticleEffect : BaseParticleEffect
{
	[SerializeField]
	private ParticleSystem _particles;

	public override void SetSize(Vector2Int p_size)
	{
		ParticleSystem.ShapeModule shape = _particles.shape;
		shape.scale = new Vector3(p_size.x, p_size.y, 1f);
		ParticleSystem.MainModule main = _particles.main;
		int maxParticles = p_size.x * p_size.y + 50;
		main.maxParticles = maxParticles;
		ParticleSystem.EmissionModule emission = _particles.emission;
		emission.burstCount = p_size.x * p_size.y + 50;
	}

	public override void Reset()
	{
		base.Reset();
		SetSize(new Vector2Int(5, 5));
	}
}
