using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Core.Easing;
using DG.Tweening.Core.Enums;
using DG.Tweening.Plugins.Core;
using DG.Tweening.Plugins.Options;

public class CustomRangePlugin : ABSTweenPlugin<CustomRange, CustomRange, NoOptions>
{
	public override void Reset(TweenerCore<CustomRange, CustomRange, NoOptions> t)
	{
	}

	public override void SetFrom(TweenerCore<CustomRange, CustomRange, NoOptions> t, bool isRelative)
	{
		CustomRange endValue = t.endValue;
		t.endValue = t.getter();
		t.startValue = (isRelative ? (t.endValue + endValue) : endValue);
		t.setter(t.startValue);
	}

	public override void SetFrom(TweenerCore<CustomRange, CustomRange, NoOptions> t, CustomRange fromValue, bool setImmediately)
	{
		t.startValue = fromValue;
		if (setImmediately)
		{
			t.setter(fromValue);
		}
	}

	public override CustomRange ConvertToStartValue(TweenerCore<CustomRange, CustomRange, NoOptions> t, CustomRange value)
	{
		return value;
	}

	public override void SetRelativeEndValue(TweenerCore<CustomRange, CustomRange, NoOptions> t)
	{
		t.endValue = t.startValue + t.changeValue;
	}

	public override void SetChangeValue(TweenerCore<CustomRange, CustomRange, NoOptions> t)
	{
		t.changeValue = t.endValue - t.startValue;
	}

	public override float GetSpeedBasedDuration(NoOptions options, float unitsXSecond, CustomRange changeValue)
	{
		return unitsXSecond;
	}

	public override void EvaluateAndApply(NoOptions options, Tween t, bool isRelative, DOGetter<CustomRange> getter, DOSetter<CustomRange> setter, float elapsed, CustomRange startValue, CustomRange changeValue, float duration, bool usingInversePosition, UpdateNotice updateNotice)
	{
		float num = EaseManager.Evaluate(t, elapsed, duration, t.easeOvershootOrAmplitude, t.easePeriod);
		startValue.min += changeValue.min * num;
		startValue.max += changeValue.max * num;
		setter(startValue);
	}
}
