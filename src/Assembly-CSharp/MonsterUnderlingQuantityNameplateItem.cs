using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MonsterUnderlingQuantityNameplateItem : NameplateItem<MonsterAndDemonUnderlingCharges>
{
	[Header("Attributes")]
	public CharacterPortrait portrait;

	[SerializeField]
	private RuinarchText txtHp;

	[SerializeField]
	private RuinarchText txtAttack;

	[SerializeField]
	private RuinarchText txtAttackSpeed;

	[SerializeField]
	private RuinarchText txtManaCost;

	[SerializeField]
	private RuinarchText txtElement;

	[SerializeField]
	private HoverHandler hpHoverHandler;

	[SerializeField]
	private HoverHandler attackHoverHandler;

	[SerializeField]
	private HoverHandler manaHoverHandler;

	[SerializeField]
	private HoverHandler chargesHoverHandler;

	[SerializeField]
	private GameObject manaCostGO;

	[SerializeField]
	private Image cooldownCoverImage;

	[SerializeField]
	private HoverHandler hoverHandler;

	[SerializeField]
	private Image statsContainerBG;

	[SerializeField]
	private Sprite bgStrength;

	[SerializeField]
	private Sprite bgInt;

	private MonsterAndDemonUnderlingCharges _monsterOrMinion;

	private MinionPlayerSkill _demonPlayerSkill;

	public override MonsterAndDemonUnderlingCharges obj => _monsterOrMinion;

	public int summonCost { get; private set; }

	public bool shouldNotShowManaCost { get; private set; }

	private void Awake()
	{
		hpHoverHandler.AddOnHoverOverAction(OnHoverOverHP);
		attackHoverHandler.AddOnHoverOverAction(OnHoverOverStrInt);
		manaHoverHandler.AddOnHoverOverAction(OnHoverOverMana);
		chargesHoverHandler.AddOnHoverOverAction(OnHoverOverCharges);
		hpHoverHandler.AddOnHoverOutAction(UIManager.Instance.HideSmallInfo);
		attackHoverHandler.AddOnHoverOutAction(UIManager.Instance.HideSmallInfo);
		manaHoverHandler.AddOnHoverOutAction(UIManager.Instance.HideSmallInfo);
		chargesHoverHandler.AddOnHoverOutAction(UIManager.Instance.HideSmallInfo);
	}

	public override void SetObject(MonsterAndDemonUnderlingCharges o)
	{
		base.SetObject(o);
		_monsterOrMinion = o;
		if (_monsterOrMinion.isDemon)
		{
			_demonPlayerSkill = PlayerSkillManager.Instance.GetMinionPlayerSkillDataByMinionType(_monsterOrMinion.minionType);
		}
		UpdateVisuals();
		UpdateBasicData();
		UpdateCooldownState();
		SubscribeToSignals();
	}

	public void UpdateBasicData()
	{
		UpdateMainText();
		UpdateQuantityText();
	}

	private void UpdateVisuals()
	{
		if (CharacterManager.Instance.GetCharacterClass(_monsterOrMinion.characterClassName).attackType == ATTACK_TYPE.PHYSICAL)
		{
			statsContainerBG.sprite = bgStrength;
		}
		else
		{
			statsContainerBG.sprite = bgInt;
		}
		if (_monsterOrMinion.isDemon)
		{
			portrait.GeneratePortrait(_monsterOrMinion.minionType);
		}
		else if (_monsterOrMinion.monsterType != SUMMON_TYPE.None)
		{
			portrait.GeneratePortrait(_monsterOrMinion.monsterType);
		}
	}

	private void UpdateMainText()
	{
		CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(obj.characterClassName);
		txtHp.text = characterClass.baseHP.ToString();
		txtAttack.text = characterClass.GetBaseDPS().ToString();
		string text = ((characterClass.attackType == ATTACK_TYPE.PHYSICAL) ? LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Physical") : LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Magical"));
		string key = characterClass.elementalType.ToStringEnumWithIcon();
		txtElement.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", key) + "\n" + text;
		txtAttackSpeed.text = $"{(float)characterClass.baseAttackSpeed / 1000f}s";
		summonCost = CharacterManager.Instance.GetCharacterClass(characterClass.className).GetSummonCost();
		if (shouldNotShowManaCost)
		{
			txtManaCost.text = "--";
		}
		else
		{
			txtManaCost.text = summonCost.ToString();
		}
		if (_monsterOrMinion.isDemon)
		{
			string demonNameColorHex = CharacterManager.Instance.demonNameColorHex;
			mainLbl.text = "<b><color=#" + demonNameColorHex + ">" + characterClass.displayName + "</color></b>";
		}
		else if (_monsterOrMinion.monsterType != SUMMON_TYPE.None)
		{
			string normalNameColorHex = CharacterManager.Instance.normalNameColorHex;
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("CharacterClasses_Table", _monsterOrMinion.monsterType.ToStringEnumWithSpace());
			mainLbl.text = "<b><color=#" + normalNameColorHex + ">" + localizedValue + "</color></b>";
		}
	}

	public void UpdateQuantityText()
	{
		int num = (m_displayRemainingChargeText = (_monsterOrMinion.isDemon ? PlayerManager.Instance.player.GetNumberOfAliveMonstersInPlayerFaction(_monsterOrMinion.minionType) : _monsterOrMinion.currentCharges));
		m_displayMaxChrageText = _monsterOrMinion.maxCharges;
		subLbl.text = num + "/" + _monsterOrMinion.maxCharges;
	}

	public void SetShouldNotShowManaCost(bool p_state)
	{
		shouldNotShowManaCost = p_state;
	}

	private void SubscribeToSignals()
	{
		Messenger.AddListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_STARTED, OnSpellCooldownStarted);
		Messenger.AddListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
		Messenger.AddListener<MonsterAndDemonUnderlingCharges>(PlayerSkillSignals.START_MONSTER_UNDERLING_COOLDOWN, OnStartMonsterUnderlingCooldown);
		Messenger.AddListener<MonsterAndDemonUnderlingCharges>(PlayerSkillSignals.STOP_MONSTER_UNDERLING_COOLDOWN, OnStopMonsterUnderlingCooldown);
		Messenger.AddListener<MinionPlayerSkill>(PlayerSkillSignals.PER_TICK_DEMON_COOLDOWN, PerTickDemonCooldownListener);
		Messenger.AddListener<MonsterAndDemonUnderlingCharges>(PlayerSkillSignals.PER_TICK_MONSTER_UNDERLING_COOLDOWN, PerTickMonsterUnderlingCooldownListener);
		Messenger.AddListener<SkillData>(PlayerSkillSignals.PLAYER_NO_ACTIVE_SPELL, OnPlayerNoActiveSpell);
	}

	private void UnsubscribeToSignals()
	{
		Messenger.RemoveListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_STARTED, OnSpellCooldownStarted);
		Messenger.RemoveListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
		Messenger.RemoveListener<MonsterAndDemonUnderlingCharges>(PlayerSkillSignals.START_MONSTER_UNDERLING_COOLDOWN, OnStartMonsterUnderlingCooldown);
		Messenger.RemoveListener<MonsterAndDemonUnderlingCharges>(PlayerSkillSignals.STOP_MONSTER_UNDERLING_COOLDOWN, OnStopMonsterUnderlingCooldown);
		Messenger.RemoveListener<MinionPlayerSkill>(PlayerSkillSignals.PER_TICK_DEMON_COOLDOWN, PerTickDemonCooldownListener);
		Messenger.RemoveListener<MonsterAndDemonUnderlingCharges>(PlayerSkillSignals.PER_TICK_MONSTER_UNDERLING_COOLDOWN, PerTickMonsterUnderlingCooldownListener);
		Messenger.RemoveListener<SkillData>(PlayerSkillSignals.PLAYER_NO_ACTIVE_SPELL, OnPlayerNoActiveSpell);
	}

	private void OnSpellCooldownStarted(SkillData data)
	{
		if (_demonPlayerSkill == data)
		{
			StartCooldownFillDemon();
		}
	}

	private void OnSpellCooldownFinished(SkillData data)
	{
		if (_demonPlayerSkill == data)
		{
			StopCooldownFill();
		}
	}

	private void OnStartMonsterUnderlingCooldown(MonsterAndDemonUnderlingCharges data)
	{
		if (_monsterOrMinion == data)
		{
			StartCooldownFillMonster();
		}
	}

	private void OnStopMonsterUnderlingCooldown(MonsterAndDemonUnderlingCharges data)
	{
		if (_monsterOrMinion == data)
		{
			StopCooldownFill();
		}
	}

	private void PerTickMonsterUnderlingCooldownListener(MonsterAndDemonUnderlingCharges data)
	{
		if (_monsterOrMinion == data)
		{
			PerTickCooldownMonster();
		}
	}

	private void PerTickDemonCooldownListener(MinionPlayerSkill data)
	{
		if (_demonPlayerSkill == data)
		{
			PerTickCooldownDemon();
		}
	}

	private void OnPlayerNoActiveSpell(SkillData spellData)
	{
		if (spellData is SummonPlayerSkill summonPlayerSkill && summonPlayerSkill.summonType == _monsterOrMinion.monsterType)
		{
			if (_toggle.isOn)
			{
				_toggle.isOn = false;
			}
		}
		else if (spellData is MinionPlayerSkill minionPlayerSkill && minionPlayerSkill.minionType == _monsterOrMinion.minionType && _toggle.isOn)
		{
			_toggle.isOn = false;
		}
	}

	private void UpdateCooldownState()
	{
		if (_monsterOrMinion.isReplenishing)
		{
			StartCooldownFillMonster();
		}
		else if (_demonPlayerSkill != null && _demonPlayerSkill.isInCooldown)
		{
			StartCooldownFillDemon();
		}
		else
		{
			StopCooldownFill();
		}
	}

	private void StartCooldownFillDemon()
	{
		cooldownCoverImage.fillAmount = (float)_demonPlayerSkill.currentCooldownTick / (float)_demonPlayerSkill.cooldown;
		cooldownCoverImage.gameObject.SetActive(value: true);
	}

	private void StartCooldownFillMonster()
	{
		cooldownCoverImage.fillAmount = (float)_monsterOrMinion.currentCooldownTick / (float)_monsterOrMinion.cooldown;
		cooldownCoverImage.gameObject.SetActive(value: true);
	}

	private void PerTickCooldownDemon()
	{
		float endValue = (float)_demonPlayerSkill.currentCooldownTick / (float)_demonPlayerSkill.cooldown;
		cooldownCoverImage.DOFillAmount(endValue, 0.4f);
	}

	private void PerTickCooldownMonster()
	{
		float endValue = (float)_monsterOrMinion.currentCooldownTick / (float)_monsterOrMinion.cooldown;
		cooldownCoverImage.DOFillAmount(endValue, 0.4f);
	}

	private void StopCooldownFill()
	{
		cooldownCoverImage.fillAmount = 0f;
		cooldownCoverImage.gameObject.SetActive(value: false);
	}

	public void SetHoverHandlerExecutePerFrame(bool p_state)
	{
		hoverHandler.ExecuteHoverEnterActionPerFrame(p_state);
	}

	private void OnHoverOverHP()
	{
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Health"));
	}

	private void OnHoverOverMana()
	{
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Summon Cost"), "", autoReplaceText: false);
	}

	private void OnHoverOverCharges()
	{
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Charges"), "", autoReplaceText: false);
	}

	private void OnHoverOverStrInt()
	{
		if (CharacterManager.Instance.GetCharacterClass(_monsterOrMinion.characterClassName).attackType == ATTACK_TYPE.PHYSICAL)
		{
			UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Strength"), "", autoReplaceText: false);
		}
		else
		{
			UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Intelligence"), "", autoReplaceText: false);
		}
	}

	public override void Reset()
	{
		base.Reset();
		portrait.ClearPointerClickAction();
		_monsterOrMinion = null;
		_demonPlayerSkill = null;
		StopCooldownFill();
		UnsubscribeToSignals();
		SetHoverHandlerExecutePerFrame(p_state: true);
	}
}
