using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SummonListUI : PopupMenuBase
{
	public GameObject goContent;

	[SerializeField]
	private UIHoverPosition _hoverPosition;

	[SerializeField]
	private Toggle _mainToggle;

	[SerializeField]
	private MonsterToolTipUI monsterToolTipUI;

	[SerializeField]
	private RectTransform rectMainLayoutGroup;

	[Header("Reserves")]
	[SerializeField]
	private GameObject prefabSummonReserveItem;

	[SerializeField]
	private Transform transformReservesParent;

	[SerializeField]
	private ToggleGroup tglGroupReserves;

	[SerializeField]
	private RectTransform rtDemonAndMonsterDivider;

	[Header("Active")]
	[SerializeField]
	private RuinarchText activeText;

	[SerializeField]
	private GameObject prefabSummonPartyItem;

	[SerializeField]
	private Transform transformActiveParent;

	[SerializeField]
	private GameObject goActive;

	[SerializeField]
	private GameObject goActiveBG;

	private List<SummonReserveItem> _summonReserveItems;

	private List<SummonPartyItem> _summonPartyItems;

	public override void Open()
	{
		base.Open();
		_mainToggle.SetIsOnWithoutNotify(value: true);
		goContent.SetActive(value: true);
		LayoutRebuilder.ForceRebuildLayoutImmediate(transformReservesParent as RectTransform);
		LayoutRebuilder.ForceRebuildLayoutImmediate(transformActiveParent as RectTransform);
		LayoutRebuilder.ForceRebuildLayoutImmediate(rectMainLayoutGroup);
		Messenger.Broadcast(UISignals.SUMMONS_LIST_OPENED);
	}

	public override void Close()
	{
		base.Close();
		monsterToolTipUI.HideToolTip();
		_mainToggle.SetIsOnWithoutNotify(value: false);
		goContent.SetActive(value: false);
	}

	protected override void OnGameObjectEnabled()
	{
		base.OnGameObjectEnabled();
		Messenger.Broadcast(UISignals.TOP_UI_ENABLED);
	}

	protected override void OnGameObjectDisabled()
	{
		base.OnGameObjectDisabled();
		Messenger.Broadcast(UISignals.TOP_UI_DISABLED);
	}

	public void Initialize()
	{
		_summonReserveItems = new List<SummonReserveItem>();
		_summonPartyItems = new List<SummonPartyItem>();
		Messenger.AddListener<MonsterAndDemonUnderlingCharges>(PlayerSignals.UPDATED_MONSTER_UNDERLING, OnUpdateMonsterUnderling);
		Messenger.AddListener<Party>(PartySignals.PARTY_CREATED, OnPartyCreated);
		Messenger.AddListener<Party, Character>(PartySignals.CHARACTER_LEFT_PARTY, OnRemovePartyMember);
		Messenger.AddListener<CHARACTER_CATEGORY, PRIMORDIAL_STATS_BONUS>(PlayerSkillSignals.ON_PRIMORDIAL_POOL_UPGRADE, OnUpgradePrimordialPool);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
	}

	public void UpdateList()
	{
		UpdateActiveSummonCount();
		UpdateActiveGameObject();
	}

	private void OnUpdateMonsterUnderling(MonsterAndDemonUnderlingCharges p_underlingCharges)
	{
		SummonReserveItem monsterUnderlingQuantityNameplateItem = GetMonsterUnderlingQuantityNameplateItem(p_underlingCharges);
		if (monsterUnderlingQuantityNameplateItem != null)
		{
			monsterUnderlingQuantityNameplateItem.UpdateBasicData();
			monsterUnderlingQuantityNameplateItem.gameObject.SetActive(p_underlingCharges.hasMaxCharge);
			UpdateSummonReserveInteractableState(monsterUnderlingQuantityNameplateItem);
		}
		else
		{
			CreateNewSummonReserveItem(p_underlingCharges);
		}
	}

	private void OnPartyCreated(Party p_party)
	{
		if (p_party.isPlayerParty && p_party != PlayerManager.Instance.player.underlingsComponent.persistentDefendParty && !HasSummonPartyItem(p_party))
		{
			CreateSummonPartyItem(p_party);
		}
	}

	private void OnRemovePartyMember(Party p_party, Character p_character)
	{
		if ((p_party.isPlayerParty || (p_character.faction != null && p_character.faction.isPlayerFaction)) && p_party.members.Count <= 0 && HasSummonPartyItem(p_party))
		{
			DeleteSummonPartyItem(p_party);
		}
	}

	private void OnCharacterDied(Character p_character)
	{
		if (!p_character.race.IsSapient())
		{
			SummonReserveItem monsterUnderlingQuantityNameplateItem = GetMonsterUnderlingQuantityNameplateItem(p_character);
			if (monsterUnderlingQuantityNameplateItem != null)
			{
				monsterUnderlingQuantityNameplateItem.UpdateBasicData();
				monsterUnderlingQuantityNameplateItem.gameObject.SetActive(monsterUnderlingQuantityNameplateItem.data.hasMaxCharge);
			}
		}
	}

	public void OnPortalUpgrade()
	{
		UpdateActiveSummonCount();
		UpdateInteractableStateOfSummonReserveItems();
	}

	private void OnUpgradePrimordialPool(CHARACTER_CATEGORY p_category, PRIMORDIAL_STATS_BONUS p_bonusType)
	{
	}

	private void OnHoverOverItem(SummonReserveItem p_item)
	{
		p_item.nameplateTooltip.gameObject.SetActive(value: true);
		p_item.nameplateTooltip.UpdateBasicData();
		UIManager.Instance.PositionTooltip(_hoverPosition, p_item.nameplateTooltip.gameObject, p_item.nameplateTooltip.transform as RectTransform);
		CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(p_item.data.characterClassName);
		CharacterCombatBehaviour combatBehaviour = CombatManager.Instance.GetCombatBehaviour(characterClass.combatBehaviourType);
		monsterToolTipUI.DisplayToolTipWithoutCharge(combatBehaviour.displayName, combatBehaviour.description);
	}

	private void OnHoverOutItem(SummonReserveItem p_item)
	{
		p_item.nameplateTooltip.gameObject.SetActive(value: false);
		monsterToolTipUI.HideToolTip();
	}

	private void CreateNewSummonReserveItem(MonsterAndDemonUnderlingCharges p_underlingCharges)
	{
		GameObject gameObject = ObjectPoolManager.Instance.InstantiateObjectFromPool(prefabSummonReserveItem.name, Vector3.zero, Quaternion.identity, transformReservesParent);
		SummonReserveItem component = gameObject.GetComponent<SummonReserveItem>();
		component.Initialize(p_underlingCharges, tglGroupReserves, OnToggleMonsterUnderlingData, OnHoverOverItem, OnHoverOutItem);
		UpdateSummonReserveInteractableState(component);
		if (p_underlingCharges.isDemon)
		{
			gameObject.transform.SetSiblingIndex(rtDemonAndMonsterDivider.GetSiblingIndex());
		}
		else
		{
			gameObject.transform.SetAsLastSibling();
		}
		rtDemonAndMonsterDivider.gameObject.SetActive(rtDemonAndMonsterDivider.GetSiblingIndex() < rtDemonAndMonsterDivider.parent.childCount - 1);
		_summonReserveItems.Add(component);
		component.gameObject.SetActive(p_underlingCharges.hasMaxCharge);
		LayoutRebuilder.ForceRebuildLayoutImmediate(rectMainLayoutGroup);
	}

	private void OnToggleMonsterUnderlingData(MonsterAndDemonUnderlingCharges p_data, bool isOn)
	{
	}

	private SummonReserveItem GetMonsterUnderlingQuantityNameplateItem(MonsterAndDemonUnderlingCharges p_underlingCharges)
	{
		for (int i = 0; i < _summonReserveItems.Count; i++)
		{
			SummonReserveItem summonReserveItem = _summonReserveItems[i];
			if (summonReserveItem.data == p_underlingCharges)
			{
				return summonReserveItem;
			}
		}
		return null;
	}

	private SummonReserveItem GetMonsterUnderlingQuantityNameplateItem(Character p_character)
	{
		for (int i = 0; i < _summonReserveItems.Count; i++)
		{
			SummonReserveItem summonReserveItem = _summonReserveItems[i];
			if (p_character.minion != null)
			{
				if (summonReserveItem.data.isDemon && summonReserveItem.data.minionType == p_character.minion.minionType)
				{
					return summonReserveItem;
				}
			}
			else if (p_character is Summon summon && !summonReserveItem.data.isDemon && summonReserveItem.data.monsterType == summon.summonType)
			{
				return summonReserveItem;
			}
		}
		return null;
	}

	public void UpdateInteractableStateOfSummonReserveItems()
	{
		for (int i = 0; i < _summonReserveItems.Count; i++)
		{
			SummonReserveItem p_item = _summonReserveItems[i];
			UpdateSummonReserveInteractableState(p_item);
		}
	}

	private void UpdateSummonReserveInteractableState(SummonReserveItem p_item)
	{
	}

	public void UpdateSummonsList()
	{
		Player player = PlayerManager.Instance.player;
		if (player == null)
		{
			return;
		}
		foreach (MonsterAndDemonUnderlingCharges value in player.underlingsComponent.monsterUnderlingCharges.Values)
		{
			CreateNewSummonReserveItem(value);
		}
		for (int i = 0; i < player.playerFaction.characters.Count; i++)
		{
			Character character = player.playerFaction.characters[i];
			if (character.partyComponent.hasParty && character.partyComponent.currentParty != PlayerManager.Instance.player.underlingsComponent.persistentDefendParty && !HasSummonPartyItem(character.partyComponent.currentParty))
			{
				CreateSummonPartyItem(character.partyComponent.currentParty);
			}
		}
	}

	public void UpdateActiveSummonCount()
	{
		activeText.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Active") ?? "";
	}

	private void UpdateActiveGameObject()
	{
		bool active = _summonPartyItems.Count > 0;
		goActiveBG.SetActive(active);
		goActive.SetActive(active);
	}

	private SummonPartyItem GetSummonPartyItem(Party p_party)
	{
		for (int i = 0; i < _summonPartyItems.Count; i++)
		{
			SummonPartyItem summonPartyItem = _summonPartyItems[i];
			if (summonPartyItem.party == p_party)
			{
				return summonPartyItem;
			}
		}
		return null;
	}

	private bool HasSummonPartyItem(Party p_party)
	{
		for (int i = 0; i < _summonPartyItems.Count; i++)
		{
			if (_summonPartyItems[i].party == p_party)
			{
				return true;
			}
		}
		return false;
	}

	private void CreateSummonPartyItem(Party p_party)
	{
		SummonPartyItem component = ObjectPoolManager.Instance.InstantiateObjectFromPool(prefabSummonPartyItem.name, Vector3.zero, Quaternion.identity, transformActiveParent).GetComponent<SummonPartyItem>();
		component.Initialize(p_party, _hoverPosition);
		_summonPartyItems.Add(component);
		UpdateActiveGameObject();
		for (int i = 0; i < _summonPartyItems.Count; i++)
		{
			_summonPartyItems[i].UpdatePartyDividerState();
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(transformActiveParent as RectTransform);
		LayoutRebuilder.ForceRebuildLayoutImmediate(rectMainLayoutGroup);
	}

	private void DeleteSummonPartyItem(Party p_party)
	{
		SummonPartyItem summonPartyItem = GetSummonPartyItem(p_party);
		if (summonPartyItem != null)
		{
			ObjectPoolManager.Instance.DestroyObject(summonPartyItem);
			_summonPartyItems.Remove(summonPartyItem);
			for (int i = 0; i < _summonPartyItems.Count; i++)
			{
				_summonPartyItems[i].UpdatePartyDividerState();
			}
			UpdateActiveGameObject();
			LayoutRebuilder.ForceRebuildLayoutImmediate(transformActiveParent as RectTransform);
			LayoutRebuilder.ForceRebuildLayoutImmediate(rectMainLayoutGroup);
		}
	}

	public void ToggleSummonList(bool isOn)
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
}
