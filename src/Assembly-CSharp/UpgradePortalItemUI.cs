using System;
using Coffee.UIExtensions;
using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePortalItemUI : PooledObject
{
	[SerializeField]
	private BaseCharacterPortrait characterPortrait;

	[SerializeField]
	private BaseLocationPortrait locationPortrait;

	[SerializeField]
	private GameObject goPassiveSkillPortrait;

	[SerializeField]
	private GameObject goSpellPortrait;

	[SerializeField]
	private Image imgSpellPortrait;

	[SerializeField]
	private HoverHandler hoverHandlerSpellPortrait;

	[SerializeField]
	private TextMeshProUGUI lblName;

	[SerializeField]
	private RectTransform _contentParent;

	[SerializeField]
	private CanvasGroup _canvasGroupContent;

	[SerializeField]
	private AnimationCurve _revealAnimationCurve;

	public UIShiny characterShineEffect;

	public UIShiny locationShineEffect;

	public UIShiny powerShineEffect;

	[Header("Icons")]
	[SerializeField]
	private Sprite spriteAffliction;

	[SerializeField]
	private Sprite spriteSpell;

	[SerializeField]
	private Sprite spritePlayerAction;

	[SerializeField]
	private Sprite spriteMinion;

	[SerializeField]
	private Sprite spriteStructure;

	private PortalUpgradeItem _upgrade;

	private Action<UpgradePortalItemUI> _onHoverOverItem;

	private Action<UpgradePortalItemUI> _onHoverOutItem;

	private bool _allowHoverInteraction = true;

	private Vector2 _defaultContentSize;

	public RectTransform contentParent => _contentParent;

	public CanvasGroup canvasGroupContent => _canvasGroupContent;

	public PortalUpgradeItem upgrade => _upgrade;

	private void Awake()
	{
		characterPortrait.AddHoverOverAction(OnHoverOverItem);
		locationPortrait.AddHoverOverAction(OnHoverOverItem);
		hoverHandlerSpellPortrait.AddOnHoverOverAction(OnHoverOverItem);
		characterPortrait.AddHoverOutAction(OnHoverOutItem);
		locationPortrait.AddHoverOutAction(OnHoverOutItem);
		hoverHandlerSpellPortrait.AddOnHoverOutAction(OnHoverOutItem);
		_defaultContentSize = _contentParent.sizeDelta;
	}

	public void SetData(PortalUpgradeItem p_upgrade)
	{
		_upgrade = p_upgrade;
		if (p_upgrade.portalUpgradeType == Portal_Upgrade_Type.Essential)
		{
			SetPower(upgrade.essentialSkillToUnlock);
		}
		else if (upgrade.portalUpgradeType == Portal_Upgrade_Type.Optional_Other)
		{
			Sprite sprite = spriteSpell;
			locationPortrait.gameObject.SetActive(value: false);
			characterPortrait.gameObject.SetActive(value: false);
			goPassiveSkillPortrait.SetActive(value: false);
			goSpellPortrait.SetActive(value: true);
			imgSpellPortrait.sprite = sprite;
			lblName.text = "Random Tier " + upgrade.otherArchetypeTier + " Power";
		}
		else if (upgrade.portalUpgradeType == Portal_Upgrade_Type.Optional_Self)
		{
			Sprite sprite2 = spriteSpell;
			locationPortrait.gameObject.SetActive(value: false);
			characterPortrait.gameObject.SetActive(value: false);
			goPassiveSkillPortrait.SetActive(value: false);
			goSpellPortrait.SetActive(value: true);
			imgSpellPortrait.sprite = sprite2;
			lblName.text = "Random Tier " + upgrade.tier + " Power";
		}
		else if (upgrade.portalUpgradeType == Portal_Upgrade_Type.Common_Structures)
		{
			Sprite sprite3 = spriteStructure;
			locationPortrait.gameObject.SetActive(value: false);
			characterPortrait.gameObject.SetActive(value: false);
			goPassiveSkillPortrait.SetActive(value: false);
			goSpellPortrait.SetActive(value: true);
			imgSpellPortrait.sprite = sprite3;
			lblName.text = "Random Common Structure";
		}
		else if (upgrade.portalUpgradeType == Portal_Upgrade_Type.Lesser_Demon)
		{
			Sprite sprite4 = spriteMinion;
			locationPortrait.gameObject.SetActive(value: false);
			characterPortrait.gameObject.SetActive(value: false);
			goPassiveSkillPortrait.SetActive(value: false);
			goSpellPortrait.SetActive(value: true);
			imgSpellPortrait.sprite = sprite4;
			lblName.text = "Random Lesser Demon";
		}
		else if (upgrade.portalUpgradeType == Portal_Upgrade_Type.Wildcard)
		{
			Sprite sprite5 = spriteSpell;
			locationPortrait.gameObject.SetActive(value: false);
			characterPortrait.gameObject.SetActive(value: false);
			goPassiveSkillPortrait.SetActive(value: false);
			goSpellPortrait.SetActive(value: true);
			imgSpellPortrait.sprite = sprite5;
			lblName.text = "Wildcard";
		}
	}

	private void SetPower(PLAYER_SKILL_TYPE p_skillType)
	{
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(p_skillType);
		if (skillData.category == PLAYER_SKILL_CATEGORY.MINION)
		{
			MinionPlayerSkill minionPlayerSkillData = PlayerSkillManager.Instance.GetMinionPlayerSkillData(p_skillType);
			locationPortrait.gameObject.SetActive(value: false);
			characterPortrait.gameObject.SetActive(value: true);
			goPassiveSkillPortrait.SetActive(value: false);
			goSpellPortrait.SetActive(value: false);
			characterPortrait.GeneratePortrait(minionPlayerSkillData.minionType);
			lblName.text = minionPlayerSkillData.localizedName;
		}
		else if (skillData.category == PLAYER_SKILL_CATEGORY.DEMONIC_STRUCTURE)
		{
			DemonicStructurePlayerSkill demonicStructureSkillData = PlayerSkillManager.Instance.GetDemonicStructureSkillData(p_skillType);
			locationPortrait.gameObject.SetActive(value: true);
			characterPortrait.gameObject.SetActive(value: false);
			goPassiveSkillPortrait.SetActive(value: false);
			goSpellPortrait.SetActive(value: false);
			locationPortrait.SetPortrait(demonicStructureSkillData.structureType);
			lblName.text = demonicStructureSkillData.localizedName;
		}
		else
		{
			Sprite powerSprite = GetPowerSprite(skillData.category);
			locationPortrait.gameObject.SetActive(value: false);
			characterPortrait.gameObject.SetActive(value: false);
			goPassiveSkillPortrait.SetActive(value: false);
			goSpellPortrait.SetActive(value: true);
			imgSpellPortrait.sprite = powerSprite;
			lblName.text = skillData.localizedName;
		}
	}

	private Sprite GetPowerSprite(PLAYER_SKILL_CATEGORY p_category)
	{
		return p_category switch
		{
			PLAYER_SKILL_CATEGORY.SPELL => spriteSpell, 
			PLAYER_SKILL_CATEGORY.AFFLICTION => spriteAffliction, 
			PLAYER_SKILL_CATEGORY.PLAYER_ACTION => spritePlayerAction, 
			PLAYER_SKILL_CATEGORY.DEMONIC_STRUCTURE => spriteStructure, 
			PLAYER_SKILL_CATEGORY.MINION => spriteMinion, 
			_ => null, 
		};
	}

	public void AddHoverOverAction(Action<UpgradePortalItemUI> p_action)
	{
		_onHoverOverItem = (Action<UpgradePortalItemUI>)Delegate.Combine(_onHoverOverItem, p_action);
	}

	public void AddHoverOutAction(Action<UpgradePortalItemUI> p_action)
	{
		_onHoverOutItem = (Action<UpgradePortalItemUI>)Delegate.Combine(_onHoverOutItem, p_action);
	}

	private void OnHoverOverItem()
	{
		if (_allowHoverInteraction)
		{
			_onHoverOverItem?.Invoke(this);
		}
	}

	private void OnHoverOutItem()
	{
		if (_allowHoverInteraction)
		{
			_onHoverOutItem?.Invoke(this);
		}
	}

	public void SetHoverInteractionAllowedState(bool p_state)
	{
		_allowHoverInteraction = p_state;
	}

	public override void Reset()
	{
		base.Reset();
		_allowHoverInteraction = true;
		_upgrade = null;
	}

	private UIShiny GetValidShineEffect()
	{
		if (characterShineEffect.gameObject.activeInHierarchy)
		{
			return characterShineEffect;
		}
		if (locationShineEffect.gameObject.activeInHierarchy)
		{
			return locationShineEffect;
		}
		return powerShineEffect;
	}
}
