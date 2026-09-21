using System;
using UnityEngine.UI.Extensions.Tweens;

namespace UnityEngine.UI.Extensions;

[RequireComponent(typeof(RectTransform), typeof(LayoutElement))]
[AddComponentMenu("UI/Extensions/Accordion/Accordion Element")]
public class AccordionElement : Toggle
{
	[SerializeField]
	private float m_MinHeight = 18f;

	[SerializeField]
	private float m_MinWidth = 40f;

	private Accordion m_Accordion;

	private RectTransform m_RectTransform;

	private LayoutElement m_LayoutElement;

	[NonSerialized]
	private readonly TweenRunner<FloatTween> m_FloatTweenRunner;

	public float MinHeight => m_MinHeight;

	public float MinWidth => m_MinWidth;

	protected AccordionElement()
	{
		if (m_FloatTweenRunner == null)
		{
			m_FloatTweenRunner = new TweenRunner<FloatTween>();
		}
		m_FloatTweenRunner.Init(this);
	}

	protected override void Awake()
	{
		base.Awake();
		base.transition = Transition.None;
		toggleTransition = ToggleTransition.None;
		m_Accordion = base.gameObject.GetComponentInParent<Accordion>();
		m_RectTransform = base.transform as RectTransform;
		m_LayoutElement = base.gameObject.GetComponent<LayoutElement>();
		onValueChanged.AddListener(OnValueChanged);
	}

	public void OnValueChanged(bool state)
	{
		if (m_LayoutElement == null)
		{
			return;
		}
		switch ((m_Accordion != null) ? m_Accordion.transition : Accordion.Transition.Instant)
		{
		case Accordion.Transition.Instant:
			if (state)
			{
				if (m_Accordion.ExpandVerticval)
				{
					m_LayoutElement.preferredHeight = -1f;
				}
				else
				{
					m_LayoutElement.preferredWidth = -1f;
				}
			}
			else if (m_Accordion.ExpandVerticval)
			{
				m_LayoutElement.preferredHeight = m_MinHeight;
			}
			else
			{
				m_LayoutElement.preferredWidth = m_MinWidth;
			}
			break;
		case Accordion.Transition.Tween:
			if (state)
			{
				if (m_Accordion.ExpandVerticval)
				{
					StartTween(m_MinHeight, GetExpandedHeight());
				}
				else
				{
					StartTween(m_MinWidth, GetExpandedWidth());
				}
			}
			else if (m_Accordion.ExpandVerticval)
			{
				StartTween(m_RectTransform.rect.height, m_MinHeight);
			}
			else
			{
				StartTween(m_RectTransform.rect.width, m_MinWidth);
			}
			break;
		}
	}

	protected float GetExpandedHeight()
	{
		if (m_LayoutElement == null)
		{
			return m_MinHeight;
		}
		float preferredHeight = m_LayoutElement.preferredHeight;
		m_LayoutElement.preferredHeight = -1f;
		float preferredHeight2 = LayoutUtility.GetPreferredHeight(m_RectTransform);
		m_LayoutElement.preferredHeight = preferredHeight;
		return preferredHeight2;
	}

	protected float GetExpandedWidth()
	{
		if (m_LayoutElement == null)
		{
			return m_MinWidth;
		}
		float preferredWidth = m_LayoutElement.preferredWidth;
		m_LayoutElement.preferredWidth = -1f;
		float preferredWidth2 = LayoutUtility.GetPreferredWidth(m_RectTransform);
		m_LayoutElement.preferredWidth = preferredWidth;
		return preferredWidth2;
	}

	protected void StartTween(float startFloat, float targetFloat)
	{
		float duration = ((m_Accordion != null) ? m_Accordion.transitionDuration : 0.3f);
		FloatTween info = new FloatTween
		{
			duration = duration,
			startFloat = startFloat,
			targetFloat = targetFloat
		};
		if (m_Accordion.ExpandVerticval)
		{
			info.AddOnChangedCallback(SetHeight);
		}
		else
		{
			info.AddOnChangedCallback(SetWidth);
		}
		info.ignoreTimeScale = true;
		m_FloatTweenRunner.StartTween(info);
	}

	protected void SetHeight(float height)
	{
		if (!(m_LayoutElement == null))
		{
			m_LayoutElement.preferredHeight = height;
		}
	}

	protected void SetWidth(float width)
	{
		if (!(m_LayoutElement == null))
		{
			m_LayoutElement.preferredWidth = width;
		}
	}
}
