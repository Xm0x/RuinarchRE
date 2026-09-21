using System;
using System.Collections.Generic;
using System.Linq;
using Plague.Death_Effect;
using Plague.Fatality;
using Plague.Symptom;
using UnityEngine;
using UtilityScripts;

public class PlagueDisease : ISingletonPattern, ISavable
{
	private static PlagueDisease _Instance;

	private PlagueLifespan _lifespan;

	private List<Fatality> _activeFatalities;

	private List<PlagueSymptom> _activeSymptoms;

	private PlagueDeathEffect _activeDeathEffect;

	private int _activeCases;

	private int _deaths;

	private int _recoveries;

	private Dictionary<PLAGUE_TRANSMISSION, int> _transmissionLevels;

	public OBJECT_TYPE objectType => OBJECT_TYPE.Plague_Disease;

	public Type serializedData => typeof(SaveDataPlagueDisease);

	public string persistentID => "-1";

	public PlagueLifespan lifespan => _lifespan;

	public List<Fatality> activeFatalities => _activeFatalities;

	public List<PlagueSymptom> activeSymptoms => _activeSymptoms;

	public PlagueDeathEffect activeDeathEffect => _activeDeathEffect;

	public int activeMaxTransmissions => 3;

	public int activeMaxFatalities => 2;

	public int activeMaxSymptoms => 5;

	public int activeMaxDeathEffect => 1;

	public int activeCases => _activeCases;

	public int deaths => _deaths;

	public int recoveries => _recoveries;

	public Dictionary<PLAGUE_TRANSMISSION, int> transmissionLevels => _transmissionLevels;

	public static PlagueDisease Instance
	{
		get
		{
			if (_Instance == null)
			{
				_Instance = new PlagueDisease();
			}
			return _Instance;
		}
	}

	public PlagueDisease()
	{
		Initialize();
	}

	public PlagueDisease(SaveDataPlagueDisease p_data)
	{
		_lifespan = p_data.lifespan.Load();
		_activeFatalities = new List<Fatality>();
		if (p_data.activeFatalities != null && p_data.activeFatalities.Count > 0)
		{
			for (int i = 0; i < p_data.activeFatalities.Count; i++)
			{
				Fatality item = CreateNewFatalityInstance(p_data.activeFatalities[i]);
				_activeFatalities.Add(item);
			}
		}
		_activeSymptoms = new List<PlagueSymptom>();
		if (p_data.activeSymptoms != null && p_data.activeSymptoms.Count > 0)
		{
			for (int j = 0; j < p_data.activeSymptoms.Count; j++)
			{
				PlagueSymptom item2 = CreateNewSymptomInstance(p_data.activeSymptoms[j]);
				_activeSymptoms.Add(item2);
			}
		}
		if (p_data.hasDeathEffect)
		{
			PlagueDeathEffect plagueDeathEffect = CreateNewPlagueDeathEffectInstance(p_data.activeDeathEffect);
			_activeDeathEffect = plagueDeathEffect;
			_activeDeathEffect.SetLevel(p_data.activeDeathEffectLevel);
		}
		else
		{
			_activeDeathEffect = null;
		}
		_activeCases = p_data.activeCases;
		_deaths = p_data.deaths;
		_recoveries = p_data.recoveries;
		_transmissionLevels = new Dictionary<PLAGUE_TRANSMISSION, int>(p_data.transmissionLevels);
		_Instance = this;
	}

	public void Initialize()
	{
		AddCleanupListener();
		_lifespan = new PlagueLifespan();
		_activeFatalities = new List<Fatality>();
		_activeSymptoms = new List<PlagueSymptom>();
		_activeDeathEffect = null;
		_transmissionLevels = new Dictionary<PLAGUE_TRANSMISSION, int>
		{
			{
				PLAGUE_TRANSMISSION.Airborne,
				0
			},
			{
				PLAGUE_TRANSMISSION.Consumption,
				1
			},
			{
				PLAGUE_TRANSMISSION.Physical_Contact,
				0
			},
			{
				PLAGUE_TRANSMISSION.Combat,
				0
			}
		};
	}

	public void AddCleanupListener()
	{
		Messenger.AddListener(Signals.CLEAN_UP_MEMORY, CleanUpAndRemoveCleanUpListener);
	}

	public void CleanUpAndRemoveCleanUpListener()
	{
		_activeFatalities.Clear();
		_activeFatalities = null;
		_activeSymptoms.Clear();
		_activeSymptoms = null;
		_activeDeathEffect = null;
		_Instance = null;
		_deaths = 0;
		_activeCases = 0;
		_recoveries = 0;
		Messenger.RemoveListener(Signals.CLEAN_UP_MEMORY, CleanUpAndRemoveCleanUpListener);
	}

