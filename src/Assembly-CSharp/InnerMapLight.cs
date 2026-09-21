using DG.Tweening;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class InnerMapLight : MonoBehaviour
{
	[SerializeField]
	private Light2D _light;

	[SerializeField]
	private float _brightestIntensity;

	[SerializeField]
	private float _darkestIntensity;

	private GameObject parent;

	private float randomOnValue;

	private void Awake()
	{
		randomOnValue = 0.95f;
	}

	private void OnEnable()
	{
		if (LightingManager.Instance != null)
		{
			InstantUpdateLightBasedOnGlobalLight(LightingManager.Instance.isTransitioning ? LightingManager.Instance.transitioningTo : LightingManager.Instance.currentGlobalLightState);
		}
		Messenger.AddListener<LightingManager.Light_State>(InnerMapSignals.UPDATE_INNER_MAP_LIGHT, UpdateLightBasedOnGlobalLight);
		Messenger.AddListener<LightingManager.Light_State>(InnerMapSignals.INSTANT_UPDATE_INNER_MAP_LIGHT, InstantUpdateLightBasedOnGlobalLight);
	}

	private void OnDisable()
	{
		Messenger.RemoveListener<LightingManager.Light_State>(InnerMapSignals.UPDATE_INNER_MAP_LIGHT, UpdateLightBasedOnGlobalLight);
		Messenger.RemoveListener<LightingManager.Light_State>(InnerMapSignals.INSTANT_UPDATE_INNER_MAP_LIGHT, InstantUpdateLightBasedOnGlobalLight);
	}

	private void UpdateLightBasedOnGlobalLight(LightingManager.Light_State globalLightState)
	{
		float targetIntensity = GetTargetIntensity(globalLightState);
		DOTween.To(SetLightIntensity, _light.intensity, targetIntensity, 8f);
	}

	private void InstantUpdateLightBasedOnGlobalLight(LightingManager.Light_State globalLightState)
	{
		SetLightIntensity(GetTargetIntensity(globalLightState));
	}

	private float GetTargetIntensity(LightingManager.Light_State lightState)
	{
		if (lightState != LightingManager.Light_State.Bright)
		{
			return _brightestIntensity;
		}
		return _darkestIntensity;
	}

	private void SetLightIntensity(float intensity)
	{
		_light.intensity = intensity;
		if (_light.alphaBlendOnOverlap)
		{
			_light.enabled = intensity >= randomOnValue;
		}
		else
		{
			_light.enabled = true;
		}
	}
}
