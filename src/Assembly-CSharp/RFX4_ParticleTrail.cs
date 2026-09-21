using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class RFX4_ParticleTrail : MonoBehaviour
{
	public GameObject Target;

	public Vector2 DefaultSizeMultiplayer = Vector2.one;

	public float VertexLifeTime = 2f;

	public float TrailLifeTime = 2f;

	public bool UseShaderMaterial;

	public Material TrailMaterial;

	public bool UseColorOverLifeTime;

	public Gradient ColorOverLifeTime = new Gradient();

	public RFX4_ShaderProperties colorShaderProperty;

	public float ColorLifeTime = 1f;

	public bool UseUvAnimation;

	public int TilesX = 4;

	public int TilesY = 4;

	public int FPS = 30;

	public bool IsLoop = true;

	[Range(0.001f, 1f)]
	public float MinVertexDistance = 0.01f;

	public bool GetVelocityFromParticleSystem;

	public float Gravity = 0.01f;

	public Vector3 Force = new Vector3(0f, 0.01f, 0f);

	public float InheritVelocity;

	public float Drag = 0.01f;

	[Range(0.001f, 10f)]
	public float Frequency = 1f;

	[Range(0.001f, 10f)]
	public float OffsetSpeed = 0.5f;

	public bool RandomTurbulenceOffset;

	[Range(0.001f, 10f)]
	public float Amplitude = 2f;

	public float TurbulenceStrength = 0.1f;

	public AnimationCurve VelocityByDistance = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);

	public float AproximatedFlyDistance = -1f;

	public bool SmoothCurves = true;

	private Dictionary<int, LineRenderer> dict = new Dictionary<int, LineRenderer>();

	private ParticleSystem ps;

	private ParticleSystem.Particle[] particles;

	private TrailRenderer[] trails;

	private Color psColor;

	private Transform targetT;

	private int layer;

	private bool isLocalSpace = true;

	private Transform t;

	private void OnEnable()
	{
		if (Target != null)
		{
			targetT = Target.transform;
		}
		ps = GetComponent<ParticleSystem>();
		t = base.transform;
		isLocalSpace = ps.main.simulationSpace == ParticleSystemSimulationSpace.Local;
		particles = new ParticleSystem.Particle[ps.main.maxParticles];
		if (TrailMaterial != null)
		{
			psColor = TrailMaterial.GetColor(TrailMaterial.HasProperty("_TintColor") ? "_TintColor" : "_Color");
		}
		layer = base.gameObject.layer;
		Update();
	}

	private void ClearTrails()
	{
		TrailRenderer[] array = trails;
		foreach (TrailRenderer trailRenderer in array)
		{
			if (trailRenderer != null)
			{
				Object.Destroy(trailRenderer.gameObject);
			}
		}
		trails = null;
	}

	private void Update()
	{
		if (dict.Count > 10)
		{
			RemoveEmptyTrails();
		}
		int num = ps.GetParticles(particles);
		for (int i = 0; i < num; i++)
		{
			int hashCode = particles[i].rotation3D.GetHashCode();
			if (!dict.ContainsKey(hashCode))
			{
				GameObject gameObject = new GameObject(hashCode.ToString());
				gameObject.transform.parent = base.transform;
				gameObject.transform.position = ps.transform.position;
				if (TrailLifeTime > 1E-05f)
				{
					Object.Destroy(gameObject, TrailLifeTime + VertexLifeTime);
				}
				gameObject.layer = layer;
				LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
				lineRenderer.startWidth = 0f;
				lineRenderer.endWidth = 0f;
				lineRenderer.sharedMaterial = TrailMaterial;
				lineRenderer.useWorldSpace = false;
				if (UseColorOverLifeTime)
				{
					RFX4_ShaderColorGradient rFX4_ShaderColorGradient = gameObject.AddComponent<RFX4_ShaderColorGradient>();
					rFX4_ShaderColorGradient.Color = ColorOverLifeTime;
					rFX4_ShaderColorGradient.TimeMultiplier = ColorLifeTime;
					rFX4_ShaderColorGradient.ShaderColorProperty = colorShaderProperty;
				}
				if (UseUvAnimation)
				{
					RFX4_UVAnimation rFX4_UVAnimation = gameObject.AddComponent<RFX4_UVAnimation>();
					rFX4_UVAnimation.TilesX = TilesX;
					rFX4_UVAnimation.TilesY = TilesY;
					rFX4_UVAnimation.FPS = FPS;
					rFX4_UVAnimation.IsLoop = IsLoop;
				}
				dict.Add(hashCode, lineRenderer);
				continue;
			}
			LineRenderer lineRenderer2 = dict[hashCode];
			if (!(lineRenderer2 == null))
			{
				if (!lineRenderer2.useWorldSpace)
				{
					lineRenderer2.useWorldSpace = true;
					InitTrailRenderer(lineRenderer2.gameObject);
				}
				Vector2 vector = DefaultSizeMultiplayer * particles[i].GetCurrentSize(ps);
				lineRenderer2.startWidth = vector.y;
				lineRenderer2.endWidth = vector.x;
				if (Target != null)
				{
					float num2 = 1f - particles[i].remainingLifetime / particles[i].startLifetime;
					Vector3 a = Vector3.Lerp(particles[i].position, targetT.position, num2);
					lineRenderer2.transform.position = Vector3.Lerp(a, targetT.position, Time.deltaTime * num2);
				}
				else
				{
					lineRenderer2.transform.position = (isLocalSpace ? ps.transform.TransformPoint(particles[i].position) : particles[i].position);
				}
				lineRenderer2.transform.rotation = t.rotation;
				Color32 currentColor = particles[i].GetCurrentColor(ps);
				Color endColor = (lineRenderer2.startColor = psColor * currentColor);
				lineRenderer2.endColor = endColor;
			}
		}
		ps.SetParticles(particles, num);
	}

	private void InitTrailRenderer(GameObject go)
	{
		RFX4_TrailRenderer rFX4_TrailRenderer = go.AddComponent<RFX4_TrailRenderer>();
		rFX4_TrailRenderer.Amplitude = Amplitude;
		rFX4_TrailRenderer.Drag = Drag;
		rFX4_TrailRenderer.Gravity = Gravity;
		rFX4_TrailRenderer.Force = Force;
		rFX4_TrailRenderer.Frequency = Frequency;
		rFX4_TrailRenderer.InheritVelocity = InheritVelocity;
		rFX4_TrailRenderer.VertexLifeTime = VertexLifeTime;
		rFX4_TrailRenderer.TrailLifeTime = TrailLifeTime;
		rFX4_TrailRenderer.MinVertexDistance = MinVertexDistance;
		rFX4_TrailRenderer.OffsetSpeed = OffsetSpeed;
		rFX4_TrailRenderer.SmoothCurves = SmoothCurves;
		rFX4_TrailRenderer.AproximatedFlyDistance = AproximatedFlyDistance;
		rFX4_TrailRenderer.VelocityByDistance = VelocityByDistance;
		rFX4_TrailRenderer.RandomTurbulenceOffset = RandomTurbulenceOffset;
		rFX4_TrailRenderer.TurbulenceStrength = TurbulenceStrength;
	}

	private void RemoveEmptyTrails()
	{
		for (int i = 0; i < dict.Count; i++)
		{
			KeyValuePair<int, LineRenderer> keyValuePair = dict.ElementAt(i);
			if (keyValuePair.Value == null)
			{
				dict.Remove(keyValuePair.Key);
			}
		}
	}

	private void OnDisable()
	{
		foreach (KeyValuePair<int, LineRenderer> item in dict)
		{
			if (item.Value != null)
			{
				Object.Destroy(item.Value.gameObject);
			}
		}
		dict.Clear();
	}
}
