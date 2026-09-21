using UnityEngine;

public class BuildStructureParticleEffect : BaseParticleEffect
{
	[SerializeField]
	private ParticleSystem _particles;

	public override void SetSize(Vector2Int p_size)
	{
		ParticleSystem.ShapeModule shape = _particles.shape;
		shape.scale = new Vector3(p_size.x, p_size.y, 1f);
		ParticleSystem.MainModule main = _particles.main;
		int num = (main.maxParticles = p_size.x * p_size.y + 5);
		ParticleSystem.EmissionModule emission = _particles.emission;
		emission.rateOverTime = new ParticleSystem.MinMaxCurve((float)num / 5f);
	}
}
