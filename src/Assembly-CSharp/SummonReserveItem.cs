using System;
using DG.Tweening;
using EZObjectPools;
using Ruinarch.Custom_UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummonReserveItem : PooledObject
{
	[SerializeField]
	private RuinarchToggle toggle;

	[SerializeField]
	private HoverHandler hoverHandler;

	[SerializeField]
	private Image imgCover;

	[Header("Charges")]
	[SerializeField]
	private GameObject goCharges;

	[SerializeField]
	private TextMeshProUGUI lblCharges;

	[Header("Portrait")]
	[SerializeField]
	private Image imgPortrait;

	[Header("Cooldown")]
	[SerializeField]
	private Image imgCooldown;

	[SerializeField]
	private GameObject goCooldown;

	[Header("Tooltip")]
	[SerializeField]
	private MonsterUnderlingQuantityNameplateItem tooltip;

	private MonsterAndDemonUnderlingCharges _data;

	private MinionPlayerSkill _demonPlayerSkill;

	private Action<MonsterAndDemonUnderlingCharges, bool> _onToggleAction;

	private Action<SummonReserveItem> _onHoverOverAction;

	private Action<SummonReserveItem> _onHoverOutAction;

	public MonsterAndDemonUnderlingCharges data => _data;

	public MonsterUnderlingQuantityNameplateItem nameplateTooltip => tooltip;

	private void OnEnable()
	{
		toggle.onValueChanged.AddListener(OnToggle);
		hoverHandler.AddOnHoverOverAction(OnHoverOverItem);
		hoverHandler.AddOnHoverOutAction(OnHoverOutItem);
	}

	private void OnDisable()
	{
		toggle.onValueChanged.RemoveListener(OnToggle);
		hoverHandler.RemoveOnHoverOverAction(OnHoverOverItem);
		hoverHandler.RemoveOnHoverOutAction(OnHoverOutItem);
		tooltip.gameObject.SetActive(value: false);
	}

	public void Initialize(MonsterAndDemonUnderlingCharges p_data, ToggleGroup p_toggleGroup, Action<MonsterAndDemonUnderlingCharges, bool> p_onToggleAction, Action<SummonReserveItem> p_onHoverOverAction, Action<SummonReserveItem> p_onHoverOutAction)
	{
		_data = p_data;
		if (p_data.isDemon)
		{
			_demonPlayerSkill = PlayerSkillManager.Instance.GetMinionPlayerSkillDataByMinionType(p_data.minionType);
		}
		tooltip.SetObject(p_data);
		_onToggleAction = p_onToggleAction;
		_onHoverOverAction = p_onHoverOverAction;
		_onHoverOutAction = p_onHoverOutAction;
		toggle.group = p_toggleGroup;
		toggle.interactable = false;
		UpdatePortrait();
		UpdateBasicData();
		UpdateCooldownState();
		SubscribeToSignals();
	}

	public void UpdateBasicData()
	{
		UpdateQuantityText();
	}

	private void UpdatePortrait()
	{
		CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(_data.characterClassName);
		imgPortrait.sprite = characterClass.smallPortraitSprite;
	}

	private void UpdateQuantityText()
	{
		if (_data.isDemon)
		{
			goCharges.SetActive(value: false);
			return;
		}
		goCharges.SetActive(value: true);
		lblCharges.text = data.currentCharges.ToString();
	}

	public void SetInteractableState(bool p_state)
	{
		toggle.interactable = p_state;
	}

	private void OnToggle(bool p_isOn)
	{
		_onToggleAction?.Invoke(_data, p_isOn);
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
		if (_data == data)
		{
			StartCooldownFillMonster();
		}
	}

	private void OnStopMonsterUnderlingCooldown(MonsterAndDemonUnderlingCharges data)
	{
		if (_data == data)
		{
			StopCooldownFill();
		}
	}

	private void PerTickMonsterUnderlingCooldownListener(MonsterAndDemonUnderlingCharges data)
	{
		if (_data == data)
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
		if (spellData is SummonPlayerSkill summonPlayerSkill && summonPlayerSkill.summonType == _data.monsterType)
		{
			if (toggle.isOn)
			{
				toggle.isOn = false;
			}
		}
		else if (spellData is MinionPlayerSkill minionPlayerSkill && minionPlayerSkill.minionType == _data.minionType && toggle.isOn)
		{
			toggle.isOn = false;
		}
	}

	private void UpdateCooldownState()
	{
		if (_data.isReplenishing)
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
		if (WorldSettings.Instance.worldSettingsData.playerSkillSettings.cooldownSpeed != SKILL_COOLDOWN_SPEED.None)
		{
			imgCooldown.fillAmount = 1f - (float)_demonPlayerSkill.currentCooldownTick / (float)_demonPlayerSkill.cooldown;
			goCooldown.SetActive(value: true);
		}
	}

	private void StartCooldownFillMonster()
	{
		if (WorldSettings.Instance.worldSettingsData.playerSkillSettings.cooldownSpeed != SKILL_COOLDOWN_SPEED.None)
		{
			imgCooldown.fillAmount = 1f - (float)_data.currentCooldownTick / (float)_data.cooldown;
			goCooldown.SetActive(value: true);
		}
	}

	private void PerTickCooldownDemon()
	{
		float endValue = 1f - (float)_demonPlayerSkill.currentCooldownTick / (float)_demonPlayerSkill.cooldown;
		imgCooldown.DOFillAmount(endValue, 0.4f);
	}

	private void PerTickCooldownMonster()
	{
		float endValue = 1f - (float)_data.currentCooldownTick / (float)_data.cooldown;
		imgCooldown.DOFillAmount(endValue, 0.4f);
	}

	private void StopCooldownFill()
	{
		imgCooldown.fillAmount = 0f;
		goCooldown.SetActive(value: false);
	}

	private void OnHoverOverItem()
	{
		_onHoverOverAction?.Invoke(this);
	}

	private void OnHoverOutItem()
	{
		_onHoverOutAction?.Invoke(this);
	}

	public override void Reset()
	{
		base.Reset();
		_onToggleAction = null;
		_data = null;
		_demonPlayerSkill = null;
		StopCooldownFill();
		UnsubscribeToSignals();
	}
}
