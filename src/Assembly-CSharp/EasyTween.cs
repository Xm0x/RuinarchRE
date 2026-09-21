using System;
using UITween;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class EasyTween : MonoBehaviour
{
	public RectTransform rectTransform;

	public AnimationParts animationParts = new AnimationParts(AnimationParts.State.CLOSE, UnscaledTimeAnimation: false, SaveState: false, AtomicAnim: false, AnimationParts.EndTweenClose.DEACTIVATE, AnimationParts.CallbackCall.NOTHING, new UnityEvent(), new UnityEvent());

	private CurrentAnimation currentAnimationGoing;

	public EasyTween()
	{
		CheckIfCurrenAnimationGoingExits();
	}

	public void OpenCloseObjectAnimation()
	{
		rectTransform.gameObject.SetActive(value: true);
		TriggerOpenClose();
	}

	public bool IsObjectOpened()
	{
		return currentAnimationGoing.IsObjectOpened();
	}

	public void SetUnscaledTimeAnimation(bool UnscaledTimeAnimation)
	{
		animationParts.UnscaledTimeAnimation = UnscaledTimeAnimation;
	}

	public void SetAnimatioDuration(float duration)
	{
		if (duration > 0f)
		{
			currentAnimationGoing.SetAniamtioDuration(duration);
		}
		else
		{
			currentAnimationGoing.SetAniamtioDuration(0.01f);
		}
	}

	public float GetAnimationDuration()
	{
		return currentAnimationGoing.GetAnimationDuration();
	}

	public void SetAnimationPosition(Vector2 StartAnchoredPos, Vector2 EndAnchoredPos, AnimationCurve EntryTween, AnimationCurve ExitTween)
	{
		currentAnimationGoing.SetAnimationPos(StartAnchoredPos, EndAnchoredPos, EntryTween, ExitTween, rectTransform);
	}

	public void SetAnimationScale(Vector3 StartAnchoredScale, Vector3 EndAnchoredScale, AnimationCurve EntryTween, AnimationCurve ExitTween)
	{
		currentAnimationGoing.SetAnimationScale(StartAnchoredScale, EndAnchoredScale, EntryTween, ExitTween);
	}

	public void SetAnimationRotation(Vector3 StartAnchoredEulerAng, Vector3 EndAnchoredEulerAng, AnimationCurve EntryTween, AnimationCurve ExitTween)
	{
		currentAnimationGoing.SetAnimationRotation(StartAnchoredEulerAng, EndAnchoredEulerAng, EntryTween, ExitTween);
	}

	public void SetFade(bool OverrideFade)
	{
		currentAnimationGoing.SetFade(OverrideFade);
	}

	public void SetFade()
	{
		currentAnimationGoing.SetFade(OverrideFade: false);
	}

	public void SetFadeStartEndValues(float startValua, float endValue)
	{
		currentAnimationGoing.SetFadeValuesStartEnd(startValua, endValue);
	}

	public void SetAnimationProperties(AnimationParts animationParts)
	{
		this.animationParts = animationParts;
		currentAnimationGoing = new CurrentAnimation(animationParts);
	}

	public void ChangeSetState(bool opened)
	{
		currentAnimationGoing.SetStatus(opened);
	}

	private void Start()
	{
		AnimationParts.OnDisableOrDestroy += CheckTriggerEndState;
	}

	private void OnDestroy()
	{
		AnimationParts.OnDisableOrDestroy -= CheckTriggerEndState;
	}

	private void Update()
	{
		currentAnimationGoing.AnimationFrame(rectTransform);
	}

	private void LateUpdate()
	{
		currentAnimationGoing.LateAnimationFrame(rectTransform);
	}

	public void TriggerOpenClose()
	{
		if (!currentAnimationGoing.IsObjectOpened())
		{
			currentAnimationGoing.PlayOpenAnimations();
		}
		else
		{
			currentAnimationGoing.PlayCloseAnimations();
		}
	}

	private void CheckTriggerEndState(bool disable, AnimationParts part)
	{
		if (part != animationParts)
		{
			return;
		}
		if (disable)
		{
			rectTransform.gameObject.SetActive(value: false);
			return;
		}
		if ((bool)base.gameObject && !rectTransform.gameObject == (bool)base.gameObject)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		UnityEngine.Object.DestroyImmediate(rectTransform.gameObject);
	}

	private void CheckIfCurrenAnimationGoingExits()
	{
		if (currentAnimationGoing == null)
		{
			SetAnimationProperties(animationParts);
		}
	}

	public void OnValueChangedAnimation(bool value)
	{
		if (value)
		{
			if (!currentAnimationGoing.IsObjectOpened())
			{
				currentAnimationGoing.PlayOpenAnimations();
			}
		}
		else if (currentAnimationGoing.IsObjectOpened())
		{
			currentAnimationGoing.PlayCloseAnimations();
		}
	}
}
