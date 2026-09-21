using System;
using UnityEngine;

namespace UITween;

[Serializable]
public class RotationPropetiesAnim
{
	[SerializeField]
	[HideInInspector]
	private bool rotationEnabled;

	[HideInInspector]
	public AnimationCurve TweenCurveEnterRot;

	[HideInInspector]
	public AnimationCurve TweenCurveExitRot;

	[HideInInspector]
	public Vector3 StartRot;

	[HideInInspector]
	public Vector3 EndRot;

	public void SetRotationEnable(bool enabled)
	{
		rotationEnabled = enabled;
	}

	public bool IsRotationEnabled()
	{
		return rotationEnabled;
	}

	public void SetAniamtionsCurve(AnimationCurve EntryTween, AnimationCurve ExitTween)
	{
		TweenCurveEnterRot = EntryTween;
		TweenCurveExitRot = ExitTween;
	}
}
