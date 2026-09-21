using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UtilityScripts;

public class EditableValuesManager : MonoBehaviour
{
	public static EditableValuesManager Instance;

	[Header("Character Values")]
	[Header("Mood")]
	[SerializeField]
	private int _normalMoodMinThreshold;

	[SerializeField]
	private int _normalMoodHighThreshold;

	[SerializeField]
	private int _lowMoodMinThreshold;

	[SerializeField]
	private int _lowMoodHighThreshold;

	[SerializeField]
	private int _criticalMoodMinThreshold;

	[SerializeField]
	private int _criticalMoodHighThreshold;

	[Tooltip("Number hours a character needs to be in a critical mood to have a 100% chance to trigger a major mental break.")]
	[SerializeField]
	private int _majorMentalBreakHourThreshold;

	[Tooltip("Number days a character needs to be in a low mood to have a 100% chance to trigger a minor mental break.")]
	[SerializeField]
	private int _minorMentalBreakDayThreshold;

	[Header("Needs")]
	[SerializeField]
	private float _baseFullnessDecreaseRate;

	[SerializeField]
	private float _baseTirednessDecreaseRate;

	[SerializeField]
	private float _baseHappinessDecreaseRate;

	[SerializeField]
	private float _outsideSettlementFullnessDecreaseRate;

	[SerializeField]
	private float _outsideSettlementTirednessDecreaseRate;

	[SerializeField]
	private float _outsideSettlementHappinessDecreaseRate;

	[Header("Mana")]
	[SerializeField]
	private int _startingMana;

	[SerializeField]
	private int _maximumMana;

	[SerializeField]
	private int _triggerFlawManaCost;

	[Header("Spirit Energy")]
	[SerializeField]
	private int _startingSpiritEnergy;

	[FormerlySerializedAs("_chaosOrbExpulsionThresholdFromRaid")]
	[Header("Chaos Orb from raid")]
	[SerializeField]
	private int _defaultChaosOrbExpulsionThresholdFromRaid;

	[FormerlySerializedAs("_chaosOrbExpulsionThreshold")]
	[Header("Chaos Orb")]
	[SerializeField]
	private int _defaultChaosOrbExpulsionThreshold;

	[Header("Currency Hover Values")]
	[SerializeField]
	private CurrencyHoverData currencyHoverData;

	[Header("Chaotic Energy")]
	[SerializeField]
	private int _initialChaoticEnergyCustom;

	[Header("Reveal Info Character")]
	[SerializeField]
	private int _revealInfoCharacterCost;

	[Header("Reveal Info Faction")]
	[SerializeField]
	private int _revealInfoFactionCost;

	[Header("Mana regen per hour")]
	[SerializeField]
	private int _manaRegenPerHr;

	[Header("Mana regen gain per manapit")]
	[SerializeField]
	private int _manaRegenPerManapit;

	[Header("Additional Max Mana per pit")]
	[SerializeField]
	private int _additionalMaxManaPerPit;

	[Space]
	[Header("Win target portal level")]
	[SerializeField]
	private int _targetPortalLevel;

	[Space]
	[Header("Beholder Costs")]
	[SerializeField]
	private List<Cost> m_eyeUpgradeCostPerLevel;

	[SerializeField]
	private List<Cost> m_radiusUpgradeCostPerLevel;

	[Space]
	[Header("Elven Power Crystal Bonus")]
	[SerializeField]
	private CharacterProgressionBonusData m_elvenBonusForPowerCrystal;

	[Header("Party Quests")]
	[SerializeField]
	private Cost _cancelPartyQuestCost;

	[FormerlySerializedAs("_removeAllCrimesCost")]
	[Header("Crimes")]
	[SerializeField]
	private Cost _removePerCrimesCost;

	[Space]
	public int vaporStacks;

	public int poisonCloudStacks;

	public int frostyFogStacks;

	public int normalMoodMinThreshold => _normalMoodMinThreshold;

	public int lowMoodMinThreshold => _lowMoodMinThreshold;

	public int lowMoodHighThreshold => _lowMoodHighThreshold;

	public int criticalMoodHighThreshold => _criticalMoodHighThreshold;

