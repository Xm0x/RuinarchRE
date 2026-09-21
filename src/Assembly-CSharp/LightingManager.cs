using DG.Tweening;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class LightingManager : BaseMonoBehaviour
{
	public enum Light_State
	{
		Dark,
		Bright
	}

	public static LightingManager Instance;

	[Header("Lights")]
	[SerializeField]
	private Light2D _globalLight;

	[SerializeField]
	private float _brightestIntensity;

	[SerializeField]
	private float _darkestIntensity;

	[Header("Transitions")]
	[SerializeField]
	private IntRange _darkPeriodRange;

	[SerializeField]
	private IntRange _brightPeriodRange;

	private TIME_IN_WORDS _currentTimeLight = TIME_IN_WORDS.NONE;

	private int _darkToLightTickDifference;

	private int _lightToDarkTickDifference;

	public Light_State currentGlobalLightState = Light_State.Bright;

	[SerializeField]
	private bool _isTransitioning;

	[SerializeField]
	private Light_State _transitioningTo;

	private Tweener _currentTween;

	public bool isTransitioning => _isTransitioning;

	public Light_State transitioningTo => _transitioningTo;

	private void Awake()
	{
		Instance = this;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Instance = null;
	}

	public void Initialize()
	{
		Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
		Messenger.AddListener<bool>(UISignals.PAUSED, OnGamePaused);
		Messenger.AddListener<PROGRESSION_SPEED>(UISignals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
		ComputeLightingValues();
		SetGlobalLightIntensity(_brightestIntensity);
		GameDate gameDate = GameManager.Instance.Today();
		if (_brightPeriodRange.IsInRange(GameManager.Instance.GetCeilingHoursBasedOnTicks(gameDate.tick)))
		{
			SetCurrentLightState(Light_State.Bright);
			SetGlobalLightIntensity(_brightestIntensity);
		}
		else
		{
			SetCurrentLightState(Light_State.Dark);
			SetGlobalLightIntensity(_darkestIntensity);
		}
		Messenger.Broadcast(InnerMapSignals.INSTANT_UPDATE_INNER_MAP_LIGHT, currentGlobalLightState);
	}

	private void OnTickEnded()
	{
		UpdateAllLightsBasedOnTimeOfDay(GameManager.Instance.Today());
	}

	private void ComputeLightingValues()
	{
		int hours = Mathf.Abs(_darkPeriodRange.lowerBound - _brightPeriodRange.lowerBound);
		_darkToLightTickDifference = GameManager.Instance.GetTicksBasedOnHour(hours);
		int hours2 = Mathf.Abs(_darkPeriodRange.upperBound - _brightPeriodRange.upperBound);
		_lightToDarkTickDifference = GameManager.Instance.GetTicksBasedOnHour(hours2);
	}

	private void UpdateAllLightsBasedOnTimeOfDay(GameDate date)
	{
		if (_darkPeriodRange.IsOutsideRange(GameManager.Instance.GetCeilingHoursBasedOnTicks(date.tick)))
		{
			SetCurrentLightState(Light_State.Dark);
		}
		else if (_brightPeriodRange.IsInRange(GameManager.Instance.GetCeilingHoursBasedOnTicks(date.tick)))
		{
			SetCurrentLightState(Light_State.Bright);
		}
		else if (!isTransitioning)
		{
			_isTransitioning = true;
			Messenger.Broadcast(arg1: _transitioningTo = ((currentGlobalLightState == Light_State.Dark) ? Light_State.Bright : Light_State.Dark), eventType: InnerMapSignals.UPDATE_INNER_MAP_LIGHT);
			float endValue = ((currentGlobalLightState == Light_State.Dark) ? _brightestIntensity : _darkestIntensity);
			_currentTween = DOTween.To(SetGlobalLightIntensity, _globalLight.intensity, endValue, (float)_darkToLightTickDifference * GameManager.Instance.GetTickSpeed(PROGRESSION_SPEED.X1)).OnComplete(OnDoneTransition);
			OnProgressionSpeedChanged(GameManager.Instance.currProgressionSpeed);
		}
	}

	private void OnDoneTransition()
	{
		_isTransitioning = false;
		_currentTween = null;
	}

	private void OnGamePaused(bool isPaused)
	{
		if (isPaused)
		{
			_currentTween?.Pause();
		}
		else
		{
			_currentTween?.Play();
		}
	}

	private void OnProgressionSpeedChanged(PROGRESSION_SPEED progression)
	{
		if (_currentTween != null)
		{
			switch (progression)
			{
			case PROGRESSION_SPEED.X1:
				_currentTween.timeScale = 1f;
				break;
			case PROGRESSION_SPEED.X2:
				_currentTween.timeScale = 1.2f;
				break;
			case PROGRESSION_SPEED.X4:
				_currentTween.timeScale = 1.4f;
				break;
			}
		}
	}

	private void SetGlobalLightIntensity(float intensity)
	{
		_globalLight.intensity = intensity;
	}

	private void SetCurrentLightState(Light_State lightState)
	{
		if (currentGlobalLightState != lightState)
		{
			currentGlobalLightState = lightState;
			if (lightState == Light_State.Bright)
			{
				AkSoundEngine.PostEvent("Play_Morning", InnerMapCameraMove.Instance.gameObject);
			}
		}
	}
}
