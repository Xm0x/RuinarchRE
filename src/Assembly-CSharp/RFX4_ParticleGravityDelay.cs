using UnityEngine;

public class RFX4_ParticleGravityDelay : MonoBehaviour
{
	public AnimationCurve GravityByTime = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float TimeMultiplier = 3f;

	[Space]
	public float GravityMultiplierMin = 1f;

	public float GravityMultiplierMax = 1f;

	private ParticleSystem.MainModule main;

	private float startTime;

	private float startMinGrav;

	private float startMaxGrav;

	private void Awake()
	{
		main = GetComponent<ParticleSystem>().main;
		startMinGrav = main.gravityModifier.constantMin;
		startMaxGrav = main.gravityModifier.constantMax;
	}

	private void OnEnable()
	{
		startTime = Time.time;
		ParticleSystem.MinMaxCurve gravityModifier = main.gravityModifier;
		gravityModifier.constantMin = startMinGrav;
		gravityModifier.constantMax = startMaxGrav;
		main.gravityModifier = gravityModifier;
	}

	private void Update()
	{
		float num = Time.time - startTime;
		if (num < TimeMultiplier)
		{
			ParticleSystem.MinMaxCurve gravityModifier = main.gravityModifier;
			float num2 = GravityByTime.Evaluate(num / TimeMultiplier);
			gravityModifier.constantMin = num2 * GravityMultiplierMin;
			gravityModifier.constantMax = num2 * GravityMultiplierMax;
			main.gravityModifier = gravityModifier;
		}
	}
}
