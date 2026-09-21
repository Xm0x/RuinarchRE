using System;
using System.Collections.Generic;
using Coffee.UIExtensions;
using DG.Tweening;
using Maccima_Games.Util;
using Ruinarch.Custom_UI;
using UnityEngine;
using UtilityScripts;

public class GrudgeItemUI : MonoBehaviour
{
	public Action<TRIGGER_GRUDGE_ACTION> onButtonClick;

	public RuinarchButton btnSkill;

	public RuinarchText txtDescription;

	public UIShiny borderShineEffect;

	public RectTransform rectTransformContent;

	public CanvasGroup canvasGroupContent;

	public CanvasGroup canvasGroupPortrait;

	public CanvasGroup canvasGroupSpellText;

	public UIShiny mainShineEffect;

	private TRIGGER_GRUDGE_ACTION m_grudgeActionType;

	[SerializeField]
	private AnimationCurve _animationCurve;

	private Vector2 _defaultContentSize;

	private void Awake()
	{
		_defaultContentSize = rectTransformContent.sizeDelta;
	}

	private void OnEnable()
	{
		btnSkill.onClick.AddListener(SkillClicked);
	}

	private void OnDisable()
	{
		btnSkill.onClick.RemoveListener(SkillClicked);
	}

	public void InitItem(TRIGGER_GRUDGE_ACTION p_grudgeActionType, Character p_actor, Character p_target)
	{
		m_grudgeActionType = p_grudgeActionType;
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("source", p_actor.visuals.GetCharacterStringIcon() + Utilities.ColorizeName(p_actor.name, CharacterManager.Instance.GetCharacterNameColorHex(p_actor)));
		dictionary.Add("target", p_target.visuals.GetCharacterStringIcon() + Utilities.ColorizeName(p_target.name, CharacterManager.Instance.GetCharacterNameColorHex(p_target)));
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("TriggerGrudge_Table", p_grudgeActionType.ToStringEnum() + "_Description", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		txtDescription.text = localizedValue;
		base.transform.localScale = Vector3.one;
	}

	private void SkillClicked()
	{
		onButtonClick?.Invoke(m_grudgeActionType);
	}

	public Sequence PrepareAnimation()
	{
		Sequence sequence = DOTween.Sequence();
		Vector2 defaultContentSize = _defaultContentSize;
		rectTransformContent.sizeDelta = new Vector2(defaultContentSize.x - 30f, defaultContentSize.y - 30f);
		canvasGroupContent.alpha = 0f;
		canvasGroupPortrait.alpha = 0f;
		canvasGroupSpellText.alpha = 0f;
		sequence.Append(rectTransformContent.DOSizeDelta(defaultContentSize, 0.3f).SetEase(_animationCurve).OnPlay(delegate
		{
			mainShineEffect.Play();
		}));
		sequence.Join(canvasGroupContent.DOFade(1f, 0.2f));
		sequence.Join(canvasGroupPortrait.DOFade(1f, 0.2f).SetDelay(0.2f));
		sequence.Join(canvasGroupSpellText.DOFade(1f, 0.2f).SetDelay(0.3f));
		sequence.OnKill(delegate
		{
			SetContentSize(_defaultContentSize);
		});
		sequence.OnComplete(delegate
		{
			SetContentSize(_defaultContentSize);
		});
		return sequence;
	}

	private void SetContentSize(Vector2 size)
	{
		rectTransformContent.sizeDelta = size;
	}
}
