using System;
using UnityEngine;

namespace UITween;

[Serializable]
public class FadePropetiesAnim
{
	[SerializeField]
	[HideInInspector]
	private bool fadeInOutEnabled;

	[SerializeField]
	[HideInInspector]
	private bool fadeOverride;

	[SerializeField]
	[HideInInspector]
	private float startFade;

	[SerializeField]
	[HideInInspector]
	private float endFade = 1f;

	public void SetFadeEnable(bool enabled)
	{
		fadeInOutEnabled = enabled;
	}

	public void SetFadeValues(float startFade, float endFade)
	{
		if (endFade < startFade)
		{
			Debug.LogError("End Value should be greater than the start value, values not changed");
			return;
		}
		this.startFade = startFade;
		this.endFade = endFade;
	}

	public float GetStartFadeValue()
	{
		return startFade;
	}

	public float GetEndFadeValue()
	{
		return endFade;
	}

	public bool IsFadeEnabled()
	{
		return fadeInOutEnabled;
	}

	public void SetFadeOverride(bool enabled)
	{
		fadeOverride = enabled;
	}

	public bool IsFadeOverrideEnabled()
	{
		return fadeOverride;
	}
}
