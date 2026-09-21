using System;
using UnityEngine;
using UnityEngine.Events;

namespace UITween;

[Serializable]
public class AnimationParts : IAniamtionPartProxy
{
	public delegate void DisableOrDestroy(bool disable, AnimationParts part);

	public enum State
	{
		OPEN,
		CLOSE
	}

	public enum EndTweenClose
	{
		DEACTIVATE,
		DESTROY,
		NOTHING
	}

	public enum CallbackCall
	{
		END_OF_INTRO_ANIM,
		END_OF_EXIT_ANIM,
		END_OF_INTRO_AND_END_OF_EXIT_ANIM,
		START_INTRO_ANIM,
		START_INTRO_END_OF_EXIT_ANIM,
		START_INTRO_END_OF_INTRO_ANIM,
		START_INTRO_END_OF_INTRO_AND_END_OF_EXIT_ANIM,
		START_EXIT_ANIM,
		START_EXIT_START_INTRO_ANIM,
		START_EXIT_END_OF_EXIT_ANIM,
		START_EXIT_END_OF_INTRO_ANIM,
		START_EXIT_END_OF_INTRO_AND_END_OF_EXIT_ANIM,
		START_INTRO_AND_START_EXIT_END_OF_EXIT_ANIM,
		START_INTRO_AND_START_EXIT_END_OF_INTRO_ANIM,
		START_INTRO_AND_START_EXIT_END_OF_INTRO_AND_END_OF_EXIT_ANIM,
		NOTHING
	}

	[HideInInspector]
	public PositionPropetiesAnim PositionPropetiesAnim = new PositionPropetiesAnim();

	[HideInInspector]
	public ScalePropetiesAnim ScalePropetiesAnim = new ScalePropetiesAnim();

	[HideInInspector]
	public RotationPropetiesAnim RotationPropetiesAnim = new RotationPropetiesAnim();

	[HideInInspector]
	public FadePropetiesAnim FadePropetiesAnim = new FadePropetiesAnim();

	public bool UnscaledTimeAnimation;

	public bool SaveState;

	public bool AtomicAnimation;

	public State ObjectState = State.CLOSE;

	public EndTweenClose EndState;

	public CallbackCall CallCallback;

	public UnityEvent IntroEvents = new UnityEvent();

	public UnityEvent ExitEvents = new UnityEvent();

	private UnityEvent CallBackObject;

	private bool CheckNextFrame;

	private bool CallOnThisFrame;

	[SerializeField]
	[HideInInspector]
	private float animationDuration = 1f;

	public static event DisableOrDestroy OnDisableOrDestroy;

	public void SetAniamtioDuration(float duration)
	{
		if (duration > 0f)
		{
			animationDuration = duration;
		}
		else
		{
			duration = 0.01f;
		}
	}

	public float GetAnimationDuration()
	{
		return animationDuration;
	}

	public AnimationParts(State ObjectState, bool UnscaledTimeAnimation, bool SaveState, bool AtomicAnim, EndTweenClose EndState, CallbackCall CallCallback, UnityEvent IntroEvents, UnityEvent ExitEvents)
	{
		this.ObjectState = ObjectState;
		this.UnscaledTimeAnimation = UnscaledTimeAnimation;
		this.SaveState = SaveState;
		AtomicAnimation = AtomicAnim;
		this.EndState = EndState;
		this.CallCallback = CallCallback;
		this.IntroEvents = IntroEvents;
		this.ExitEvents = ExitEvents;
	}

	public void CheckCallbackStatus()
	{
		if (CallCallback != CallbackCall.NOTHING)
		{
			if ((CallCallback == CallbackCall.START_INTRO_END_OF_EXIT_ANIM || CallCallback == CallbackCall.START_INTRO_ANIM || CallCallback == CallbackCall.START_INTRO_END_OF_INTRO_ANIM || CallCallback == CallbackCall.START_INTRO_END_OF_INTRO_AND_END_OF_EXIT_ANIM || CallCallback == CallbackCall.START_INTRO_AND_START_EXIT_END_OF_EXIT_ANIM || CallCallback == CallbackCall.START_INTRO_AND_START_EXIT_END_OF_INTRO_ANIM || CallCallback == CallbackCall.START_INTRO_AND_START_EXIT_END_OF_INTRO_AND_END_OF_EXIT_ANIM || CallCallback == CallbackCall.START_EXIT_START_INTRO_ANIM) && ObjectState == State.OPEN)
			{
				CheckCallBack(IntroEvents);
			}
			else if ((CallCallback == CallbackCall.START_EXIT_END_OF_EXIT_ANIM || CallCallback == CallbackCall.START_EXIT_ANIM || CallCallback == CallbackCall.START_EXIT_END_OF_INTRO_ANIM || CallCallback == CallbackCall.START_EXIT_END_OF_INTRO_AND_END_OF_EXIT_ANIM || CallCallback == CallbackCall.START_INTRO_AND_START_EXIT_END_OF_EXIT_ANIM || CallCallback == CallbackCall.START_INTRO_AND_START_EXIT_END_OF_INTRO_ANIM || CallCallback == CallbackCall.START_INTRO_AND_START_EXIT_END_OF_INTRO_AND_END_OF_EXIT_ANIM || CallCallback == CallbackCall.START_EXIT_START_INTRO_ANIM) && ObjectState == State.CLOSE)
			{
				CheckCallBack(ExitEvents);
			}
		}
	}

