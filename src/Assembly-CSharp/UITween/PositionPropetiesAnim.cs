using System;
using UnityEngine;

namespace UITween;

[Serializable]
public class PositionPropetiesAnim
{
	[SerializeField]
	[HideInInspector]
	private bool positionEnabled;

	[HideInInspector]
	public AnimationCurve TweenCurveEnterPos;

	[HideInInspector]
	public AnimationCurve TweenCurveExitPos;

	[HideInInspector]
	public Vector3 StartPos;

	[HideInInspector]
	public Vector3 EndPos;

	public void SetPositionEnable(bool enabled)
	{
		positionEnabled = enabled;
	}

	public bool IsPositionEnabled()
	{
		return positionEnabled;
	}

	public void SetPosStart(Vector3 StartPos, RectTransform rectTr)
	{
		this.StartPos = StartPos;
	}

	public void SetPosEnd(Vector3 EndPos, Transform rectTr)
	{
		this.EndPos = EndPos;
	}

	public void SetAniamtionsCurve(AnimationCurve EntryTween, AnimationCurve ExitTween)
	{
		TweenCurveEnterPos = EntryTween;
		TweenCurveExitPos = ExitTween;
	}
}
