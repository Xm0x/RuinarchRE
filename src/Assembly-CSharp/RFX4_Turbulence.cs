using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystem))]
public class RFX4_Turbulence : MonoBehaviour
{
	public enum MoveMethodEnum
	{
		Position,
		Velocity,
		RelativePosition
	}

	public enum PerfomanceEnum
	{
		High,
		Low
	}

	public float TurbulenceStrenght = 1f;

	public bool TurbulenceByTime;

	public float TimeDelay;

	public AnimationCurve TurbulenceStrengthByTime = AnimationCurve.EaseInOut(1f, 1f, 1f, 1f);

	public Vector3 Frequency = new Vector3(1f, 1f, 1f);

	public Vector3 OffsetSpeed = new Vector3(0.5f, 0.5f, 0.5f);

	public Vector3 Amplitude = new Vector3(5f, 5f, 5f);

	public Vector3 GlobalForce;

	public bool UseGlobalOffset = true;

	public MoveMethodEnum MoveMethod;

	public PerfomanceEnum Perfomance;

	public float ThreshholdSpeed;

	public AnimationCurve VelocityByDistance = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);

	public float AproximatedFlyDistance = -1f;

	private float lastStopTime;

	private Vector3 currentOffset;

	private float deltaTime;

	private float deltaTimeLastUpdateOffset;

	private ParticleSystem.Particle[] particleArray;

	private ParticleSystem particleSys;

	private float time;

	private int currentSplit;

	private float fpsTime;

	private int FPS;

	private int splitUpdate = 2;

	private PerfomanceEnum perfomanceOldSettings;

	private bool skipFrame;

	private Transform t;

	private float currentDelay;

	private void Start()
	{
		t = base.transform;
		particleSys = GetComponent<ParticleSystem>();
		if (particleArray == null || particleArray.Length < particleSys.main.maxParticles)
		{
			particleArray = new ParticleSystem.Particle[particleSys.main.maxParticles];
		}
		perfomanceOldSettings = Perfomance;
		UpdatePerfomanceSettings();
	}

	private void OnEnable()
	{
		currentDelay = 0f;
	}

	private void Update()
	{
		if (!Application.isPlaying)
		{
			deltaTime = Time.realtimeSinceStartup - lastStopTime;
			lastStopTime = Time.realtimeSinceStartup;
		}
		else
		{
			deltaTime = Time.deltaTime;
		}
		currentDelay += deltaTime;
		if (currentDelay < TimeDelay)
		{
			return;
		}
		if (!UseGlobalOffset)
		{
			currentOffset += OffsetSpeed * deltaTime;
		}
		else if (Application.isPlaying)
		{
			currentOffset = OffsetSpeed * Time.time;
		}
		else
		{
			currentOffset = OffsetSpeed * Time.realtimeSinceStartup;
		}
		if (Perfomance != perfomanceOldSettings)
		{
			perfomanceOldSettings = Perfomance;
			UpdatePerfomanceSettings();
		}
		time += deltaTime;
		if (QualitySettings.vSyncCount == 2)
		{
			UpdateTurbulence();
		}
		else if (QualitySettings.vSyncCount == 1)
		{
			if (Perfomance == PerfomanceEnum.Low)
			{
				if (skipFrame)
				{
					UpdateTurbulence();
				}
				skipFrame = !skipFrame;
			}
			if (Perfomance == PerfomanceEnum.High)
			{
				UpdateTurbulence();
			}
		}
		else if (QualitySettings.vSyncCount == 0)
		{
			if (time >= fpsTime)
			{
				time = 0f;
				UpdateTurbulence();
				deltaTimeLastUpdateOffset = 0f;
			}
			else
			{
				deltaTimeLastUpdateOffset += deltaTime;
			}
		}
	}

	private void UpdatePerfomanceSettings()
	{
		if (Perfomance == PerfomanceEnum.High)
		{
			FPS = 80;
			splitUpdate = 2;
		}
		if (Perfomance == PerfomanceEnum.Low)
		{
			FPS = 40;
			splitUpdate = 2;
		}
		fpsTime = 1f / (float)FPS;
	}

	private void UpdateTurbulence()
	{
		int particles = particleSys.GetParticles(particleArray);
		int num = 1;
		int num2;
		int num3;
		if (splitUpdate > 1)
		{
			num2 = particles / splitUpdate * currentSplit;
			num3 = Mathf.CeilToInt((float)particles * 1f / (float)splitUpdate * ((float)currentSplit + 1f));
			num = splitUpdate;
		}
		else
		{
			num2 = 0;
			num3 = particles;
		}
		for (int i = num2; i < num3; i++)
		{
			ParticleSystem.Particle particle = particleArray[i];
			float num4 = 1f;
			if (TurbulenceByTime)
			{
				num4 = TurbulenceStrengthByTime.Evaluate(1f - particle.remainingLifetime / particle.startLifetime);
			}
			if (ThreshholdSpeed > 1E-07f && num4 < ThreshholdSpeed)
			{
				return;
			}
			Vector3 position = particle.position;
			position.x /= Frequency.x + 1E-07f;
			position.y /= Frequency.y + 1E-07f;
			position.z /= Frequency.z + 1E-07f;
			Vector3 vector = default(Vector3);
			float num5 = deltaTime + deltaTimeLastUpdateOffset;
			vector.x = (Mathf.PerlinNoise(position.z - currentOffset.z, position.y - currentOffset.y) * 2f - 1f) * Amplitude.x * num5;
			vector.y = (Mathf.PerlinNoise(position.x - currentOffset.x, position.z - currentOffset.z) * 2f - 1f) * Amplitude.y * num5;
			vector.z = (Mathf.PerlinNoise(position.y - currentOffset.y, position.x - currentOffset.x) * 2f - 1f) * Amplitude.z * num5;
			float num6 = TurbulenceStrenght * num4 * (float)num;
			float num7 = 1f;
			float num8 = Mathf.Abs((particle.position - t.position).magnitude);
			if (AproximatedFlyDistance > 0f)
			{
				num7 = VelocityByDistance.Evaluate(Mathf.Clamp01(num8 / AproximatedFlyDistance));
			}
			vector *= num6;
			if (MoveMethod == MoveMethodEnum.Position)
			{
				particleArray[i].position += vector * num7;
			}
			if (MoveMethod == MoveMethodEnum.Velocity)
			{
				particleArray[i].velocity += vector * num7;
			}
			if (MoveMethod == MoveMethodEnum.RelativePosition)
			{
				particleArray[i].position += vector * particleArray[i].velocity.magnitude;
				particleArray[i].velocity = particleArray[i].velocity * 0.85f + vector.normalized * 0.15f * num7 + GlobalForce * num7;
			}
		}
		particleSys.SetParticles(particleArray, particles);
		currentSplit++;
		if (currentSplit >= splitUpdate)
		{
			currentSplit = 0;
		}
	}
}
