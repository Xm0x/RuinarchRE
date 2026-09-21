using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Extensions/Cooldown Button")]
public class CooldownButton : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
{
	[Serializable]
	public class CooldownButtonEvent : UnityEvent<PointerEventData.InputButton>
	{
	}

	[SerializeField]
	private float cooldownTimeout;

	[SerializeField]
	private float cooldownSpeed = 1f;

	[SerializeField]
	[ReadOnly]
	private bool cooldownActive;

	[SerializeField]
	[ReadOnly]
	private bool cooldownInEffect;

	[SerializeField]
	[ReadOnly]
	private float cooldownTimeElapsed;

	[SerializeField]
	[ReadOnly]
	private float cooldownTimeRemaining;

	[SerializeField]
	[ReadOnly]
	private int cooldownPercentRemaining;

	[SerializeField]
	[ReadOnly]
	private int cooldownPercentComplete;

	private PointerEventData buttonSource;

	[Tooltip("Event that fires when a button is initially pressed down")]
	public CooldownButtonEvent OnCooldownStart;

	[Tooltip("Event that fires when a button is released")]
	public CooldownButtonEvent OnButtonClickDuringCooldown;

	[Tooltip("Event that continually fires while a button is held down")]
	public CooldownButtonEvent OnCoolDownFinish;

	public float CooldownTimeout
	{
		get
		{
			return cooldownTimeout;
		}
		set
		{
			cooldownTimeout = value;
		}
	}

	public float CooldownSpeed
	{
		get
		{
			return cooldownSpeed;
		}
		set
		{
			cooldownSpeed = value;
		}
	}

	public bool CooldownInEffect => cooldownInEffect;

	public bool CooldownActive
	{
		get
		{
			return cooldownActive;
		}
		set
		{
			cooldownActive = value;
		}
	}

	public float CooldownTimeElapsed
	{
		get
		{
			return cooldownTimeElapsed;
		}
		set
		{
			cooldownTimeElapsed = value;
		}
	}

	public float CooldownTimeRemaining => cooldownTimeRemaining;

	public int CooldownPercentRemaining => cooldownPercentRemaining;

	public int CooldownPercentComplete => cooldownPercentComplete;

	private void Update()
	{
		if (CooldownActive)
		{
			cooldownTimeRemaining -= Time.deltaTime * cooldownSpeed;
			cooldownTimeElapsed = CooldownTimeout - CooldownTimeRemaining;
			if (cooldownTimeRemaining < 0f)
			{
				StopCooldown();
				return;
			}
			cooldownPercentRemaining = (int)(100f * cooldownTimeRemaining * CooldownTimeout / 100f);
			cooldownPercentComplete = (int)((CooldownTimeout - cooldownTimeRemaining) / CooldownTimeout * 100f);
		}
	}

	public void PauseCooldown()
	{
		if (CooldownInEffect)
		{
			CooldownActive = false;
		}
	}

	public void RestartCooldown()
	{
		if (CooldownInEffect)
		{
			CooldownActive = true;
		}
	}

	public void StopCooldown()
	{
		cooldownTimeElapsed = CooldownTimeout;
		cooldownTimeRemaining = 0f;
		cooldownPercentRemaining = 0;
		cooldownPercentComplete = 100;
		cooldownActive = (cooldownInEffect = false);
		if (OnCoolDownFinish != null)
		{
			OnCoolDownFinish.Invoke(buttonSource.button);
		}
	}

	public void CancelCooldown()
	{
		cooldownActive = (cooldownInEffect = false);
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		buttonSource = eventData;
		if (CooldownInEffect && OnButtonClickDuringCooldown != null)
		{
			OnButtonClickDuringCooldown.Invoke(eventData.button);
		}
		if (!CooldownInEffect)
		{
			if (OnCooldownStart != null)
			{
				OnCooldownStart.Invoke(eventData.button);
			}
			cooldownTimeRemaining = cooldownTimeout;
			cooldownActive = (cooldownInEffect = true);
		}
	}
}
