using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Extensions/Stepper")]
[RequireComponent(typeof(RectTransform))]
public class Stepper : UIBehaviour
{
	[Serializable]
	public class StepperValueChangedEvent : UnityEvent<int>
	{
	}

	private Selectable[] _sides;

	[SerializeField]
	[Tooltip("The current step value of the control")]
	private int _value;

	[SerializeField]
	[Tooltip("The minimum step value allowed by the control. When reached it will disable the '-' button")]
	private int _minimum;

	[SerializeField]
	[Tooltip("The maximum step value allowed by the control. When reached it will disable the '+' button")]
	private int _maximum = 100;

	[SerializeField]
	[Tooltip("The step increment used to increment / decrement the step value")]
	private int _step = 1;

	[SerializeField]
	[Tooltip("Does the step value loop around from end to end")]
	private bool _wrap;

	[SerializeField]
	[Tooltip("A GameObject with an Image to use as a separator between segments. Size of the RectTransform will determine the size of the separator used.\nNote, make sure to disable the separator GO so that it does not affect the scene")]
	private Graphic _separator;

	private float _separatorWidth;

	[SerializeField]
	private StepperValueChangedEvent _onValueChanged = new StepperValueChangedEvent();

	private float separatorWidth
	{
		get
		{
			if (_separatorWidth == 0f && (bool)separator)
			{
				_separatorWidth = separator.rectTransform.rect.width;
				Image component = separator.GetComponent<Image>();
				if ((bool)component)
				{
					_separatorWidth /= component.pixelsPerUnit;
				}
			}
			return _separatorWidth;
		}
	}

	public Selectable[] sides
	{
		get
		{
			if (_sides == null || _sides.Length == 0)
			{
				_sides = GetSides();
			}
			return _sides;
		}
	}

	public int value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}

	public int minimum
	{
		get
		{
			return _minimum;
		}
		set
		{
			_minimum = value;
		}
	}

	public int maximum
	{
		get
		{
			return _maximum;
		}
		set
		{
			_maximum = value;
		}
	}

	public int step
	{
		get
		{
			return _step;
		}
		set
		{
			_step = value;
		}
	}

	public bool wrap
	{
		get
		{
			return _wrap;
		}
		set
		{
			_wrap = value;
		}
	}

	public Graphic separator
	{
		get
		{
			return _separator;
		}
		set
		{
			_separator = value;
			_separatorWidth = 0f;
			LayoutSides(sides);
		}
	}

	public StepperValueChangedEvent onValueChanged
	{
		get
		{
			return _onValueChanged;
		}
		set
		{
			_onValueChanged = value;
		}
	}

	protected Stepper()
	{
	}

	private Selectable[] GetSides()
	{
		Selectable[] componentsInChildren = GetComponentsInChildren<Selectable>();
		if (componentsInChildren.Length != 2)
		{
			throw new InvalidOperationException("A stepper must have two Button children");
		}
		for (int i = 0; i < 2; i++)
		{
			if (componentsInChildren[i].GetComponent<StepperSide>() == null)
			{
				componentsInChildren[i].gameObject.AddComponent<StepperSide>();
			}
		}
		if (!wrap)
		{
			DisableAtExtremes(componentsInChildren);
		}
		LayoutSides(componentsInChildren);
		return componentsInChildren;
	}

	public void StepUp()
	{
		Step(step);
	}

	public void StepDown()
	{
		Step(-step);
	}

	private void Step(int amount)
	{
		value += amount;
		if (wrap)
		{
			if (value > maximum)
			{
				value = minimum;
			}
			if (value < minimum)
			{
				value = maximum;
			}
		}
		else
		{
			value = Math.Max(minimum, value);
			value = Math.Min(maximum, value);
			DisableAtExtremes(sides);
		}
		_onValueChanged.Invoke(value);
	}

	private void DisableAtExtremes(Selectable[] sides)
	{
		sides[0].interactable = wrap || value > minimum;
		sides[1].interactable = wrap || value < maximum;
	}

	private void RecreateSprites(Selectable[] sides)
	{
		for (int i = 0; i < 2; i++)
		{
			if (sides[i].image == null)
			{
				continue;
			}
			Sprite sprite = sides[i].image.sprite;
			if (sprite.border.x != 0f && sprite.border.z != 0f)
			{
				Rect rect = sprite.rect;
				Vector4 border = sprite.border;
				if (i == 0)
				{
					rect.xMax = border.z;
					border.z = 0f;
				}
				else
				{
					rect.xMin = border.x;
					border.x = 0f;
				}
				sides[i].image.sprite = Sprite.Create(sprite.texture, rect, sprite.pivot, sprite.pixelsPerUnit, 0u, SpriteMeshType.FullRect, border);
			}
		}
	}

	public void LayoutSides(Selectable[] sides = null)
	{
		sides = sides ?? this.sides;
		RecreateSprites(sides);
		RectTransform rectTransform = base.transform as RectTransform;
		float num = rectTransform.rect.width / 2f - separatorWidth;
		for (int i = 0; i < 2; i++)
		{
			float inset = ((i == 0) ? 0f : (num + separatorWidth));
			RectTransform component = sides[i].GetComponent<RectTransform>();
			component.anchorMin = Vector2.zero;
			component.anchorMax = Vector2.zero;
			component.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, inset, num);
			component.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0f, rectTransform.rect.height);
		}
		if ((bool)separator)
		{
			Transform transform = base.gameObject.transform.Find("Separator");
			Graphic obj = ((transform != null) ? transform.GetComponent<Graphic>() : Object.Instantiate(separator.gameObject).GetComponent<Graphic>());
			obj.gameObject.name = "Separator";
			obj.gameObject.SetActive(value: true);
			obj.rectTransform.SetParent(base.transform, worldPositionStays: false);
			obj.rectTransform.anchorMin = Vector2.zero;
			obj.rectTransform.anchorMax = Vector2.zero;
			obj.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, num, separatorWidth);
			obj.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0f, rectTransform.rect.height);
		}
	}

	public void SetStepperValue(string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			int num = int.Parse(value);
			num = Mathf.Clamp(num, minimum, maximum);
			this.value = num;
			DisableAtExtremes(sides);
			onValueChanged.Invoke(this.value);
		}
	}

	public void SetStepperValue(int value)
	{
		value = Mathf.Clamp(value, minimum, maximum);
		this.value = value;
		DisableAtExtremes(sides);
		onValueChanged.Invoke(this.value);
	}
}
