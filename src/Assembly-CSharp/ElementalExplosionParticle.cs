using UnityEngine;

public class ElementalExplosionParticle : MonoBehaviour
{
	public ParticleSystem coreExplosion;

	public ParticleSystem flameExplosion;

	public void SetColor(Color p_color)
	{
		ParticleSystem.MainModule main = coreExplosion.main;
		main.startColor = new ParticleSystem.MinMaxGradient(p_color);
		ParticleSystem.MainModule main2 = flameExplosion.main;
		main2.startColor = new ParticleSystem.MinMaxGradient(p_color);
	}
}
