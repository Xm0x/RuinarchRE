using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UtilityScripts;

public class EquipmentItem : TileObject
{
	public List<RESISTANCE> resistanceBonuses = new List<RESISTANCE>();

	public EQUIPMENT_QUALITY quality;

	public EquipmentData equipmentData;

	public List<EQUIPMENT_BONUS> addedBonus = new List<EQUIPMENT_BONUS>();

	public EQUIPMENT_SLAYER_BONUS addedSlayerBonus;

	public EQUIPMENT_WARD_BONUS addedWardBonus;

	private string _expiryKey;

	public EQUIPMENT_PREFIX prefix;

	public ELEMENTAL_TYPE addedElementalBonus;

	public int addedCritRate;

	public float strPercentageReduced;

	public float intPercentageReduced;

	public bool isDeadly;

	public bool isFestering;

	public bool isHaunted;

	public bool isMentor;

	public string prefixName = string.Empty;

	public GameDate expiryDate { get; private set; }

	public string wholeName
	{
		get
		{
			if (LocalizationSettings.SelectedLocale.Identifier.Code == "es" || LocalizationSettings.SelectedLocale.Identifier.Code == "es-ES" || LocalizationSettings.SelectedLocale.Identifier.Code == "th")
			{
				return base.name + " " + prefixName;
			}
			return prefixName + " " + base.name;
		}
	}

	public override string nameplateName => wholeName;

	public override Type serializedData => typeof(SaveDataEquipmentItem);

	public EquipmentItem()
	{
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
		AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
		AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
		AddAdvertisedAction(INTERACTION_TYPE.CRAFT_EQUIPMENT);
		AddAdvertisedAction(INTERACTION_TYPE.BUY_ITEM);
		AddAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
		AddAdvertisedAction(INTERACTION_TYPE.DEMON_STEAL);
	}

	public EquipmentItem(SaveDataEquipmentItem data)
		: base(data)
	{
	}