	public static bool HasInstance()
	{
		return _Instance != null;
	}

	public void AddAndInitializeFatality(PLAGUE_FATALITY p_fatalityType)
	{
		Fatality fatality = CreateNewFatalityInstance(p_fatalityType);
		_activeFatalities.Add(fatality);
		Messenger.Broadcast(PlayerSignals.ADDED_PLAGUE_DISEASE_FATALITY, fatality);
	}

	public bool IsFatalityActive(PLAGUE_FATALITY p_fatalityType)
	{
		for (int i = 0; i < _activeFatalities.Count; i++)
		{
			if (_activeFatalities[i].fatalityType == p_fatalityType)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasMaxActiveFatalities()
	{
		return _activeFatalities.Count >= activeMaxFatalities;
	}

	private Fatality CreateNewFatalityInstance(PLAGUE_FATALITY fatality)
	{
		Type type = Type.GetType("Plague.Fatality." + fatality.ToStringEnumNoSpace() + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
		if (type != null)
		{
			return Activator.CreateInstance(type) as Fatality;
		}
		throw new Exception($"No fatality class of type {fatality}");
	}

	public void AddAndInitializeSymptom(PLAGUE_SYMPTOM p_symptomType)
	{
		PlagueSymptom plagueSymptom = CreateNewSymptomInstance(p_symptomType);
		_activeSymptoms.Add(plagueSymptom);
		Messenger.Broadcast(PlayerSignals.ADDED_PLAGUE_DISEASE_SYMPTOM, plagueSymptom);
	}

	private PlagueSymptom CreateNewSymptomInstance(PLAGUE_SYMPTOM p_symptomType)
	{
		Type type = Type.GetType("Plague.Symptom." + p_symptomType.ToStringEnumWithNoSpace() + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
		if (type != null)
		{
			return Activator.CreateInstance(type) as PlagueSymptom;
		}
		throw new Exception($"No plague symptom class of type {p_symptomType}");
	}

	public bool IsSymptomActive(PLAGUE_SYMPTOM p_symptomType)
	{
		for (int i = 0; i < _activeSymptoms.Count; i++)
		{
			if (_activeSymptoms[i].symptomType == p_symptomType)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasMaxActiveSymptoms()
	{
		return _activeSymptoms.Count >= activeMaxSymptoms;
	}

	public void UpdateRecoveriesOnPOILostPlagued(IPointOfInterest p_poi)
	{
		if (p_poi is Character character && !character.traitContainer.HasTrait("Plague Reservoir"))
		{
			AdjustRecoveries(1);
		}
	}

	public void UpdateActiveCasesOnPOILostPlagued(IPointOfInterest p_poi)
	{
		if (p_poi is Character character && !character.traitContainer.HasTrait("Plague Reservoir"))
		{
			AdjustActiveCases(-1);
		}
	}

	public void UpdateActiveCasesOnPOIGainedPlagued(IPointOfInterest p_poi)
	{
		if (p_poi is Character character && !character.traitContainer.HasTrait("Plague Reservoir"))
		{
			AdjustActiveCases(1);
		}
	}

	public void UpdateDeathsOnCharacterDied(Character p_character)
	{
		if (!p_character.traitContainer.HasTrait("Plague Reservoir"))
		{
			AdjustDeaths(1);
		}
	}

	public void UpdateActiveCasesOnCharacterDied(Character p_character)
	{
		if (!p_character.traitContainer.HasTrait("Plague Reservoir"))
		{
			AdjustActiveCases(-1);
		}
	}

	private void AdjustDeaths(int p_adjustment)
	{
		_deaths += p_adjustment;
		_deaths = Mathf.Max(0, _deaths);
	}

	private void AdjustRecoveries(int p_adjustment)
	{
		_recoveries += p_adjustment;
		_recoveries = Mathf.Max(0, _recoveries);
	}

	private void AdjustActiveCases(int p_adjustment)
	{
		_activeCases += p_adjustment;
		_activeCases = Mathf.Max(0, _activeCases);
	}

	public int GetTransmissionLevel(PLAGUE_TRANSMISSION p_transmissionType)
	{
		if (_transmissionLevels.ContainsKey(p_transmissionType))
		{
			return _transmissionLevels[p_transmissionType];
		}
		return 0;
	}

	public string GetTransmissionRateDescription(int level)
	{
		return level switch
		{
			1 => LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Low"), 
			2 => LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Medium"), 
			3 => LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "High"), 
			_ => LocalizationManager.Not_Applicable, 
		};
	}

	public bool IsMaxLevel(PLAGUE_TRANSMISSION p_transmissionType)
	{
		return GetTransmissionLevel(p_transmissionType) == 3;
	}

	public int UpgradeTransmissionLevel(PLAGUE_TRANSMISSION p_transmissionType)
	{
		if (_transmissionLevels.ContainsKey(p_transmissionType))
		{
			_transmissionLevels[p_transmissionType]++;
			return _transmissionLevels[p_transmissionType];
		}
		return 0;
	}

	public bool HasMaxActiveTransmissions()
	{
		int num = 0;
		foreach (KeyValuePair<PLAGUE_TRANSMISSION, int> transmissionLevel in _transmissionLevels)
		{
			if (transmissionLevel.Value > 0)
			{
				num++;
			}
		}
		return num >= activeMaxTransmissions;
	}

	public bool IsTransmissionActive(PLAGUE_TRANSMISSION p_transmissionType)
	{
		if (_transmissionLevels.ContainsKey(p_transmissionType))
		{
			return _transmissionLevels[p_transmissionType] > 0;
		}
		return false;
	}

	public void SetNewPlagueDeathEffectAndUnsetPrev(PLAGUE_DEATH_EFFECT p_deathEffectType)
	{
		UnseteDeathEffect();
		PlagueDeathEffect plagueDeathEffect = CreateNewPlagueDeathEffectInstance(p_deathEffectType);
		_activeDeathEffect = plagueDeathEffect;
		Messenger.Broadcast(PlayerSignals.SET_PLAGUE_DEATH_EFFECT, _activeDeathEffect);
	}

	private void UnseteDeathEffect()
	{
		if (_activeDeathEffect != null)
		{
			Messenger.Broadcast(PlayerSignals.UNSET_PLAGUE_DEATH_EFFECT, _activeDeathEffect);
			_activeDeathEffect = null;
		}
	}

	private PlagueDeathEffect CreateNewPlagueDeathEffectInstance(PLAGUE_DEATH_EFFECT p_deathEffectType)
	{
		Type type = Type.GetType("Plague.Death_Effect." + p_deathEffectType.ToStringEnumNoSpace() + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
		if (type != null)
		{
			return Activator.CreateInstance(type) as PlagueDeathEffect;
		}
		throw new Exception($"No plague death effect class of type {p_deathEffectType}");
	}

	public bool HasMaxActiveDeathEffect()
	{
		return _activeDeathEffect != null;
	}

	public bool IsDeathEffectActive(PLAGUE_DEATH_EFFECT p_deathEffect, out PlagueDeathEffect deathEffect)
	{
		deathEffect = _activeDeathEffect;
		return IsDeathEffectActive(p_deathEffect);
	}

	public bool IsDeathEffectActive(PLAGUE_DEATH_EFFECT p_deathEffect)
	{
		if (_activeDeathEffect != null)
		{
			return _activeDeathEffect.deathEffectType == p_deathEffect;
		}
		return false;
	}

	public bool AddPlaguedStatusOnPOIWithLifespanDuration(IPointOfInterest poi)
	{
		if (CanAddPlaguedStatusOnPOIBasedOnLifespan(poi, out var lifespanInTicks))
		{
			return poi.traitContainer.AddTrait(poi, "Plagued", null, bypassElementalChance: false, lifespanInTicks);
		}
		return false;
	}

	public bool CanAddPlaguedStatusOnPOIBasedOnLifespan(IPointOfInterest poi, out int lifespanInTicks)
	{
		lifespanInTicks = lifespan.GetLifespanInTicksOfPlagueOn(poi);
		return lifespanInTicks != -1;
	}

	public void OnLoadoutPicked()
	{
	}

	private void RandomizePlague()
	{
		List<PLAGUE_TRANSMISSION> list = CollectionUtilities.GetEnumValues<PLAGUE_TRANSMISSION>().ToList();
		for (int i = 0; i < 2; i++)
		{
			if (list.Count == 0)
			{
				break;
			}
			PLAGUE_TRANSMISSION randomElement = CollectionUtilities.GetRandomElement(list);
			int value = ((i == 0) ? 1 : 2);
			_transmissionLevels[randomElement] = value;
			list.Remove(randomElement);
		}
		List<string> list2 = new List<string> { "Tile Object", "Monster", "Undead", "Human", "Elf" };
		for (int j = 0; j < 2; j++)
		{
			if (list2.Count == 0)
			{
				break;
			}
			string randomElement2 = CollectionUtilities.GetRandomElement(list2);
			switch (randomElement2)
			{
			case "Tile Object":
				_lifespan.UpgradeTileObjectInfectionTime();
				break;
			case "Monster":
				_lifespan.UpgradeMonsterInfectionTime();
				break;
			case "Undead":
				_lifespan.UpgradeUndeadInfectionTime();
				break;
			case "Human":
				_lifespan.UpgradeSapientInfectionTime(RACE.HUMANS);
				break;
			case "Elf":
				_lifespan.UpgradeSapientInfectionTime(RACE.ELVES);
				break;
			}
			list2.Remove(randomElement2);
		}
		PLAGUE_FATALITY randomElement3 = CollectionUtilities.GetRandomElement(CollectionUtilities.GetEnumValues<PLAGUE_FATALITY>());
		AddAndInitializeFatality(randomElement3);
		List<PLAGUE_SYMPTOM> list3 = CollectionUtilities.GetEnumValues<PLAGUE_SYMPTOM>().ToList();
		for (int k = 0; k < 1; k++)
		{
			if (list3.Count == 0)
			{
				break;
			}
			PLAGUE_SYMPTOM randomElement4 = CollectionUtilities.GetRandomElement(list3);
			AddAndInitializeSymptom(randomElement4);
			list3.Remove(randomElement4);
		}
		PLAGUE_DEATH_EFFECT randomElement5 = CollectionUtilities.GetRandomElement(CollectionUtilities.GetEnumValues<PLAGUE_DEATH_EFFECT>());
		SetNewPlagueDeathEffectAndUnsetPrev(randomElement5);
		_activeDeathEffect.AdjustLevel(1);
	}

	public string GetPlagueEffectsSummary()
	{
		string text = "<b>" + LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Effects") + "</b>";
		int transmissionLevel = GetTransmissionLevel(PLAGUE_TRANSMISSION.Airborne);
		int transmissionLevel2 = GetTransmissionLevel(PLAGUE_TRANSMISSION.Combat);
		int transmissionLevel3 = GetTransmissionLevel(PLAGUE_TRANSMISSION.Consumption);
		int transmissionLevel4 = GetTransmissionLevel(PLAGUE_TRANSMISSION.Physical_Contact);
		if (transmissionLevel > 0)
		{
			text = text + "\n" + LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Airborne Rate") + ": " + GetTransmissionRateDescription(transmissionLevel);
		}
		if (transmissionLevel2 > 0)
		{
			text = text + "\n" + LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Combat Rate") + ": " + GetTransmissionRateDescription(transmissionLevel2);
		}
		if (transmissionLevel3 > 0)
		{
			text = text + "\n" + LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Consumption Rate") + ": " + GetTransmissionRateDescription(transmissionLevel3);
		}
		if (transmissionLevel4 > 0)
		{
			text = text + "\n" + LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Direct Contact Rate") + ": " + GetTransmissionRateDescription(transmissionLevel4);
		}
		text = text + "\n" + LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Object Lifespan") + ": " + lifespan.GetInfectionTimeString(lifespan.tileObjectInfectionTimeInHours);
		text = text + "\n" + LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Human Lifespan") + ": " + lifespan.GetInfectionTimeString(lifespan.GetSapientLifespanOfPlagueInHours(RACE.HUMANS));
		text = text + "\n" + LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Elves Lifespan") + ": " + lifespan.GetInfectionTimeString(lifespan.GetSapientLifespanOfPlagueInHours(RACE.ELVES));
		text = text + "\n" + LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Monster Lifespan") + ": " + lifespan.GetInfectionTimeString(lifespan.monsterInfectionTimeInHours);
		text = text + "\n" + LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Undead Lifespan") + ": " + lifespan.GetInfectionTimeString(lifespan.undeadInfectionTimeInHours);
		text = text + "\n" + LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Fatality") + ": ";
		if (activeFatalities.Count > 0)
		{
			for (int i = 0; i < activeFatalities.Count; i++)
			{
				Fatality fatality = activeFatalities[i];
				text += fatality.fatalityType.LocalizedName();
				if (i + 1 < activeFatalities.Count)
				{
					text += ", ";
				}
			}
		}
		else
		{
			text += "-";
		}
		text = text + "\n" + LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Symptoms") + ": ";
		if (activeSymptoms.Count > 0)
		{
			for (int j = 0; j < activeSymptoms.Count; j++)
			{
				PlagueSymptom plagueSymptom = activeSymptoms[j];
				text += plagueSymptom.symptomType.LocalizedName();
				if (j + 1 < activeSymptoms.Count)
				{
					text += ", ";
				}
			}
		}
		else
		{
			text += "-";
		}
		return text + "\n" + LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "On Death") + ": " + (activeDeathEffect?.GetCurrentEffectDescription() ?? "-");
	}
}
