using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace UnityEngine.UI.Extensions;

public class ScrollPositionController : UIBehaviour, IBeginDragHandler, IEventSystemHandler, IEndDragHandler, IDragHandler
{
	[Serializable]
	public class UpdatePositionEvent : UnityEvent<float>
	{
	}

	[Serializable]
	public class ItemSelectedEvent : UnityEvent<int>
	{
	}

	[Serializable]
	private struct Snap
	{
		public bool Enable;

		public float VelocityThreshold;

		public float Duration;
	}

	private enum ScrollDirection
	{
		Vertical,
		Horizontal
	}

	private enum MovementType
	{
		Unrestricted,
		Elastic,
		Clamped
	}

	[SerializeField]
	private RectTransform viewport;

	[SerializeField]
	private ScrollDirection directionOfRecognize;

	[SerializeField]
	private MovementType movementType = MovementType.Elastic;

	[SerializeField]
	private float elasticity = 0.1f;

	[SerializeField]
	private float scrollSensitivity = 1f;

	[SerializeField]
	private bool inertia = true;

	[SerializeField]
	[Tooltip("Only used when inertia is enabled")]
	private float decelerationRate = 0.03f;

	[SerializeField]
	[Tooltip("Only used when inertia is enabled")]
	private Snap snap = new Snap
	{
		Enable = true,
		VelocityThreshold = 0.5f,
		Duration = 0.3f
	};

	[SerializeField]
	private int dataCount;

	[Tooltip("Event that fires when the position of an item changes")]
	public UpdatePositionEvent OnUpdatePosition;

	[Tooltip("Event that fires when an item is selected/focused")]
	public ItemSelectedEvent OnItemSelected;

	private Vector2 pointerStartLocalPosition;

	private float dragStartScrollPosition;

	private float currentScrollPosition;

	private bool dragging;

	private float velocity;

	private float prevScrollPosition;

	private bool autoScrolling;

	private float autoScrollDuration;

	private float autoScrollStartTime;

	private float autoScrollPosition;

	void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			pointerStartLocalPosition = Vector2.zero;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, eventData.position, eventData.pressEventCamera, out pointerStartLocalPosition);
			dragStartScrollPosition = currentScrollPosition;
			dragging = true;
		}
	}

	void IDragHandler.OnDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left && dragging && RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, eventData.position, eventData.pressEventCamera, out var localPoint))
		{
			Vector2 vector = localPoint - pointerStartLocalPosition;
			float num = ((directionOfRecognize == ScrollDirection.Horizontal) ? (0f - vector.x) : vector.y) / GetViewportSize() * scrollSensitivity + dragStartScrollPosition;
			float num2 = CalculateOffset(num);
			num += num2;
			if (movementType == MovementType.Elastic && num2 != 0f)
			{
				num -= RubberDelta(num2, scrollSensitivity);
			}
			UpdatePosition(num);
		}
	}

	void IEndDragHandler.OnEndDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			dragging = false;
		}
	}

	private float GetViewportSize()
	{
		if (directionOfRecognize != ScrollDirection.Horizontal)
		{
			return viewport.rect.size.y;
		}
		return viewport.rect.size.x;
	}

	private float CalculateOffset(float position)
	{
		if (movementType == MovementType.Unrestricted)
		{
			return 0f;
		}
		if (position < 0f)
		{
			return 0f - position;
		}
		if (position > (float)(dataCount - 1))
		{
			return (float)(dataCount - 1) - position;
		}
		return 0f;
	}

	private void UpdatePosition(float position)
	{
		currentScrollPosition = position;
		if (OnUpdatePosition != null)
		{
			OnUpdatePosition.Invoke(currentScrollPosition);
		}
	}

	private float RubberDelta(float overStretching, float viewSize)
	{
		return (1f - 1f / (Mathf.Abs(overStretching) * 0.55f / viewSize + 1f)) * viewSize * Mathf.Sign(overStretching);
	}

	public void SetDataCount(int dataCont)
	{
		dataCount = dataCont;
	}

	private void Update()
	{
		float unscaledDeltaTime = Time.unscaledDeltaTime;
		float num = CalculateOffset(currentScrollPosition);
		if (autoScrolling)
		{
			float num2 = Mathf.Clamp01((Time.unscaledTime - autoScrollStartTime) / Mathf.Max(autoScrollDuration, float.Epsilon));
			float position = Mathf.Lerp(dragStartScrollPosition, autoScrollPosition, EaseInOutCubic(0f, 1f, num2));
			UpdatePosition(position);
			if (Mathf.Approximately(num2, 1f))
			{
				autoScrolling = false;
				if (OnItemSelected != null)
				{
					OnItemSelected.Invoke(Mathf.RoundToInt(GetLoopPosition(autoScrollPosition, dataCount)));
				}
			}
		}
		else if (!dragging && (num != 0f || velocity != 0f))
		{
			float num3 = currentScrollPosition;
			if (movementType == MovementType.Elastic && num != 0f)
			{
				float currentVelocity = velocity;
				num3 = Mathf.SmoothDamp(currentScrollPosition, currentScrollPosition + num, ref currentVelocity, elasticity, float.PositiveInfinity, unscaledDeltaTime);
				velocity = currentVelocity;
			}
			else if (inertia)
			{
				velocity *= Mathf.Pow(decelerationRate, unscaledDeltaTime);
				if (Mathf.Abs(velocity) < 0.001f)
				{
					velocity = 0f;
				}
				num3 += velocity * unscaledDeltaTime;
				if (snap.Enable && Mathf.Abs(velocity) < snap.VelocityThreshold)
				{
					ScrollTo(Mathf.RoundToInt(currentScrollPosition), snap.Duration);
				}
			}
			else
			{
				velocity = 0f;
			}
			if (velocity != 0f)
			{
				if (movementType == MovementType.Clamped)
				{
					num = CalculateOffset(num3);
					num3 += num;
				}
				UpdatePosition(num3);
			}
		}
		if (!autoScrolling && dragging && inertia)
		{
			float b = (currentScrollPosition - prevScrollPosition) / unscaledDeltaTime;
			velocity = Mathf.Lerp(velocity, b, unscaledDeltaTime * 10f);
		}
		if (currentScrollPosition != prevScrollPosition)
		{
			prevScrollPosition = currentScrollPosition;
		}
	}

	public void ScrollTo(int index, float duration)
	{
		velocity = 0f;
		autoScrolling = true;
		autoScrollDuration = duration;
		autoScrollStartTime = Time.unscaledTime;
		dragStartScrollPosition = currentScrollPosition;
		autoScrollPosition = ((movementType == MovementType.Unrestricted) ? CalculateClosestPosition(index) : ((float)index));
	}

	private float CalculateClosestPosition(int index)
	{
		float num = GetLoopPosition(index, dataCount) - GetLoopPosition(currentScrollPosition, dataCount);
		if (Mathf.Abs(num) > (float)dataCount * 0.5f)
		{
			num = Mathf.Sign(0f - num) * ((float)dataCount - Mathf.Abs(num));
		}
		return num + currentScrollPosition;
	}

	private float GetLoopPosition(float position, int length)
	{
		if (position < 0f)
		{
			position = (float)(length - 1) + (position + 1f) % (float)length;
		}
		else if (position > (float)(length - 1))
		{
			position %= (float)length;
		}
		return position;
	}

	private float EaseInOutCubic(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end * 0.5f * value * value * value + start;
		}
		value -= 2f;
		return end * 0.5f * (value * value * value + 2f) + start;
	}
}