	public void AssignData()
	{
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		if (equipmentData.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Random_Ward_Bonus) && addedWardBonus == EQUIPMENT_WARD_BONUS.None)
		{
			addedWardBonus = (EQUIPMENT_WARD_BONUS)GameUtilities.RandomBetweenTwoNumbers(1, 5);
			addedBonus.Add(EQUIPMENT_BONUS.Ward_Bonus);
		}
		if (equipmentData.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Random_Slayer_Bonus) && addedSlayerBonus == EQUIPMENT_SLAYER_BONUS.None)
		{
			addedSlayerBonus = (EQUIPMENT_SLAYER_BONUS)GameUtilities.RandomBetweenTwoNumbers(1, 5);
			addedBonus.Add(EQUIPMENT_BONUS.Slayer_Bonus);
		}
		if (addedSlayerBonus != EQUIPMENT_SLAYER_BONUS.None && equipmentData.equipmentUpgradeData.slayerBonus == EQUIPMENT_SLAYER_BONUS.None)
		{
			equipmentData.equipmentUpgradeData.slayerBonus = addedSlayerBonus;
		}
		if (addedWardBonus != EQUIPMENT_WARD_BONUS.None && equipmentData.equipmentUpgradeData.wardBonus == EQUIPMENT_WARD_BONUS.None)
		{
			equipmentData.equipmentUpgradeData.wardBonus = addedWardBonus;
		}
	}

	public override void LoadSecondWave(SaveDataTileObject data)
	{
		base.LoadSecondWave(data);
		if (data is SaveDataEquipmentItem saveDataEquipmentItem)
		{
			saveDataEquipmentItem.resistanceBonuses.ForEach(delegate(RESISTANCE eachResistance)
			{
				resistanceBonuses.Add(eachResistance);
			});
			addedSlayerBonus = saveDataEquipmentItem.randomSlayerBonus;
			addedWardBonus = saveDataEquipmentItem.randomWardBonus;
			saveDataEquipmentItem.addedBonus.ForEach(delegate(EQUIPMENT_BONUS eachBonus)
			{
				addedBonus.Add(eachBonus);
			});
			addedElementalBonus = saveDataEquipmentItem.addedElementalBonus;
			addedCritRate = saveDataEquipmentItem.addedCritRate;
			strPercentageReduced = saveDataEquipmentItem.strPercentageReduced;
			intPercentageReduced = saveDataEquipmentItem.intPercentageReduced;
			isDeadly = saveDataEquipmentItem.isDeadly;
			isFestering = saveDataEquipmentItem.isFestering;
			isHaunted = saveDataEquipmentItem.isHaunted;
			isMentor = saveDataEquipmentItem.isMentor;
			prefixName = saveDataEquipmentItem.prefixName;
			prefix = saveDataEquipmentItem.prefix;
			if (saveDataEquipmentItem.expiryDate.hasValue)
			{
				expiryDate = saveDataEquipmentItem.expiryDate;
				LoadExpiry();
			}
		}
	}

	public override void OnLocaleChanged(Locale locale)
	{
		base.OnLocaleChanged(locale);
		if (prefix != EQUIPMENT_PREFIX.None)
		{
			prefixName = LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", prefix.ToStringEnum());
		}
	}

	public override void Initialize(SaveDataTileObject data)
	{
		base.Initialize(data);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
	}

	public void MakeQualityHigh()
	{
		quality = EQUIPMENT_QUALITY.High;
		base.traitContainer.AddTrait(this, "High Quality");
		base.maxHP += (int)((float)base.maxHP * 0.5f);
		base.currentHP = (int)Mathf.Clamp((float)base.currentHP + (float)base.maxHP * 0.5f, 0f, base.maxHP);
	}

	public void MakeQualityPremium()
	{
		quality = EQUIPMENT_QUALITY.Premium;
		base.maxHP += (int)((float)base.maxHP * 2f);
		base.traitContainer.AddTrait(this, "Premium");
		base.currentHP = (int)Mathf.Clamp((float)base.currentHP + (float)base.maxHP * 2f, 0f, base.maxHP);
	}

	public virtual void ProcessEffectsOnEquip(Character p_character)
	{
	}

	public virtual void ProcessEffectsOnUnEquip(Character p_character)
	{
	}

	public string GetBonusDescription()
	{
		string bonusDescription = equipmentData.equipmentUpgradeData.GetBonusDescription(quality, addedElementalBonus);
		bonusDescription += equipmentData.equipmentUpgradeData.GetDescriptionForRandomResistance(resistanceBonuses, quality);
		if (prefix != EQUIPMENT_PREFIX.None)
		{
			bonusDescription += LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", prefix.ToStringEnum());
		}
		return bonusDescription;
	}

	public bool TryAddRandomPrefix()
	{
		if (base.tileObjectType.IsStaff())
		{
			AddRandomElementalElementPrefix();
			return true;
		}
		if (base.tileObjectType.IsBow())
		{
			AddRandomSecondaryElementPrefix();
			return true;
		}
		if (GameUtilities.RollChance(ChanceData.GetChance(CHANCE_TYPE.Equipment_Prefix_Chance)))
		{
			AddRandomPrefixBonus();
			return true;
		}
		return false;
	}

	public void ForceElementalPrefix(int p_bonus)
	{
		ApplyElementalBonus(p_bonus);
	}

	public void AddRandomElementalElementPrefix()
	{
		int p_bonus = UnityEngine.Random.Range(0, 3);
		ApplyElementalBonus(p_bonus);
	}

	public void AddRandomSecondaryElementPrefix()
	{
		int p_bonus = UnityEngine.Random.Range(3, 6);
		ApplyElementalBonus(p_bonus);
	}

	private void ApplyElementalBonus(int p_bonus)
	{
		switch (p_bonus)
		{
		case 0:
			prefix = EQUIPMENT_PREFIX.Burning;
			prefixName = LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", prefix.ToStringEnum());
			addedBonus.Add(EQUIPMENT_BONUS.Attack_Element);
			addedElementalBonus = ELEMENTAL_TYPE.Fire;
			break;
		case 1:
			prefix = EQUIPMENT_PREFIX.Moist;
			prefixName = LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", prefix.ToStringEnum());
			addedBonus.Add(EQUIPMENT_BONUS.Attack_Element);
			addedElementalBonus = ELEMENTAL_TYPE.Water;
			break;
		case 2:
			prefix = EQUIPMENT_PREFIX.Breezy;
			prefixName = LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", prefix.ToStringEnum());
			addedBonus.Add(EQUIPMENT_BONUS.Attack_Element);
			addedElementalBonus = ELEMENTAL_TYPE.Wind;
			break;
		case 3:
			prefix = EQUIPMENT_PREFIX.Icy;
			prefixName = LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", prefix.ToStringEnum());
			addedBonus.Add(EQUIPMENT_BONUS.Attack_Element);
			addedElementalBonus = ELEMENTAL_TYPE.Ice;
			break;
		case 4:
			prefix = EQUIPMENT_PREFIX.Venomous;
			prefixName = LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", prefix.ToStringEnum());
			addedBonus.Add(EQUIPMENT_BONUS.Attack_Element);
			addedElementalBonus = ELEMENTAL_TYPE.Poison;
			break;
		case 5:
			prefix = EQUIPMENT_PREFIX.Static;
			prefixName = LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", prefix.ToStringEnum());
			addedBonus.Add(EQUIPMENT_BONUS.Attack_Element);
			addedElementalBonus = ELEMENTAL_TYPE.Electric;
			break;
		}
	}

	public void ForcePrefixBonus(EQUIPMENT_PREFIX p_prefix)
	{
		prefix = p_prefix;
		prefixName = LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", prefix.ToStringEnum());
		ApplyPrefixEffect(prefix);
	}

	private void AddRandomPrefixBonus()
	{
		prefix = (EQUIPMENT_PREFIX)GameUtilities.RandomBetweenTwoNumbers(0, Enum.GetValues(typeof(EQUIPMENT_PREFIX)).Length - 1);
		prefixName = LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", prefix.ToStringEnum());
		ApplyPrefixEffect(prefix);
	}

	private void ApplyPrefixEffect(EQUIPMENT_PREFIX p_prefix)
	{
		switch (p_prefix)
		{
		case EQUIPMENT_PREFIX.Weightless:
			addedBonus.Add(EQUIPMENT_BONUS.Flight);
			break;
		case EQUIPMENT_PREFIX.Burning:
			addedBonus.Add(EQUIPMENT_BONUS.Attack_Element);
			addedElementalBonus = ELEMENTAL_TYPE.Fire;
			break;
		case EQUIPMENT_PREFIX.Icy:
			addedBonus.Add(EQUIPMENT_BONUS.Attack_Element);
			addedElementalBonus = ELEMENTAL_TYPE.Ice;
			break;
		case EQUIPMENT_PREFIX.Venomous:
			addedBonus.Add(EQUIPMENT_BONUS.Attack_Element);
			addedElementalBonus = ELEMENTAL_TYPE.Poison;
			break;
		case EQUIPMENT_PREFIX.Moist:
			addedBonus.Add(EQUIPMENT_BONUS.Attack_Element);
			addedElementalBonus = ELEMENTAL_TYPE.Water;
			break;
		case EQUIPMENT_PREFIX.Static:
			addedBonus.Add(EQUIPMENT_BONUS.Attack_Element);
			addedElementalBonus = ELEMENTAL_TYPE.Electric;
			break;
		case EQUIPMENT_PREFIX.Breezy:
			addedBonus.Add(EQUIPMENT_BONUS.Attack_Element);
			addedElementalBonus = ELEMENTAL_TYPE.Wind;
			break;
		case EQUIPMENT_PREFIX.Sharp:
			addedBonus.Add(EQUIPMENT_BONUS.Crit_Rate_Actual);
			addedCritRate = 10;
			break;
		case EQUIPMENT_PREFIX.Dull:
			addedBonus.Add(EQUIPMENT_BONUS.Str_Percentage);
			addedBonus.Add(EQUIPMENT_BONUS.Int_Percentage);
			strPercentageReduced = -25f;
			intPercentageReduced = -25f;
			break;
		case EQUIPMENT_PREFIX.Deadly:
			isDeadly = true;
			break;
		case EQUIPMENT_PREFIX.Festering:
			isFestering = true;
			break;
		case EQUIPMENT_PREFIX.Haunted:
			isHaunted = true;
			break;
		case EQUIPMENT_PREFIX.Mentor:
			isMentor = true;
			break;
		}
	}

	public override void GeneralReactionToTileObject(Character actor, ref string debugLog)
	{
		base.GeneralReactionToTileObject(actor, ref debugLog);
		if (actor.equipmentComponent.EvaluateNewEquipment(this, actor))
		{
			if (actor.isNormalCharacter && actor.race != RACE.RATMAN)
			{
				if (base.currentStructure.structureType != STRUCTURE_TYPE.WORKSHOP)
				{
					bool flag;
					JOB_TYPE jOB_TYPE;
					if (actor.traitContainer.HasTrait("Kleptomaniac"))
					{
						flag = true;
						jOB_TYPE = ((base.characterOwner == actor || base.characterOwner == null) ? JOB_TYPE.TAKE_ITEM_ON_SIGHT : JOB_TYPE.KLEPTOMANIAC_STEAL);
					}
					else
					{
						flag = base.characterOwner == null || base.characterOwner == actor;
						jOB_TYPE = JOB_TYPE.TAKE_ITEM_ON_SIGHT;
					}
					if (flag && !actor.jobQueue.HasJob(jOB_TYPE))
					{
						if (jOB_TYPE == JOB_TYPE.KLEPTOMANIAC_STEAL)
						{
							actor.jobComponent.CreateStealItemJob(jOB_TYPE, this);
						}
						else
						{
							actor.jobComponent.CreateTakeItemOnSightJob(this, jOB_TYPE);
						}
						return;
					}
				}
			}
			else
			{
				bool flag2 = base.currentStructure.structureType != STRUCTURE_TYPE.WORKSHOP || base.currentStructure.settlementLocation.owner == null || actor.faction.IsHostileWith(base.currentStructure.settlementLocation.owner);
				if (flag2 && !actor.jobQueue.HasJob(JOB_TYPE.TAKE_ITEM_ON_SIGHT) && !actor.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.TAKE_ITEM_ON_SIGHT))
				{
					actor.jobComponent.CreateTakeItemOnSightJob(this);
					return;
				}
			}
		}
		if (actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive && actor.partyComponent.currentParty.partyState == PARTY_STATE.Working && base.mapObjectState == MAP_OBJECT_STATE.BUILT && actor.partyComponent.currentParty.currentQuest is RaidPartyQuest raidPartyQuest && gridTileLocation != null && gridTileLocation.IsPartOfSettlement(raidPartyQuest.targetSettlement))
		{
			if (!actor.equipmentComponent.IsCurrentEquipmentBetterThanOrEqualTo(this) && actor.equipmentComponent.CanEquipItem(this, actor))
			{
				actor.jobComponent.CreateTakeItemOnSightJob(this);
				raidPartyQuest.SetIsSuccessful(state: true);
			}
			else if (GameUtilities.RollChance(35) && actor.jobComponent.TriggerStealRaidJob(this))
			{
				raidPartyQuest.SetIsSuccessful(state: true);
			}
		}
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		ScheduleExpiry();
	}

	public override void OnLoadPlacePOI()
	{
		DefaultProcessOnPlacePOI();
	}

	private void ScheduleExpiry()
	{
		if (gridTileLocation != null)
		{
			if (!gridTileLocation.structure.structureType.IsVillageStructure())
			{
				CancelExpiry();
				expiryDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(48));
				_expiryKey = SchedulingManager.Instance.AddEntry(expiryDate, TryExpire, this);
			}
			else
			{
				CancelExpiry();
			}
		}
	}

	private void LoadExpiry()
	{
		if (gridTileLocation != null && expiryDate.hasValue)
		{
			_expiryKey = SchedulingManager.Instance.AddEntry(expiryDate, TryExpire, this);
		}
	}

	private void CancelExpiry()
	{
		if (!string.IsNullOrEmpty(_expiryKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_expiryKey);
			_expiryKey = string.Empty;
			expiryDate = default(GameDate);
		}
	}

	private void TryExpire()
	{
		if (!base.isBeingSeized)
		{
			Expire();
		}
		else
		{
			ScheduleExpiry();
		}
	}

	private void Expire()
	{
		if (base.isBeingCarriedBy != null)
		{
			base.isBeingCarriedBy.DropItem(this);
		}
		if (gridTileLocation != null)
		{
			gridTileLocation.structure.RemovePOI(this);
		}
	}

	public void OnEquipmentPickedUp(Character p_character)
	{
		CancelExpiry();
	}
}
