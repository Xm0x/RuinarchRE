using System;
using UnityEngine;

namespace UITween;

[Serializable]
public class ScalePropetiesAnim
{
	[SerializeField]
	[HideInInspector]
	private bool scaleEnabled;

	[HideInInspector]
	public AnimationCurve TweenCurveEnterScale;

	[HideInInspector]
	public AnimationCurve TweenCurveExitScale;

	[HideInInspector]
	public Vector3 StartScale;

	[HideInInspector]
	public Vector3 EndScale;

	public void SetScaleEnable(bool enabled)
	{
		scaleEnabled = enabled;
	}

	public bool IsScaleEnabled()
	{
		return scaleEnabled;
	}

	public void SetAniamtionsCurve(AnimationCurve EntryTween, AnimationCurve ExitTween)
	{
		TweenCurveEnterScale = EntryTween;
		TweenCurveExitScale = ExitTween;
	}
}
