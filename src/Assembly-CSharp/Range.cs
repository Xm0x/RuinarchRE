using System;
using UnityEngine;

[Serializable]
public struct Range
{
	[Tooltip("Minimum value in range - Inclusive")]
	public float minimum;

	[Tooltip("Maximum value in range - Inclusive")]
	public float maximum;

	public Range(float p_minimum, float p_maximum)
	{
		minimum = p_minimum;
		maximum = p_maximum;
	}

	public bool IsInRange(float value)
	{
		if (value >= minimum)
		{
			return value <= maximum;
		}
		return false;
	}
}
