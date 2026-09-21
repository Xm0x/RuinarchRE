using System.Collections;
using UnityEngine;

public class RFX4_EffectSettings : MonoBehaviour
{
	[Range(0.1f, 1f)]
	public float ParticlesBudget = 1f;

	public bool UseLightShadows;

	public bool UseFastFlatDecalsForMobiles = true;

	public bool UseCustomColor;

	public Color EffectColor = Color.red;

	public bool IsVisible = true;

	public float FadeoutTime = 1.5f;

	public bool UseCollisionDetection = true;

	public bool LimitMaxDistance;

	public float MaxDistnace = -1f;

	public float Mass = 1f;

	public float Speed = 10f;

	public float AirDrag = 0.1f;

	public bool UseGravity = true;

	private const string distortionNamePC = "KriptoFX/RFX4/Distortion";

	private const string distortionNameMobile = "KriptoFX/RFX4/DistortionMobile";

	private bool isCheckedDistortion;

	private bool prevIsVisible;

	private float currentFadeoutTime;

	private Renderer[] renderers;

	private Renderer[] skinRenderers;

	private Light[] lights;

	private ParticleSystem[] particleSystems;

	private AudioSource[] audioSources;

	private string[] colorProperties = new string[3] { "_TintColor", "_Color", "_MainColor" };

	private void Awake()
	{
		prevIsVisible = IsVisible;
		CacheRenderers();
	}

	private void OnEnable()
	{
		if (ParticlesBudget < 0.99f)
		{
			ChangeParticlesBudget(ParticlesBudget);
		}
		if (UseCustomColor)
		{
			ChangeParticleColor();
		}
		if (UseFastFlatDecalsForMobiles)
		{
			SetFlatDecals();
		}
		if (!UseLightShadows || IsMobilePlatform())
		{
			DisableShadows();
		}
	}

	private void Update()
	{
		if (prevIsVisible != IsVisible)
		{
			prevIsVisible = IsVisible;
			if (!IsVisible)
			{
				StartCoroutine(Fadeout());
			}
			else
			{
				Fadein();
			}
		}
	}

	private void ChangeParticlesBudget(float particlesMul)
	{
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		foreach (ParticleSystem obj in componentsInChildren)
		{
			ParticleSystem.MainModule main = obj.main;
			main.maxParticles = Mathf.Max(1, (int)((float)main.maxParticles * particlesMul));
			ParticleSystem.EmissionModule emission = obj.emission;
			if (!emission.enabled)
			{
				continue;
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
		}
	}

	public void ChangeParticleColor()
	{
		Debug.Log("ColorChanged");
		float h = RFX4_ColorHelper.ColorToHSV(EffectColor).H;
		RFX4_ColorHelper.ChangeObjectColorByHUE(base.gameObject, h);
		RFX4_PhysicsMotion componentInChildren = GetComponentInChildren<RFX4_PhysicsMotion>();
		if (componentInChildren != null)
		{
			componentInChildren.HUE = h;
		}
		RFX4_RaycastCollision componentInChildren2 = GetComponentInChildren<RFX4_RaycastCollision>();
		if (componentInChildren2 != null)
		{
			componentInChildren2.HUE = h;
		}
	}

	public void SetFlatDecals()
	{
		RFX4_Decal[] componentsInChildren = GetComponentsInChildren<RFX4_Decal>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].IsScreenSpace = false;
		}
	}

	public void DisableShadows()
	{
		Light[] componentsInChildren = GetComponentsInChildren<Light>();
		foreach (Light obj in componentsInChildren)
		{
			RFX4_LightCurves component = obj.GetComponent<RFX4_LightCurves>();
			if (component != null && component.UseShadowsIfPossible)
			{
				component.UseShadowsIfPossible = false;
			}
			obj.shadows = LightShadows.None;
		}
		RFX4_ParticleLight[] componentsInChildren2 = GetComponentsInChildren<RFX4_ParticleLight>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].UseShadows = false;
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

	private IEnumerator Fadeout()
	{
		currentFadeoutTime = Time.time;
		while (Time.time - currentFadeoutTime < FadeoutTime)
		{
			ChangeAlphaFade();
			yield return new WaitForSeconds(1f / 30f);
		}
	}

	private void UpdateAlphaByProperties(Material mat, float overrideAlpha = -1f)
	{
		string[] array = colorProperties;
		foreach (string text in array)
		{
			if (mat.HasProperty(text))
			{
				Color color = mat.GetColor(text);
				if (overrideAlpha > -0.5f)
				{
					color.a = overrideAlpha;
				}
				else
				{
					color.a -= 1f / 30f / FadeoutTime;
				}
				mat.SetColor(text, color);
			}
		}
	}

	private void ChangeAlphaFade()
	{
		Renderer[] array = renderers;
		foreach (Renderer renderer in array)
		{
			if (!(renderer.GetComponent<ParticleSystem>() != null))
			{
				Material[] materials = renderer.materials;
				for (int j = 0; j < materials.Length; j++)
				{
					UpdateAlphaByProperties(materials[j]);
				}
			}
		}
		array = skinRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			Material[] materials2 = array[i].materials;
			for (int k = 0; k < materials2.Length; k++)
			{
				UpdateAlphaByProperties(materials2[k]);
			}
		}
		for (int l = 0; l < lights.Length; l++)
		{
			lights[l].intensity -= 1f / 30f / FadeoutTime;
		}
		ParticleSystem[] array2 = particleSystems;
		foreach (ParticleSystem particleSystem in array2)
		{
			if (!particleSystem.isStopped)
			{
				particleSystem.Stop();
			}
		}
		AudioSource[] array3 = audioSources;
		for (int i = 0; i < array3.Length; i++)
		{
			array3[i].volume -= 1f / 30f / FadeoutTime;
		}
	}

	private void CacheRenderers()
	{
		renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
		Renderer[] componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
		skinRenderers = componentsInChildren;
		lights = GetComponentsInChildren<Light>(includeInactive: true);
		particleSystems = GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		audioSources = GetComponentsInChildren<AudioSource>();
	}

	private void Fadein()
	{
		Transform[] componentsInChildren = base.gameObject.GetComponentsInChildren<Transform>();
		foreach (Transform obj in componentsInChildren)
		{
			obj.gameObject.SetActive(value: false);
			obj.gameObject.SetActive(value: true);
		}
		Renderer[] array = renderers;
		foreach (Renderer renderer in array)
		{
			if (!(renderer.GetComponent<ParticleSystem>() != null))
			{
				Material[] materials = renderer.materials;
				for (int j = 0; j < materials.Length; j++)
				{
					UpdateAlphaByProperties(materials[j], 1f);
				}
			}
		}
		AudioSource[] array2 = audioSources;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].volume = 1f;
		}
	}
}
