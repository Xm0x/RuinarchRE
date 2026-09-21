using System;
using System.Collections.Generic;
using Coffee.UIExtensions;
using DG.Tweening;
using Inner_Maps.Location_Structures;
using Maccima_Games.Util;
using Ruinarch.Custom_UI;
using UnityEngine;
using UtilityScripts;

public class CriticalBreakItemUI : MonoBehaviour
{
	public Action<CriticalBreakItemUI> onButtonClick;

	public RuinarchButton btnSkill;

	public RuinarchText txtDescription;

	public UIShiny borderShineEffect;

	public RectTransform rectTransformContent;

	public CanvasGroup canvasGroupContent;

	public CanvasGroup canvasGroupPortrait;

	public CanvasGroup canvasGroupSpellText;

	public UIShiny mainShineEffect;

	private CRITICAL_BREAK_ACTION m_criticalBreakActionType;

	[SerializeField]
	private AnimationCurve _animationCurve;

	private Vector2 _defaultContentSize;

	public CRITICAL_BREAK_ACTION criticalBreakActionType => m_criticalBreakActionType;

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

	public void InitItem(CRITICAL_BREAK_ACTION p_actionType, Character p_actor, object p_target)
	{
		m_criticalBreakActionType = p_actionType;
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("source", p_actor.visuals.GetCharacterStringIcon() + Utilities.ColorizeName(p_actor.name, CharacterManager.Instance.GetCharacterNameColorHex(p_actor)));
		if (p_target != null)
		{
			if (p_target is Character character)
			{
				dictionary.Add("target", character.visuals.GetCharacterStringIcon() + Utilities.ColorizeName(character.name, CharacterManager.Instance.GetCharacterNameColorHex(character)));
			}
			else if (p_target is LocationStructure locationStructure)
			{
				dictionary.Add("target", locationStructure.bookmarkName ?? "");
			}
		}
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("PlayerActions_Table", "Critical Break_" + p_actionType.ToStringEnum(), dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		txtDescription.text = localizedValue;
		base.transform.localScale = Vector3.one;
	}

	private void SkillClicked()
	{
		onButtonClick?.Invoke(this);
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
