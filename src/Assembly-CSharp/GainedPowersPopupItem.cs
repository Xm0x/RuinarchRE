using Coffee.UIExtensions;
using DG.Tweening;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class GainedPowersPopupItem : MonoBehaviour
{
	public RuinarchButton mainButton;

	public RuinarchText txtSkillName;

	public RuinarchText txtDescription;

	public RuinarchText txtLevel;

	public Image imgIcon;

	public Sprite affliction;

	public Sprite spell;

	public Sprite playerAction;

	public Sprite minion;

	public Sprite passive;

	public Sprite structure;

	public HoverHandler hoverHandler;

	public UIShiny borderShineEffect;

	public RectTransform rectTransformContent;

	public CanvasGroup canvasGroupContent;

	public CanvasGroup canvasGroupPortrait;

	public CanvasGroup canvasGroupSpellText;

	public UIShiny mainShineEffect;

	private PLAYER_SKILL_TYPE m_skillType;

	private PlayerSkillData m_data;

	[SerializeField]
	private AnimationCurve _animationCurve;

	private Vector2 _defaultContentSize;

	private void Awake()
	{
		_defaultContentSize = rectTransformContent.sizeDelta;
	}

	public void InitItem(PLAYER_SKILL_TYPE p_type)
	{
		mainButton.playClickAudio = false;
		m_data = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(p_type);
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(p_type);
		string text = Utilities.ColorizeSpellTitle(skillData.localizedName ?? "") ?? "";
		txtSkillName.text = text;
		txtDescription.text = skillData.localizedDescription;
		imgIcon.sprite = m_data.skillIcon;
		txtLevel.text = "Level 0";
		m_skillType = p_type;
		base.transform.localScale = Vector3.one;
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
