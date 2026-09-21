using UnityEngine;
using UnityEngine.UI;

namespace UITween;

public class CurrentAnimation
{
	private enum States
	{
		AWAKE,
		READY,
		START,
		ENDED,
		FINALENDED,
		COUNT
	}

	private AnimationParts animationPart;

	private float counterTween = 2f;

	private States AnimationStates;

	private AnimationCurve currentAnimationCurvePos;

	private Vector3 currentStartPos;

	private Vector3 currentEndPos;

	private AnimationCurve currentAnimationCurveScale;

	private Vector3 currentStartScale;

	private Vector3 currentEndScale;

	private AnimationCurve currentAnimationCurveRot;

	private Vector3 currentStartRot;

	private Vector3 currentEndRot;

	private float startAlphaValue;

	private float endAlphaValue;

	public CurrentAnimation(AnimationParts animationPart)
	{
		this.animationPart = animationPart;
	}

	public void AnimationFrame(RectTransform rectTransform)
	{
		if (AnimationStates != States.AWAKE && counterTween <= 1f)
		{
			SetAnimationOnFrame(rectTransform, counterTween);
		}
	}

	public void SetAnimationOnFrame(RectTransform rectTransform, float percentage)
	{
		if (animationPart.PositionPropetiesAnim.IsPositionEnabled())
		{
			MoveAnimation(rectTransform, percentage);
		}
		if (animationPart.RotationPropetiesAnim.IsRotationEnabled())
		{
			RotateAnimation(rectTransform, percentage);
		}
		if (animationPart.ScalePropetiesAnim.IsScaleEnabled())
		{
			ScaleAnimation(rectTransform, percentage);
		}
		if (animationPart.FadePropetiesAnim.IsFadeEnabled())
		{
			SetAlphaValue(rectTransform.transform, percentage);
		}
	}

	public void LateAnimationFrame(RectTransform rectTransform)
	{
		if (AnimationStates == States.AWAKE)
		{
			return;
		}
		if (counterTween <= 1f)
		{
			float num = (animationPart.UnscaledTimeAnimation ? Time.unscaledDeltaTime : Time.deltaTime);
			counterTween += num / animationPart.GetAnimationDuration();
		}
		else if (AnimationStates != States.FINALENDED)
		{
			SetAnimationOnFrame(rectTransform, 1f);
			if (AnimationStates != States.ENDED && AnimationStates != States.FINALENDED && animationPart.AtomicAnimation)
			{
				animationPart.Ended();
				AnimationStates = States.ENDED;
			}
			else
			{
				animationPart.FinalEnd();
				AnimationStates = States.FINALENDED;
			}
		}
		if (counterTween > 0.9f && !animationPart.AtomicAnimation && AnimationStates != States.ENDED && AnimationStates != States.FINALENDED)
		{
			animationPart.Ended();
			AnimationStates = States.ENDED;
		}
		animationPart.FrameCheck();
	}

	public void PlayOpenAnimations()
	{
		if (animationPart.PositionPropetiesAnim.IsPositionEnabled())
		{
			SetCurrentAnimPos(animationPart.PositionPropetiesAnim.TweenCurveEnterPos, animationPart.PositionPropetiesAnim.StartPos, animationPart.PositionPropetiesAnim.EndPos);
		}
		if (animationPart.RotationPropetiesAnim.IsRotationEnabled())
		{
			SetCurrentAnimRot(animationPart.RotationPropetiesAnim.TweenCurveEnterRot, animationPart.RotationPropetiesAnim.StartRot, animationPart.RotationPropetiesAnim.EndRot);
		}
		if (animationPart.ScalePropetiesAnim.IsScaleEnabled())
		{
			SetCurrentAnimScale(animationPart.ScalePropetiesAnim.TweenCurveEnterScale, animationPart.ScalePropetiesAnim.StartScale, animationPart.ScalePropetiesAnim.EndScale);
		}
		if (animationPart.FadePropetiesAnim.IsFadeEnabled())
		{
			SetFadeAnimation(animationPart.FadePropetiesAnim.GetStartFadeValue(), animationPart.FadePropetiesAnim.GetEndFadeValue());
		}
		counterTween = 0f;
		AnimationStates = States.READY;
		animationPart.ChangeStatus();
		animationPart.CheckCallbackStatus();
	}

	public void SetStatus(bool status)
	{
		animationPart.SetStatus(status);
	}

	public void PlayCloseAnimations()
	{
		if (animationPart.PositionPropetiesAnim.IsPositionEnabled())
		{
			SetCurrentAnimPos(animationPart.PositionPropetiesAnim.TweenCurveExitPos, animationPart.PositionPropetiesAnim.EndPos, animationPart.PositionPropetiesAnim.StartPos);
		}
		if (animationPart.RotationPropetiesAnim.IsRotationEnabled())
		{
			SetCurrentAnimRot(animationPart.RotationPropetiesAnim.TweenCurveExitRot, animationPart.RotationPropetiesAnim.EndRot, animationPart.RotationPropetiesAnim.StartRot);
		}
		if (animationPart.ScalePropetiesAnim.IsScaleEnabled())
		{
			SetCurrentAnimScale(animationPart.ScalePropetiesAnim.TweenCurveExitScale, animationPart.ScalePropetiesAnim.EndScale, animationPart.ScalePropetiesAnim.StartScale);
		}
		if (animationPart.FadePropetiesAnim.IsFadeEnabled())
		{
			SetFadeAnimation(animationPart.FadePropetiesAnim.GetEndFadeValue(), animationPart.FadePropetiesAnim.GetStartFadeValue());
		}
		counterTween = 0f;
		AnimationStates = States.READY;
		animationPart.ChangeStatus();
		animationPart.CheckCallbackStatus();
	}

