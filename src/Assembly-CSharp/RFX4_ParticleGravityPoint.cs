using UnityEngine;

[ExecuteInEditMode]
public class RFX4_ParticleGravityPoint : MonoBehaviour
{
	public Transform target;

	public float Force = 1f;

	public float StopDistance = -1f;

	private ParticleSystem ps;

	private ParticleSystem.Particle[] particles;

	private ParticleSystem.MainModule mainModule;

	private void Start()
	{
		ps = GetComponent<ParticleSystem>();
		mainModule = ps.main;
	}

	private void LateUpdate()
	{
		int maxParticles = mainModule.maxParticles;
		if (particles == null || particles.Length < maxParticles)
		{
			particles = new ParticleSystem.Particle[maxParticles];
		}
		int num = ps.GetParticles(particles);
		float num2 = Time.deltaTime * Force;
		Vector3 vector = Vector3.zero;
		if (mainModule.simulationSpace == ParticleSystemSimulationSpace.Local)
		{
			vector = base.transform.InverseTransformPoint(target.position);
		}
		if (mainModule.simulationSpace == ParticleSystemSimulationSpace.World)
		{
			vector = target.position;
		}
		for (int i = 0; i < num; i++)
		{
			Vector3 vector2 = Vector3.Normalize(vector - particles[i].position) * num2;
			if (StopDistance > 0f && (particles[i].position - target.position).magnitude < StopDistance)
			{
				particles[i].velocity = Vector3.zero;
			}
			else
			{
				particles[i].velocity += vector2;
			}
		}
		ps.SetParticles(particles, num);
	}
}
