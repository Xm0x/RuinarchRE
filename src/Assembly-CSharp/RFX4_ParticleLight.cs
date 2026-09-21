using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystem))]
public class RFX4_ParticleLight : MonoBehaviour
{
	public float LightIntencityMultiplayer = 1f;

	public bool UseShadows;

	public int LightsLimit = 10;

	private ParticleSystem ps;

	private ParticleSystem.Particle[] particles;

	private Light[] lights;

	private bool isLocalSpace;

	private void Start()
	{
		ps = GetComponent<ParticleSystem>();
		ParticleSystem.MainModule main = ps.main;
		if (main.maxParticles > LightsLimit)
		{
			main.maxParticles = LightsLimit;
		}
		particles = new ParticleSystem.Particle[main.maxParticles];
		isLocalSpace = ps.main.simulationSpace == ParticleSystemSimulationSpace.Local;
		lights = new Light[main.maxParticles];
		for (int i = 0; i < lights.Length; i++)
		{
			GameObject gameObject = new GameObject($"ParticleLight{i}");
			gameObject.hideFlags = HideFlags.DontSave;
			lights[i] = gameObject.AddComponent<Light>();
			lights[i].transform.parent = base.transform;
			lights[i].intensity = 0f;
			lights[i].shadows = (UseShadows ? LightShadows.Soft : LightShadows.None);
		}
	}

	private void Update()
	{
		int num = ps.GetParticles(particles);
		for (int i = 0; i < num; i++)
		{
			lights[i].gameObject.SetActive(value: true);
			lights[i].transform.position = (isLocalSpace ? ps.transform.TransformPoint(particles[i].position) : particles[i].position);
			lights[i].color = particles[i].GetCurrentColor(ps);
			lights[i].range = particles[i].GetCurrentSize(ps);
			lights[i].intensity = (float)(int)particles[i].GetCurrentColor(ps).a / 255f * LightIntencityMultiplayer;
			lights[i].shadows = (UseShadows ? LightShadows.Soft : LightShadows.None);
			if (lights[i].intensity < 0.01f)
			{
				lights[i].gameObject.SetActive(value: false);
			}
		}
		for (int j = num; j < particles.Length; j++)
		{
			lights[j].gameObject.SetActive(value: false);
		}
	}
}