	public void SetAnimationPos(Vector2 StartAnchoredPos, Vector2 EndAnchoredPos, AnimationCurve EntryTween, AnimationCurve ExitTween, RectTransform rectTransform)
	{
		animationPart.PositionPropetiesAnim.SetPositionEnable(enabled: true);
		animationPart.PositionPropetiesAnim.SetPosStart(StartAnchoredPos, rectTransform);
		animationPart.PositionPropetiesAnim.SetPosEnd(EndAnchoredPos, rectTransform.transform);
		animationPart.PositionPropetiesAnim.SetAniamtionsCurve(EntryTween, ExitTween);
	}

	public void SetAnimationScale(Vector2 StartAnchoredScale, Vector2 EndAnchoredScale, AnimationCurve EntryTween, AnimationCurve ExitTween)
	{
		animationPart.ScalePropetiesAnim.StartScale = StartAnchoredScale;
		animationPart.ScalePropetiesAnim.SetScaleEnable(enabled: true);
		animationPart.ScalePropetiesAnim.EndScale = EndAnchoredScale;
		animationPart.ScalePropetiesAnim.SetAniamtionsCurve(EntryTween, ExitTween);
	}

	public void SetAnimationRotation(Vector2 StartAnchoredEulerAng, Vector2 EndAnchoredEulerAng, AnimationCurve EntryTween, AnimationCurve ExitTween)
	{
		animationPart.RotationPropetiesAnim.SetRotationEnable(enabled: true);
		animationPart.RotationPropetiesAnim.StartRot = StartAnchoredEulerAng;
		animationPart.RotationPropetiesAnim.EndRot = EndAnchoredEulerAng;
		animationPart.RotationPropetiesAnim.SetAniamtionsCurve(EntryTween, ExitTween);
	}

	public void SetFade(bool OverrideFade)
	{
		animationPart.FadePropetiesAnim.SetFadeEnable(enabled: true);
		animationPart.FadePropetiesAnim.SetFadeOverride(OverrideFade);
	}

	public void SetFadeValuesStartEnd(float startAlphaValue, float endAlphaValue)
	{
		animationPart.FadePropetiesAnim.SetFadeValues(startAlphaValue, endAlphaValue);
	}

	public bool IsObjectOpened()
	{
		return animationPart.IsObjectOpened();
	}

	public void SetAniamtioDuration(float duration)
	{
		animationPart.SetAniamtioDuration(duration);
	}

	public float GetAnimationDuration()
	{
		return animationPart.GetAnimationDuration();
	}

	public void SetCurrentAnimPos(AnimationCurve currentAnimationCurvePos, Vector3 currentStartPos, Vector3 currentEndPos)
	{
		this.currentAnimationCurvePos = currentAnimationCurvePos;
		this.currentStartPos = currentStartPos;
		this.currentEndPos = currentEndPos;
	}

	public void MoveAnimation(RectTransform _rectTransform, float _counterTween)
	{
		float num = currentAnimationCurvePos.Evaluate(_counterTween);
		Vector3 vector = (currentEndPos - currentStartPos) * num;
		_rectTransform.anchoredPosition = currentStartPos + vector;
	}

	public void SetCurrentAnimScale(AnimationCurve currentAnimationCurveScale, Vector3 currentStartScale, Vector3 currentEndScale)
	{
		this.currentAnimationCurveScale = currentAnimationCurveScale;
		this.currentStartScale = currentStartScale;
		this.currentEndScale = currentEndScale;
	}

	public void ScaleAnimation(RectTransform _rectTransform, float _counterTween)
	{
		float num = currentAnimationCurveScale.Evaluate(_counterTween);
		Vector3 vector = (currentEndScale - currentStartScale) * num;
		_rectTransform.localScale = currentStartScale + vector;
	}

	public void SetCurrentAnimRot(AnimationCurve currentAnimationCurveRot, Vector3 currentStartRot, Vector3 currentEndRot)
	{
		this.currentAnimationCurveRot = currentAnimationCurveRot;
		this.currentStartRot = currentStartRot;
		this.currentEndRot = currentEndRot;
	}

	public void RotateAnimation(RectTransform _rectTransform, float _counterTween)
	{
		float num = currentAnimationCurveRot.Evaluate(_counterTween);
		Vector3 vector = (currentEndRot - currentStartRot) * num;
		_rectTransform.localEulerAngles = currentStartRot + vector;
	}

	public void SetFadeAnimation(float startAlphaValue, float endAlphaValue)
	{
		this.startAlphaValue = startAlphaValue;
		this.endAlphaValue = endAlphaValue;
	}

	public void SetAlphaValue(Transform _objectToSetAlpha, float _counterTween)
	{
		if ((bool)_objectToSetAlpha.GetComponent<MaskableGraphic>())
		{
			MaskableGraphic component = _objectToSetAlpha.GetComponent<MaskableGraphic>();
			Color color = component.color;
			_counterTween = Mathf.Clamp(_counterTween, 0f, 1f);
			color.a = Mathf.Abs(startAlphaValue + (endAlphaValue - startAlphaValue) * _counterTween);
			component.color = color;
		}
		if (_objectToSetAlpha.childCount <= 0)
		{
			return;
		}
		for (int i = 0; i < _objectToSetAlpha.childCount; i++)
		{
			Transform child = _objectToSetAlpha.GetChild(i);
			if (child.gameObject.activeSelf && (!child.GetComponent<ReferencedFrom>() || animationPart.FadePropetiesAnim.IsFadeOverrideEnabled()))
			{
				SetAlphaValue(child, _counterTween);
			}
		}
	}
}