	public int majorMentalBreakHourThreshold => _majorMentalBreakHourThreshold;

	public int minorMentalBreakDayThreshold => _minorMentalBreakDayThreshold;

	public float baseFullnessDecreaseRate => _baseFullnessDecreaseRate;

	public float baseTirednessDecreaseRate => _baseTirednessDecreaseRate;

	public float baseHappinessDecreaseRate => _baseHappinessDecreaseRate;

	public float outsideSettlementFullnessDecreaseRate => _outsideSettlementFullnessDecreaseRate;

	public float outsideSettlementTirednessDecreaseRate => _outsideSettlementTirednessDecreaseRate;

	public float outsideSettlementHappinessDecreaseRate => _outsideSettlementHappinessDecreaseRate;

	public int maximumMana
	{
		get
		{
			return _maximumMana;
		}
		set
		{
			_maximumMana = value;
		}
	}

	public int startingMana => _startingMana;

	public int startingSpiritEnergy => _startingSpiritEnergy;

	public int triggerFlawManaCost => _triggerFlawManaCost;

	public int defaultChaosOrbExpulsionThreshold => _defaultChaosOrbExpulsionThreshold;

	public int defaultChaosOrbExpulsionThresholdFromRaid => _defaultChaosOrbExpulsionThresholdFromRaid;

	public Cost cancelPartyQuestCost => _cancelPartyQuestCost;

	public Cost removePerCrimesCost => _removePerCrimesCost;

	private void Awake()
	{
		Instance = this;
	}

	public int GetRevealCharacterInfoCost()
	{
		return SpellUtilities.GetModifiedSpellCost(_revealInfoCharacterCost, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease());
	}

	public int GetRevealFactionInfoCost()
	{
		return SpellUtilities.GetModifiedSpellCost(_revealInfoFactionCost, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease());
	}

	public int GetChaosOrbHoverAmount()
	{
		return GameUtilities.RandomBetweenTwoNumbers(currencyHoverData.minAmountHover, currencyHoverData.maxAmountHover);
	}

	public int GetInitialMaxChaoticEnergy()
	{
		return currencyHoverData.maxChaoticPerValues[0];
	}

	public int GetInitialChaoticEnergyCustom()
	{
		return _initialChaoticEnergyCustom;
	}

	public int GetInitialChaoticEnergyBaseOnGameMode()
	{
		return GetInitialChaoticEnergyCustom();
	}

	public int GetManaRegenPerHour()
	{
		return _manaRegenPerHr;
	}

	public int GetManaRegenPerManaPit()
	{
		return _manaRegenPerManapit;
	}

	public int GetAdditionalMaxManaPerManaPit()
	{
		return _additionalMaxManaPerPit;
	}

	public int GetTargetPortalLevel()
	{
		return _targetPortalLevel;
	}

	public int GetMaxChaoticEnergyPerPortalLevel(int p_portalLevel)
	{
		int p_index = p_portalLevel - 1;
		if (currencyHoverData.maxChaoticPerValues.IsIndexInList(p_index))
		{
			return currencyHoverData.maxChaoticPerValues[p_portalLevel - 1];
		}
		return -1;
	}

	public Cost GetReleaseAbilitiesRerollCost()
	{
		return currencyHoverData.releaseAbilitiesRerollCost;
	}

	public Cost GetCorruptTileCost()
	{
		return currencyHoverData.corruptFloorCost;
	}

	public Cost GetBuildWallCost()
	{
		return currencyHoverData.buildWallCost;
	}

	public Cost GetBeholderEyeUpgradeCostPerLevel(int level)
	{
		return m_eyeUpgradeCostPerLevel[level];
	}

	public Cost GetBeholderRadiusUpgradeCostPerLevel(int level)
	{
		return m_radiusUpgradeCostPerLevel[level];
	}

	public Cost GetTotalRemoveAllCrimesCost(int p_crimeCount)
	{
		int p_amount = removePerCrimesCost.amount * p_crimeCount;
		return new Cost(removePerCrimesCost.currency, p_amount);
	}

	public CharacterProgressionBonusData GetElvenProgressionBonusData()
	{
		return m_elvenBonusForPowerCrystal;
	}
}
