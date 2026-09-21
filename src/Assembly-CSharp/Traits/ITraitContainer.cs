using System.Collections.Generic;

namespace Traits;

public interface ITraitContainer
{
	Dictionary<string, Trait> allTraitsAndStatuses { get; }

	List<Status> statuses { get; }

	List<Trait> traits { get; }

	Dictionary<string, List<Trait>> traitOverrideFunctions { get; }

	Dictionary<string, List<TraitRemoveSchedule>> scheduleTickets { get; }

	Dictionary<string, int> stacks { get; }

	bool AddTrait(ITraitable addTo, string traitName, Character characterResponsible = null, bool bypassElementalChance = false, int overrideDuration = -1, float piercing = 0f, ELEMENTAL_TYPE elementalType = ELEMENTAL_TYPE.Normal);

	bool AddTrait(ITraitable addTo, Trait trait, Character characterResponsible = null, bool bypassElementalChance = false, int overrideDuration = -1, float piercing = 0f, ELEMENTAL_TYPE elementalType = ELEMENTAL_TYPE.Normal);

	bool AddTrait(ITraitable addTo, string traitName, out Trait trait, Character characterResponsible = null, bool bypassElementalChance = false, int overrideDuration = -1, float piercing = 0f, ELEMENTAL_TYPE elementalType = ELEMENTAL_TYPE.Normal);

	void AddTraitOverrideFunction(string identifier, Trait trait);

	bool RestrainAndImprison(ITraitable addTo, Character characterResponsible = null, Faction factionThatImprisoned = null, Character characterThatImprisoned = null);

	bool BecomeObsessWith(Character p_sourceCharacter, Character p_targetCharacter);

	bool RemoveTrait(ITraitable removeFrom, Trait trait, Character removedBy = null, bool bySchedule = false);

	void RemoveStatusAndStacks(ITraitable removeFrom, string name, Character removedBy = null, bool bySchedule = false);

	bool RemoveTrait(ITraitable removeFrom, string traitName, Character removedBy = null, bool bySchedule = false);

	bool RemoveTrait(ITraitable removeFrom, int index, Character removedBy = null);

	void RemoveTrait(ITraitable removeFrom, List<Trait> traits);

	List<Trait> RemoveAllTraitsAndStatusesByType(ITraitable removeFrom, TRAIT_TYPE traitType);

	void RemoveAllTraitsAndStatusesByName(ITraitable removeFrom, string name);

	bool RemoveTraitOnSchedule(ITraitable removeFrom, Trait trait);

	void RemoveAllNonPersistentTraitAndStatuses(ITraitable traitable);

	void RemoveAllTraitsAndStatuses(ITraitable traitable);

	void RemoveAllTraits(ITraitable traitable);

	void RemoveAllTraitsByType(ITraitable traitable, TRAIT_TYPE traitType);

	void RemoveTraitOverrideFunction(string identifier, Trait trait);

	bool RemoveRestrainAndImprison(ITraitable removedFrom, Character removedBy = null);

	T GetTraitOrStatus<T>(string traitName) where T : Trait;

	T GetTraitOrStatus<T>(string traitName1, string traitName2) where T : Trait;

	int GetElementalTraitChanceToBeAdded(string traitName, ITraitable addTo, bool bypassElementalChance, Character characterResponsible, float piercing, ELEMENTAL_TYPE elementalType);

	List<Trait> GetAllTraitsOf(TRAIT_TYPE type);

	List<Trait> GetTraitOverrideFunctions(string identifier);

	int GetStacks(string traitName);

	Trait GetRandomNonHiddenTrait();

	PowerLockerTrait GetPowerLockerTraitWithNoAssignedPower();

	void ProcessOnTickStarted(ITraitable owner);

	void ProcessOnTickEnded(ITraitable owner);

	void ProcessOnHourStarted(ITraitable owner);

	void AddScheduleTicket(string traitName, string ticket, GameDate removeDate);

	void RemoveScheduleTicket(string traitName, bool bySchedule);

	void RescheduleLatestTraitRemoval(ITraitable p_traitable, Trait p_trait, GameDate p_newRemoveDate);

	bool HasScheduleTicket(string p_traitName);

	GameDate GetLatestExpiryDate(string p_traitName);

	bool HasTangibleStatus();

	bool HasAnyNotHiddenTrait();

	bool HasTrait(string traitName);

	bool HasTrait(string traitName1, string traitName2);

	bool HasTrait(string traitName1, string traitName2, string traitName3);

	bool HasTrait(string traitName1, string traitName2, string traitName3, string traitName4);

	bool HasTrait(string traitName1, string traitName2, string traitName3, string traitName4, string traitName5);

	bool HasTrait(string[] traitNames);

	bool HasTraitOf(TRAIT_TYPE traitType);

	bool HasTraitOrStatusOf(TRAIT_EFFECT traitEffect);

	bool IsBlessed();

	bool IsReligiousCultist();

	bool IsReligiousCultist(RELIGION p_religion);

	bool IsReligiousCultist(out RELIGION p_religion);

	void RemoveReligiousCultistTrait(ITraitable traitable);

	bool IsResponsibleForTrait(string p_traitName, Character p_character);

	void Load(ITraitable owner, SaveDataTraitContainer saveDataTraitContainer);

	void CleanUp();

	void DisconnectFromCharacter(IPointOfInterest p_owner, Character pCharacter);
}
