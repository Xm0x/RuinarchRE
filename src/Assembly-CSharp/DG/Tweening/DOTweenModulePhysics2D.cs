using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace DG.Tweening;

public static class DOTweenModulePhysics2D
{
	public static TweenerCore<Vector2, Vector2, VectorOptions> DOMove(this Rigidbody2D target, Vector2 endValue, float duration, bool snapping = false)
	{
		TweenerCore<Vector2, Vector2, VectorOptions> tweenerCore = DOTween.To(() => target.position, target.MovePosition, endValue, duration);
		tweenerCore.SetOptions(snapping).SetTarget(target);
		return tweenerCore;
	}

	public static TweenerCore<Vector2, Vector2, VectorOptions> DOMoveX(this Rigidbody2D target, float endValue, float duration, bool snapping = false)
	{
		TweenerCore<Vector2, Vector2, VectorOptions> tweenerCore = DOTween.To(() => target.position, target.MovePosition, new Vector2(endValue, 0f), duration);
		tweenerCore.SetOptions(AxisConstraint.X, snapping).SetTarget(target);
		return tweenerCore;
	}

	public static TweenerCore<Vector2, Vector2, VectorOptions> DOMoveY(this Rigidbody2D target, float endValue, float duration, bool snapping = false)
	{
		TweenerCore<Vector2, Vector2, VectorOptions> tweenerCore = DOTween.To(() => target.position, target.MovePosition, new Vector2(0f, endValue), duration);
		tweenerCore.SetOptions(AxisConstraint.Y, snapping).SetTarget(target);
		return tweenerCore;
	}

	public static TweenerCore<float, float, FloatOptions> DORotate(this Rigidbody2D target, float endValue, float duration)
	{
		TweenerCore<float, float, FloatOptions> tweenerCore = DOTween.To(() => target.rotation, target.MoveRotation, endValue, duration);
		tweenerCore.SetTarget(target);
		return tweenerCore;
	}

	public static Sequence DOJump(this Rigidbody2D target, Vector2 endValue, float jumpPower, int numJumps, float duration, bool snapping = false)
	{
		if (numJumps < 1)
		{
			numJumps = 1;
		}
		float startPosY = 0f;
		float offsetY = -1f;
		bool offsetYSet = false;
		Sequence s = DOTween.Sequence();
		Tween yTween = DOTween.To(() => target.position, delegate(Vector2 x)
		{
			target.position = x;
		}, new Vector2(0f, jumpPower), duration / (float)(numJumps * 2)).SetOptions(AxisConstraint.Y, snapping).SetEase(Ease.OutQuad)
			.SetRelative()
			.SetLoops(numJumps * 2, LoopType.Yoyo)
			.OnStart(delegate
			{
				startPosY = target.position.y;
			});
		s.Append(DOTween.To(() => target.position, delegate(Vector2 x)
		{
			target.position = x;
		}, new Vector2(endValue.x, 0f), duration).SetOptions(AxisConstraint.X, snapping).SetEase(Ease.Linear)).Join(yTween).SetTarget(target)
			.SetEase(DOTween.defaultEaseType);
		yTween.OnUpdate(delegate
		{
			if (!offsetYSet)
			{
				offsetYSet = true;
				offsetY = (s.isRelative ? endValue.y : (endValue.y - startPosY));
			}
			Vector3 vector = target.position;
			vector.y += DOVirtual.EasedValue(0f, offsetY, yTween.ElapsedPercentage(), Ease.OutQuad);
			target.MovePosition(vector);
		});
		return s;
	}
}
