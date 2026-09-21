using UnityEngine;

public class RFX4_PerPlatformSettings : MonoBehaviour
{
	public bool DisableOnMobiles;

	public bool RenderMobileDistortion;

	[Range(0.1f, 1f)]
	public float ParticleBudgetForMobiles = 0.5f;

	private bool isMobile;

	private void Start()
	{
	}

	private void Awake()
	{
		isMobile = IsMobilePlatform();
		if (isMobile)
		{
			if (DisableOnMobiles)
			{
				base.gameObject.SetActive(value: false);
			}
			else
			{
				ChangeParticlesBudget(ParticleBudgetForMobiles);
			}
		}
	}

	private void OnEnable()
	{
		Camera main = Camera.main;
		if (!(main == null))
		{
			if (RenderMobileDistortion && !DisableOnMobiles && isMobile && main.GetComponent<RFX4_MobileDistortion>() == null)
			{
				main.gameObject.AddComponent<RFX4_MobileDistortion>();
			}
			LWRP_Rendering();
		}
	}

	private void Update()
	{
		LWRP_Rendering();
	}

	private void LWRP_Rendering()
	{
	}

	private void OnDisable()
	{
		Camera main = Camera.main;
		if (!(main == null) && RenderMobileDistortion && !DisableOnMobiles && isMobile)
		{
			RFX4_MobileDistortion component = main.GetComponent<RFX4_MobileDistortion>();
			if (component != null)
			{
				Object.DestroyImmediate(component);
			}
		}
	}

	private bool IsMobilePlatform()
	{
		bool result = false;
		if (Application.isMobilePlatform)
		{
			result = true;
		}
		return result;
	}

	private void ChangeParticlesBudget(float particlesMul)
	{
		ParticleSystem component = GetComponent<ParticleSystem>();
		if (component == null)
		{
			return;
		}
		ParticleSystem.MainModule main = component.main;
		main.maxParticles = Mathf.Max(1, (int)((float)main.maxParticles * particlesMul));
		ParticleSystem.EmissionModule emission = component.emission;
		if (!emission.enabled)
		{
			return;
		}
		ParticleSystem.MinMaxCurve rateOverTime = emission.rateOverTime;
		if (rateOverTime.constantMin > 1f)
		{
			rateOverTime.constantMin *= particlesMul;
		}
		if (rateOverTime.constantMax > 1f)
		{
			rateOverTime.constantMax *= particlesMul;
		}
		emission.rateOverTime = rateOverTime;
		ParticleSystem.MinMaxCurve rateOverDistance = emission.rateOverDistance;
		if (rateOverDistance.constantMin > 1f)
		{
			if (rateOverDistance.constantMin > 1f)
			{
				rateOverDistance.constantMin *= particlesMul;
			}
			if (rateOverDistance.constantMax > 1f)
			{
				rateOverDistance.constantMax *= particlesMul;
			}
			emission.rateOverDistance = rateOverDistance;
		}
		ParticleSystem.Burst[] array = new ParticleSystem.Burst[emission.burstCount];
		emission.GetBursts(array);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].minCount > 1)
			{
				array[i].minCount = (short)((float)array[i].minCount * particlesMul);
			}
			if (array[i].maxCount > 1)
			{
				array[i].maxCount = (short)((float)array[i].maxCount * particlesMul);
			}
		}
		emission.SetBursts(array);
	}
}