	public void FinalEnd()
	{
		if (ObjectState == State.CLOSE)
		{
			if (EndState == EndTweenClose.DEACTIVATE)
			{
				if (AnimationParts.OnDisableOrDestroy != null)
				{
					AnimationParts.OnDisableOrDestroy(disable: true, this);
				}
			}
			else if (EndState == EndTweenClose.DESTROY && AnimationParts.OnDisableOrDestroy != null)
			{
				AnimationParts.OnDisableOrDestroy(disable: false, this);
			}
		}
		if (SaveState)
		{
			ObjectState = ((ObjectState == State.OPEN) ? State.CLOSE : State.OPEN);
		}
	}

	public void Ended()
	{
		if (CallCallback != CallbackCall.NOTHING)
		{
			if (ObjectState == State.CLOSE && (CallCallback == CallbackCall.END_OF_EXIT_ANIM || CallCallback == CallbackCall.END_OF_INTRO_AND_END_OF_EXIT_ANIM || CallCallback == CallbackCall.START_INTRO_END_OF_EXIT_ANIM || CallCallback == CallbackCall.START_INTRO_END_OF_INTRO_AND_END_OF_EXIT_ANIM || CallCallback == CallbackCall.START_EXIT_END_OF_EXIT_ANIM || CallCallback == CallbackCall.START_EXIT_END_OF_INTRO_AND_END_OF_EXIT_ANIM || CallCallback == CallbackCall.START_INTRO_AND_START_EXIT_END_OF_EXIT_ANIM || CallCallback == CallbackCall.START_INTRO_AND_START_EXIT_END_OF_INTRO_AND_END_OF_EXIT_ANIM))
			{
				CheckCallBack(ExitEvents);
			}
			if ((CallCallback == CallbackCall.END_OF_INTRO_ANIM || CallCallback == CallbackCall.END_OF_INTRO_AND_END_OF_EXIT_ANIM || CallCallback == CallbackCall.START_INTRO_END_OF_INTRO_ANIM || CallCallback == CallbackCall.START_INTRO_END_OF_INTRO_AND_END_OF_EXIT_ANIM || CallCallback == CallbackCall.START_EXIT_END_OF_INTRO_ANIM || CallCallback == CallbackCall.START_EXIT_END_OF_INTRO_AND_END_OF_EXIT_ANIM || CallCallback == CallbackCall.START_INTRO_AND_START_EXIT_END_OF_INTRO_ANIM || CallCallback == CallbackCall.START_INTRO_AND_START_EXIT_END_OF_INTRO_AND_END_OF_EXIT_ANIM) && ObjectState == State.OPEN)
			{
				CheckCallBack(IntroEvents);
			}
		}
	}

	public void FrameCheck()
	{
		if (CheckNextFrame)
		{
			if (CallOnThisFrame)
			{
				CallCallbackObjects();
			}
			CallOnThisFrame = !CallOnThisFrame;
		}
	}

	public bool IsObjectOpened()
	{
		if (ObjectState == State.CLOSE)
		{
			return false;
		}
		return true;
	}

	public void ChangeStatus()
	{
		if (ObjectState == State.CLOSE)
		{
			ObjectState = State.OPEN;
		}
		else
		{
			ObjectState = State.CLOSE;
		}
	}

	public void SetStatus(bool open)
	{
		if (open)
		{
			ObjectState = State.OPEN;
		}
		else
		{
			ObjectState = State.CLOSE;
		}
	}

	private void CheckCallBack(UnityEvent CallbackObject)
	{
		CallBackObject = CallbackObject;
		CheckNextFrame = !CheckNextFrame;
	}

	private void CallCallbackObjects()
	{
		CheckNextFrame = !CheckNextFrame;
		CallBackObject.Invoke();
	}
}
