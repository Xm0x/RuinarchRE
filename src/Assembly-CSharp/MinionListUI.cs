using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class MinionListUI : PopupMenuBase
{
	[Header("Minion List")]
	[SerializeField]
	private GameObject activeMinionItemPrefab;

	[SerializeField]
	private GameObject minionItemPrefab;

	[SerializeField]
	private ScrollRect minionListScrollView;

	[SerializeField]
	private Toggle minionListToggle;

	[SerializeField]
	private UIHoverPosition _hoverPosition;

	[Header("Monster Quantity Column")]
	[SerializeField]
	private GameObject quantityMonsterItemPrefab;

	[SerializeField]
	private ScrollRect quantityScrollView;

	[SerializeField]
	private MonsterToolTipUI monsterToolTipUI;

	private List<SummonMinionPlayerSkillNameplateItem> _minionItems;

	private List<MonsterUnderlingQuantityNameplateItem> _monsterUnderlingQuantityNameplateItems;

	public override void Open()
	{
		base.Open();
		UpdateMinionPlayerSkillItems();
	}

	public override void Close()
	{
		base.Close();
		monsterToolTipUI.HideToolTip();
		HideMinionList();
	}

	public void Initialize()
	{
		_minionItems = new List<SummonMinionPlayerSkillNameplateItem>();
		_monsterUnderlingQuantityNameplateItems = new List<MonsterUnderlingQuantityNameplateItem>();
		Messenger.AddListener<Minion>(PlayerSignals.PLAYER_GAINED_MINION, OnGainMinion);
		Messenger.AddListener<Minion>(PlayerSignals.PLAYER_LOST_MINION, OnLostMinion);
		Messenger.AddListener<MonsterAndDemonUnderlingCharges>(PlayerSignals.UPDATED_MONSTER_UNDERLING, OnUpdateMonsterUnderling);
		Messenger.AddListener<CHARACTER_CATEGORY, PRIMORDIAL_STATS_BONUS>(PlayerSkillSignals.ON_PRIMORDIAL_POOL_UPGRADE, OnUpgradePrimordialPool);
		Messenger.AddListener<Party, Character>(PartySignals.CHARACTER_JOINED_PARTY, OnAddPartyMember);
		Messenger.AddListener<Party, Character>(PartySignals.CHARACTER_LEFT_PARTY, OnRemovePartyMember);
	}

	public void UpdateList()
	{
		for (int i = 0; i < PlayerManager.Instance.player.playerFaction.characters.Count; i++)
		{
			Character character = PlayerManager.Instance.player.playerFaction.characters[i];
			if (character.minion != null && !character.isDead)
			{
				CreateNewActiveMinionItem(character.minion);
			}
		}
	}

	private void UpdateMinionPlayerSkillItems()
	{
		for (int i = 0; i < _minionItems.Count; i++)
		{
			_minionItems[i].UpdateAllData();
		}
	}

	private void CreateNewActiveMinionItem(Minion minion)
	{
		CharacterNameplateItem component = ObjectPoolManager.Instance.InstantiateObjectFromPool(activeMinionItemPrefab.name, Vector3.zero, Quaternion.identity, minionListScrollView.content).GetComponent<CharacterNameplateItem>();
		component.SetObject(minion.character);
		component.SetAsDefaultBehaviour();
		component.AddOnClickAction(delegate(Character c)
		{
			UIManager.Instance.ShowCharacterInfo(c, centerOnCharacter: true);
		});
		component.AddHoverEnterAction(delegate(Character data)
		{
			OnHoverEnterActiveMinion(data);
		});
		component.AddHoverExitAction(OnHoverExitActiveMinion);
	}

	private void CreateNewReserveMinionItem(PLAYER_SKILL_TYPE minionPlayerSkillType)
	{
	}

	private void DeleteMinionItem(Minion minion)
	{
		CharacterNameplateItem minionItem = GetMinionItem(minion);
		if (minionItem != null)
		{
			ObjectPoolManager.Instance.DestroyObject(minionItem);
		}
	}

	private CharacterNameplateItem GetMinionItem(Minion minion)
	{
		CharacterNameplateItem[] componentsInDirectChildren = GameUtilities.GetComponentsInDirectChildren<CharacterNameplateItem>(minionListScrollView.content.gameObject);
		foreach (CharacterNameplateItem characterNameplateItem in componentsInDirectChildren)
		{
			if (characterNameplateItem.character == minion.character)
			{
				return characterNameplateItem;
			}
		}
		return null;
	}

	private void OnGainPlayerMinionSkill(PLAYER_SKILL_TYPE minionPlayerSkillType)
	{
		CreateNewReserveMinionItem(minionPlayerSkillType);
	}

	private void OnGainMinion(Minion minion)
	{
		CreateNewActiveMinionItem(minion);
		UpdateMinionPlayerSkillItems();
	}

	private void OnLostMinion(Minion minion)
	{
		DeleteMinionItem(minion);
	}

	public void ToggleMinionList(bool isOn)
	{
		if (isOn)
		{
			Open();
		}
		else
		{
			Close();
		}
	}

	public void HideMinionList()
	{
		if (minionListToggle.isOn)
		{
			minionListToggle.isOn = false;
		}
	}

	private void OnHoverEnterActiveMinion(Character p_character)
	{
		CharacterCombatBehaviour currentCombatBehaviour = p_character.combatComponent.combatBehaviourParent.currentCombatBehaviour;
		if (currentCombatBehaviour != null)
		{
			monsterToolTipUI.DisplayToolTipWithoutCharge(currentCombatBehaviour.displayName, currentCombatBehaviour.description);
		}
	}

	private void OnHoverExitActiveMinion(Character p_character)
	{
		monsterToolTipUI.HideToolTip();
		UIManager.Instance.HideSmallInfo();
	}

	private void UpdateBasicDataUnderlingItems()
	{
		for (int i = 0; i < _monsterUnderlingQuantityNameplateItems.Count; i++)
		{
			_monsterUnderlingQuantityNameplateItems[i].UpdateBasicData();
		}
	}

	private MonsterUnderlingQuantityNameplateItem CreateNewMonsterUnderlingQuantityItem(MonsterAndDemonUnderlingCharges p_underlingCharges)
	{
		GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(quantityMonsterItemPrefab.name, Vector3.zero, Quaternion.identity, quantityScrollView.content);
		MonsterUnderlingQuantityNameplateItem component = obj.GetComponent<MonsterUnderlingQuantityNameplateItem>();
		component.SetObject(p_underlingCharges);
		component.SetAsToggle();
		component.AddOnToggleAction(OnToggleMonsterUnderlingData);
		component.AddHoverEnterAction(OnHoverEnterDemonUnderlingData);
		component.AddHoverExitAction(OnHoverExitDemonUnderlingData);
		obj.SetActive(p_underlingCharges.hasMaxCharge);
		_monsterUnderlingQuantityNameplateItems.Add(component);
		UpdateMonsterUnderlingInteractableState(component);
		return component;
	}

	private void OnToggleMonsterUnderlingData(MonsterAndDemonUnderlingCharges p_data, bool isOn)
	{
		if (isOn)
		{
			MinionPlayerSkill minionPlayerSkillDataByMinionType = PlayerSkillManager.Instance.GetMinionPlayerSkillDataByMinionType(p_data.minionType);
			if (minionPlayerSkillDataByMinionType != null)
			{
				PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
				PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(minionPlayerSkillDataByMinionType);
			}
		}
		else
		{
			PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
		}
	}

	private MonsterUnderlingQuantityNameplateItem GetMonsterUnderlingQuantityNameplateItem(MonsterAndDemonUnderlingCharges p_underlingCharges)
	{
		for (int i = 0; i < _monsterUnderlingQuantityNameplateItems.Count; i++)
		{
			MonsterUnderlingQuantityNameplateItem monsterUnderlingQuantityNameplateItem = _monsterUnderlingQuantityNameplateItems[i];
			if (monsterUnderlingQuantityNameplateItem.obj == p_underlingCharges)
			{
				return monsterUnderlingQuantityNameplateItem;
			}
		}
		return null;
	}

	private void DeleteMonsterUnderlingItem(MonsterAndDemonUnderlingCharges p_underlingCharges)
	{
		MonsterUnderlingQuantityNameplateItem monsterUnderlingQuantityNameplateItem = GetMonsterUnderlingQuantityNameplateItem(p_underlingCharges);
		if (monsterUnderlingQuantityNameplateItem != null)
		{
			ObjectPoolManager.Instance.DestroyObject(monsterUnderlingQuantityNameplateItem);
		}
	}

	public void UpdateMonsterUnderlingQuantityList()
	{
		Player player = PlayerManager.Instance.player;
		if (player == null)
		{
			return;
		}
		foreach (MonsterAndDemonUnderlingCharges value in player.underlingsComponent.monsterUnderlingCharges.Values)
		{
			CreateNewMonsterUnderlingQuantityItem(value);
		}
	}

	private void OnUpdateMonsterUnderling(MonsterAndDemonUnderlingCharges p_underlingCharges)
	{
		if (p_underlingCharges.isDemon)
		{
			MonsterUnderlingQuantityNameplateItem monsterUnderlingQuantityNameplateItem = GetMonsterUnderlingQuantityNameplateItem(p_underlingCharges);
			if (monsterUnderlingQuantityNameplateItem != null)
			{
				monsterUnderlingQuantityNameplateItem.UpdateBasicData();
				monsterUnderlingQuantityNameplateItem.gameObject.SetActive(p_underlingCharges.hasMaxCharge);
				UpdateMonsterUnderlingInteractableState(monsterUnderlingQuantityNameplateItem);
			}
			else
			{
				CreateNewMonsterUnderlingQuantityItem(p_underlingCharges);
			}
		}
	}

	private void OnHoverEnterDemonUnderlingData(MonsterAndDemonUnderlingCharges p_data)
	{
		if (!PlayerManager.Instance.player.underlingsComponent.CanStillSpawnPlayerDefenders())
		{
			UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Defender_Cap"));
		}
		string empty = string.Empty;
		MinionPlayerSkill minionPlayerSkillDataByMinionType = PlayerSkillManager.Instance.GetMinionPlayerSkillDataByMinionType(p_data.minionType);
		CharacterCombatBehaviour characterCombatBehaviour = null;
		if (minionPlayerSkillDataByMinionType != null)
		{
			CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(minionPlayerSkillDataByMinionType.className);
			if (characterClass.combatBehaviourType == CHARACTER_COMBAT_BEHAVIOUR.None)
			{
				return;
			}
			characterCombatBehaviour = CombatManager.Instance.GetCombatBehaviour(characterClass.combatBehaviourType);
		}
		empty = PlayerUI.Instance.OnHoverSpellChargeRemaining(minionPlayerSkillDataByMinionType, p_data);
		if (string.IsNullOrEmpty(empty))
		{
			monsterToolTipUI.DisplayToolTipWithoutCharge(characterCombatBehaviour.displayName, characterCombatBehaviour.description);
		}
		else
		{
			monsterToolTipUI.DisplayToolTipWithCharge(characterCombatBehaviour.displayName, characterCombatBehaviour.description, empty);
		}
	}

	private void OnHoverExitDemonUnderlingData(MonsterAndDemonUnderlingCharges p_data)
	{
		monsterToolTipUI.HideToolTip();
		Tooltip.Instance.HideSmallInfo();
		UIManager.Instance.HideSmallInfo();
		PlayerUI.Instance.skillDetailsTooltip.HidePlayerSkillDetails();
	}

	private void UpdateMonsterUnderlingInteractableState(MonsterUnderlingQuantityNameplateItem p_item)
	{
		p_item.SetInteractableState(PlayerManager.Instance.player.underlingsComponent.CanStillSpawnPlayerDefenders() && p_item.obj.CanBeCasted());
	}

	private void UpdateInteractableStateOfMonsterUnderlingNameplates()
	{
		for (int i = 0; i < _monsterUnderlingQuantityNameplateItems.Count; i++)
		{
			MonsterUnderlingQuantityNameplateItem p_item = _monsterUnderlingQuantityNameplateItems[i];
			UpdateMonsterUnderlingInteractableState(p_item);
		}
	}

	private void OnUpgradePrimordialPool(CHARACTER_CATEGORY p_category, PRIMORDIAL_STATS_BONUS p_bonusType)
	{
		UpdateBasicDataUnderlingItems();
	}

	private void OnAddPartyMember(Party p_party, Character p_character)
	{
		if (p_party == PlayerManager.Instance.player.underlingsComponent.persistentDefendParty)
		{
			UpdateInteractableStateOfMonsterUnderlingNameplates();
		}
	}

	private void OnRemovePartyMember(Party p_party, Character p_character)
	{
		if (p_party == PlayerManager.Instance.player.underlingsComponent.persistentDefendParty)
		{
			UpdateInteractableStateOfMonsterUnderlingNameplates();
		}
	}

	public void OnPortalUpgrade()
	{
		UpdateInteractableStateOfMonsterUnderlingNameplates();
	}
}
