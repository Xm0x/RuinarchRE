using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

namespace Traits;

public class TraitContainer : ITraitContainer
{
	public Dictionary<string, Trait> allTraitsAndStatuses { get; private set; }

	public List<Trait> traits { get; private set; }

	public List<Status> statuses { get; private set; }

	public Dictionary<string, List<Trait>> traitOverrideFunctions { get; private set; }

	public Dictionary<string, int> stacks { get; private set; }

	public Dictionary<string, List<TraitRemoveSchedule>> scheduleTickets { get; private set; }

	public TraitContainer()
	{
		allTraitsAndStatuses = new Dictionary<string, Trait>(20);
		statuses = new List<Status>(20);
		traits = new List<Trait>(20);
		traitOverrideFunctions = new Dictionary<string, List<Trait>>(20);
		stacks = new Dictionary<string, int>(20);
		scheduleTickets = new Dictionary<string, List<TraitRemoveSchedule>>(20);
	}

	public bool AddTrait(ITraitable addTo, Trait trait, Character characterResponsible = null, bool bypassElementalChance = false, int overrideDuration = -1, float piercing = 0f, ELEMENTAL_TYPE elementalType = ELEMENTAL_TYPE.Normal)
	{
		if (!TraitValidator.CanAddTraitGeneric(addTo, trait.name, this))
		{
			return false;
		}
		if (TraitManager.Instance.IsTraitElemental(trait.name))
		{
			return TryAddElementalStatus(addTo, trait, characterResponsible, bypassElementalChance, overrideDuration, piercing, elementalType);
		}
		return TraitAddition(addTo, trait, characterResponsible, overrideDuration);
	}

	public bool AddTrait(ITraitable addTo, string traitName, out Trait trait, Character characterResponsible = null, bool bypassElementalChance = false, int overrideDuration = -1, float piercing = 0f, ELEMENTAL_TYPE elementalType = ELEMENTAL_TYPE.Normal)
	{
		if (!TraitValidator.CanAddTraitGeneric(addTo, traitName, this))
		{
			trait = null;
			return false;
		}
		if (TraitManager.Instance.IsTraitElemental(traitName))
		{
			return TryAddElementalStatus(addTo, traitName, out trait, characterResponsible, bypassElementalChance, overrideDuration, piercing, elementalType);
		}
		return TraitAddition(addTo, traitName, out trait, characterResponsible, overrideDuration);
	}

	public bool AddTrait(ITraitable addTo, string traitName, Character characterResponsible = null, bool bypassElementalChance = false, int overrideDuration = -1, float piercing = 0f, ELEMENTAL_TYPE elementalType = ELEMENTAL_TYPE.Normal)
	{
		if (!TraitValidator.CanAddTraitGeneric(addTo, traitName, this))
		{
			return false;
		}
		if (TraitManager.Instance.IsTraitElemental(traitName))
		{
			return TryAddElementalStatus(addTo, traitName, characterResponsible, bypassElementalChance, overrideDuration, piercing, elementalType);
		}
		return TraitAddition(addTo, traitName, characterResponsible, overrideDuration);
	}

	private bool TryAddElementalStatus(ITraitable addTo, string traitName, Character characterResponsible, bool bypassElementalChance, int overrideDuration, float piercing, ELEMENTAL_TYPE elementalType)
	{
		if (addTo is MovingTileObject || addTo is Quicksand)
		{
			return false;
		}
		bool flag = ProcessBeforeAddingElementalStatus(addTo, traitName, bypassElementalChance, characterResponsible, piercing, elementalType);
		if (flag)
		{
			flag = ProcessBeforeSuccessfullyAddingElementalStatus(addTo, traitName, ref overrideDuration);
			if (flag)
			{
				Trait trait = null;
				flag = TraitAddition(addTo, traitName, out trait, characterResponsible, overrideDuration);
				if (flag)
				{
					ProcessAfterSuccessfulAddingElementalTrait(addTo, trait as Status);
				}
			}
		}
		return flag;
	}

	private bool TryAddElementalStatus(ITraitable addTo, string traitName, out Trait trait, Character characterResponsible, bool bypassElementalChance, int overrideDuration, float piercing, ELEMENTAL_TYPE elementalType)
	{
		trait = null;
		if (addTo is MovingTileObject || addTo is Quicksand)
		{
			return false;
		}
		bool flag = ProcessBeforeAddingElementalStatus(addTo, traitName, bypassElementalChance, characterResponsible, piercing, elementalType);
		if (flag)
		{
			flag = ProcessBeforeSuccessfullyAddingElementalStatus(addTo, traitName, ref overrideDuration);
			if (flag)
			{
				flag = TraitAddition(addTo, traitName, out trait, characterResponsible, overrideDuration);
				if (flag)
				{
					ProcessAfterSuccessfulAddingElementalTrait(addTo, trait as Status);
				}
			}
		}
		return flag;
	}

	private bool TryAddElementalStatus(ITraitable addTo, Trait trait, Character characterResponsible, bool bypassElementalChance, int overrideDuration, float piercing, ELEMENTAL_TYPE elementalType)
	{
		bool flag = ProcessBeforeAddingElementalStatus(addTo, trait.name, bypassElementalChance, characterResponsible, piercing, elementalType);
		if (flag)
		{
			flag = ProcessBeforeSuccessfullyAddingElementalStatus(addTo, trait.name, ref overrideDuration);
			if (flag)
			{
				flag = TraitAddition(addTo, trait, characterResponsible, overrideDuration);
				if (flag)
				{
					ProcessAfterSuccessfulAddingElementalTrait(addTo, trait as Status);
				}
			}
		}
		return flag;
	}

	private bool ProcessBeforeAddingElementalStatus(ITraitable addTo, string traitName, bool bypassElementalChance, Character characterResponsible, float piercing, ELEMENTAL_TYPE elementalType)
	{
		bool flag = true;
		if (addTo is TileObject tileObject && !tileObject.CanBeAffectedByElementalStatus(traitName))
		{
			return false;
		}
		switch (traitName)
		{
		case "Burning":
		{
			if (HasTrait("Freezing"))
			{
				RemoveTrait(addTo, "Freezing");
				flag = false;
			}
			if (HasTrait("Frozen"))
			{
				RemoveTrait(addTo, "Frozen");
				flag = false;
			}
			bool flag2 = false;
			if (HasTrait("Poisoned"))
			{
				int num2 = stacks["Poisoned"];
				Poisoned traitOrStatus2 = GetTraitOrStatus<Poisoned>("Poisoned");
				RemoveStatusAndStacks(addTo, "Poisoned");
				LocationGridTile gridTileLocation2 = addTo.gridTileLocation;
				if (addTo is TileObject tileObject3)
				{
					if (tileObject3.gridTileLocation != null)
					{
						gridTileLocation2 = tileObject3.gridTileLocation;
					}
					else if (tileObject3.isBeingCarriedBy != null)
					{
						gridTileLocation2 = tileObject3.isBeingCarriedBy.gridTileLocation;
					}
				}
				if (gridTileLocation2 != null && addTo is IPointOfInterest target2)
				{
					CombatManager.Instance.PoisonExplosion(target2, gridTileLocation2, num2, characterResponsible, 1, traitOrStatus2.isPlayerSource);
				}
				flag = false;
				flag2 = true;
			}
			if (!flag2 && addTo is IceBlockWall iceBlockWall)
			{
				iceBlockWall.OnHitByFire();
				flag = false;
			}
			break;
		}
		case "Poisoned":
		{
			if (!HasTrait("Burning"))
			{
				break;
			}
			LocationGridTile gridTileLocation = addTo.gridTileLocation;
			int num = 1;
			bool isPlayerSource = false;
			if (HasTrait("Poisoned"))
			{
				num += stacks["Poisoned"];
				isPlayerSource = GetTraitOrStatus<Poisoned>("Poisoned").isPlayerSource;
				RemoveStatusAndStacks(addTo, "Poisoned");
			}
			RemoveTrait(addTo, "Burning");
			if (addTo is Cinder)
			{
				CombatManager.Instance.PoisonExplosion(gridTileLocation.tileObjectComponent.genericTileObject, gridTileLocation, num, characterResponsible, 1, isPlayerSource);
			}
			else
			{
				if (addTo is TileObject tileObject2)
				{
					if (tileObject2.gridTileLocation != null)
					{
						gridTileLocation = tileObject2.gridTileLocation;
					}
					else if (tileObject2.isBeingCarriedBy != null)
					{
						gridTileLocation = tileObject2.isBeingCarriedBy.gridTileLocation;
					}
				}
				if (gridTileLocation != null && addTo is IPointOfInterest target)
				{
					CombatManager.Instance.PoisonExplosion(target, gridTileLocation, num, characterResponsible, 1, isPlayerSource);
				}
			}
			flag = false;
			break;
		}
		case "Overheating":
			if (HasTrait("Wet"))
			{
				RemoveStatusAndStacks(addTo, "Wet");
				flag = false;
			}
			if (HasTrait("Freezing"))
			{
				RemoveTrait(addTo, "Freezing");
				flag = false;
			}
			if (HasTrait("Frozen"))
			{
				RemoveTrait(addTo, "Frozen");
				flag = false;
			}
			break;
		case "Freezing":
			if (addTo is Character && HasTrait("Overheating"))
			{
				RemoveTrait(addTo, "Overheating");
			}
			if (HasTrait("Poisoned"))
			{
				RemoveTrait(addTo, "Poisoned");
				flag = false;
			}
			break;
		case "Zapped":
			if (addTo is Character character && character.characterClass.className == "Barbarian")
			{
				flag = false;
			}
			if (HasTrait("Electric"))
			{
				flag = false;
			}
			else if ((addTo is GenericTileObject || addTo is ThinWall) && !HasTrait("Wet"))
			{
				flag = false;
			}
			if (HasTrait("Frozen"))
			{
				Frozen traitOrStatus = GetTraitOrStatus<Frozen>("Frozen");
				RemoveTrait(addTo, "Frozen");
				if (addTo is IPointOfInterest && !(addTo is GenericTileObject) && addTo.gridTileLocation != null)
				{
					CombatManager.Instance.FrozenExplosion(addTo as IPointOfInterest, addTo.gridTileLocation, 1, traitOrStatus.isPlayerSource);
				}
				flag = false;
			}
			break;
		case "Wet":
			if (addTo is Cinder cinder)
			{
				cinder.OnHitByWater();
				flag = false;
			}
			else
			{
				addTo.traitContainer.RemoveTrait(addTo, "Burning");
			}
			break;
		}
		if (flag)
		{
			int num3 = Random.Range(0, 100);
			int elementalTraitChanceToBeAdded = GetElementalTraitChanceToBeAdded(traitName, addTo, bypassElementalChance, characterResponsible, piercing, elementalType);
			if (num3 < elementalTraitChanceToBeAdded)
			{
				return true;
			}
		}
		return false;
	}

	private bool ProcessBeforeSuccessfullyAddingElementalStatus(ITraitable addTo, string traitName, ref int overrideDuration)
	{
		bool result = true;
		if (traitName == "Freezing" && HasTrait("Frozen"))
		{
			AddTrait(addTo, "Frozen", null, bypassElementalChance: true, -1, 0f, ELEMENTAL_TYPE.Ice);
			result = false;
		}
		return result;
	}

	private void ProcessAfterSuccessfulAddingElementalTrait(ITraitable traitable, Status status)
	{
		if (status.name == "Freezing")
		{
			if (stacks[status.name] >= status.stackLimit)
			{
				bool isPlayerSource = false;
				if (status is IElementalTrait elementalTrait)
				{
					isPlayerSource = elementalTrait.isPlayerSource;
				}
				RemoveStatusAndStacks(traitable, status.name);
				AddTrait(traitable, "Frozen", null, bypassElementalChance: true, -1, 0f, ELEMENTAL_TYPE.Ice);
				GetTraitOrStatus<Frozen>("Frozen").SetIsPlayerSource(isPlayerSource);
			}
		}
		else if (status.name == "Frozen")
		{
			RemoveStatusAndStacks(traitable, "Wet");
		}
	}

	private bool TraitAddition(ITraitable addTo, string traitName, Character characterResponsible, int overrideDuration)
	{
		if (TraitManager.Instance.IsInstancedTrait(traitName))
		{
			return AddTraitRoot(addTo, TraitManager.Instance.CreateNewInstancedTraitClass<Trait>(traitName), characterResponsible, overrideDuration);
		}
		return AddTraitRoot(addTo, TraitManager.Instance.allTraits[traitName], characterResponsible, overrideDuration);
	}

	private bool TraitAddition(ITraitable addTo, string traitName, out Trait trait, Character characterResponsible, int overrideDuration)
	{
		if (TraitManager.Instance.IsInstancedTrait(traitName))
		{
			trait = TraitManager.Instance.CreateNewInstancedTraitClass<Trait>(traitName);
			return AddTraitRoot(addTo, trait, characterResponsible, overrideDuration);
		}
		trait = TraitManager.Instance.allTraits[traitName];
		return AddTraitRoot(addTo, trait, characterResponsible, overrideDuration);
	}

	private bool TraitAddition(ITraitable addTo, Trait trait, Character characterResponsible, int overrideDuration)
	{
		return AddTraitRoot(addTo, trait, characterResponsible, overrideDuration);
	}

	private bool AddTraitRoot(ITraitable addTo, Trait trait, Character characterResponsible, int overrideDuration)
	{
		if (!TraitValidator.CanAddTrait(addTo, trait, this))
		{
			return false;
		}
		if ((trait is Wet || trait is Poisoned) && addTo is GenericTileObject genericTileObject && genericTileObject.gridTileLocation.tileObjectComponent.objHere is Crops { isFarmCrop: not false } crops)
		{
			crops.traitContainer.AddTrait(crops, trait, characterResponsible, bypassElementalChance: false, overrideDuration);
			return false;
		}
		if (trait is Status { name: var name } status)
		{
			if (status.isStacking)
			{
				if (stacks.ContainsKey(name))
				{
					stacks[name]++;
					if (TraitManager.Instance.IsInstancedTrait(name))
					{
						Status traitOrStatus = GetTraitOrStatus<Status>(name);
						addTo.traitProcessor.OnStatusStacked(addTo, traitOrStatus, characterResponsible, overrideDuration);
					}
					else
					{
						addTo.traitProcessor.OnStatusStacked(addTo, status, characterResponsible, overrideDuration);
					}
				}
				else
				{
					stacks.Add(name, 1);
					statuses.Add(status);
					allTraitsAndStatuses.Add(name, status);
					addTo.traitProcessor.OnTraitAdded(addTo, status, characterResponsible, overrideDuration);
				}
			}
			else
			{
				statuses.Add(status);
				allTraitsAndStatuses.Add(name, status);
				addTo.traitProcessor.OnTraitAdded(addTo, status, characterResponsible, overrideDuration);
			}
		}
		else
		{
			traits.Add(trait);
			allTraitsAndStatuses.Add(trait.name, trait);
			addTo.traitProcessor.OnTraitAdded(addTo, trait, characterResponsible, overrideDuration);
		}
		return true;
	}

	public int GetElementalTraitChanceToBeAdded(string traitName, ITraitable addTo, bool bypassElementalChance, Character characterResponsible, float piercing, ELEMENTAL_TYPE elementalType)
	{
		int result = 100;
		if (!bypassElementalChance)
		{
			int p_value = GetElementalTraitBaseChance(traitName, addTo);
			if (addTo is Character character)
			{
				ELEMENTAL_TYPE p_element = elementalType;
				float piercingPower = piercing;
				if (characterResponsible != null)
				{
					p_element = characterResponsible.combatComponent.currentElement.type;
					piercingPower = piercing;
				}
				if (elementalType != ELEMENTAL_TYPE.Normal)
				{
					character.piercingAndResistancesComponent.ModifyValueByResistance(ref p_value, p_element, piercingPower);
				}
				result = p_value;
			}
		}
		else
		{
			result = 100;
		}
		switch (traitName)
		{
		case "Burning":
			if (HasTrait("Wet", "Burnt") || !HasTrait("Flammable"))
			{
				result = 0;
			}
			else if (HasTrait("Poisoned"))
			{
				result = 100;
			}
			break;
		case "Freezing":
			if (HasTrait("Burning", "Frozen Immune"))
			{
				result = 0;
			}
			else if (HasTrait("Wet"))
			{
				result = 100;
			}
			break;
		case "Zapped":
			if (addTo is Character character2 && character2.characterClass.className == "Barbarian")
			{
				result = 0;
			}
			if (HasTrait("Electric"))
			{
				result = 0;
			}
			else if (HasTrait("Wet"))
			{
				result = 100;
			}
			break;
		case "Frozen":
			if (HasTrait("Burning", "Frozen Immune"))
			{
				result = 0;
			}
			break;
		}
		return result;
	}

	private int GetElementalTraitBaseChance(string traitName, ITraitable traitable)
	{
		int result = 100;
		switch (traitName)
		{
		case "Burning":
			result = 15;
			break;
		case "Freezing":
			result = 20;
			break;
		case "Zapped":
			result = 15;
			break;
		case "Poisoned":
			if (traitable is Character)
			{
				result = 25;
				if (HasTrait("Poisoned"))
				{
					result = 15;
				}
			}
			break;
		}
		return result;
	}

	public bool RestrainAndImprison(ITraitable addTo, Character characterResponsible = null, Faction factionThatImprisoned = null, Character characterThatImprisoned = null)
	{
		AddTrait(addTo, "Restrained", characterResponsible);
		AddTrait(addTo, "Prisoner", characterResponsible);
		Prisoner traitOrStatus = GetTraitOrStatus<Prisoner>("Prisoner");
		Restrained traitOrStatus2 = GetTraitOrStatus<Restrained>("Restrained");
		if (traitOrStatus != null)
		{
			traitOrStatus.ClearResponsibleCharacters();
			if (characterResponsible != null)
			{
				traitOrStatus.AddCharacterResponsibleForTrait(characterResponsible);
			}
			traitOrStatus.SetPrisonerOfFaction(factionThatImprisoned);
			traitOrStatus.SetPrisonerOfCharacter(characterThatImprisoned);
		}
		if (traitOrStatus2 != null)
		{
			traitOrStatus2.ClearResponsibleCharacters();
			if (characterResponsible != null)
			{
				traitOrStatus2.AddCharacterResponsibleForTrait(characterResponsible);
			}
		}
		return true;
	}

	public bool RemoveRestrainAndImprison(ITraitable removedFrom, Character removedBy = null)
	{
		Prisoner traitOrStatus = GetTraitOrStatus<Prisoner>("Prisoner");
		bool num = RemoveTrait(removedFrom, "Restrained", removedBy);
		bool flag = RemoveTrait(removedFrom, "Prisoner", removedBy);
		if (removedFrom is Character { faction: not null } character && traitOrStatus != null && traitOrStatus.IsFactionPrisonerOf(character.faction) && character.crimeComponent.IsWantedBy(character.faction))
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, character, "left_faction_normal");
		}
		return num && flag;
	}

	public bool BecomeObsessWith(Character p_sourceCharacter, Character p_targetCharacter)
	{
		p_sourceCharacter.traitContainer.AddTrait(p_sourceCharacter, "Obsessed");
		Obsessed traitOrStatus = p_sourceCharacter.traitContainer.GetTraitOrStatus<Obsessed>("Obsessed");
		if (traitOrStatus != null)
		{
			traitOrStatus.SetTargetCharacter(p_sourceCharacter, p_targetCharacter);
			return true;
		}
		return false;
	}

	public bool RemoveTrait(ITraitable removeFrom, Trait trait, Character removedBy = null, bool bySchedule = false)
	{
		bool flag = false;
		if (trait is Status status)
		{
			flag = RemoveStatus(removeFrom, status, removedBy, bySchedule);
			if (!flag)
			{
			}
		}
		else
		{
			flag = traits.Remove(trait);
			if (flag)
			{
				allTraitsAndStatuses.Remove(trait.name);
				removeFrom.traitProcessor.OnTraitRemoved(removeFrom, trait, removedBy);
				RemoveScheduleTicket(trait.name, bySchedule);
			}
		}
		return flag;
	}

	public bool RemoveTrait(ITraitable removeFrom, string traitName, Character removedBy = null, bool bySchedule = false)
	{
		if (HasTrait(traitName))
		{
			if (removeFrom is Character character)
			{
				PLAYER_SKILL_TYPE afflictionTypeByTraitName = PlayerSkillManager.Instance.GetAfflictionTypeByTraitName(traitName);
				if (afflictionTypeByTraitName != PLAYER_SKILL_TYPE.NONE)
				{
					character.afflictionsSkillsInflictedByPlayer.Remove(afflictionTypeByTraitName);
				}
			}
			Trait traitOrStatus = GetTraitOrStatus<Trait>(traitName);
			return RemoveTrait(removeFrom, traitOrStatus, removedBy, bySchedule);
		}
		return false;
	}

	private bool RemoveStatusAndStacks(ITraitable removeFrom, Status status, Character removedBy = null, bool bySchedule = false)
	{
		int num = 1;
		if (stacks.ContainsKey(status.name))
		{
			num = stacks[status.name];
		}
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if (RemoveStatus(removeFrom, status, removedBy, bySchedule))
			{
				num2++;
			}
		}
		return num2 == num;
	}

	public void RemoveStatusAndStacks(ITraitable removeFrom, string name, Character removedBy = null, bool bySchedule = false)
	{
		Status traitOrStatus = GetTraitOrStatus<Status>(name);
		if (traitOrStatus != null)
		{
			RemoveStatusAndStacks(removeFrom, traitOrStatus, removedBy, bySchedule);
		}
	}

	public bool RemoveStatus(ITraitable removeFrom, Status status, Character removedBy = null, bool bySchedule = false)
	{
		bool flag = true;
		if (!status.isStacking)
		{
			flag = statuses.Remove(status);
			if (flag)
			{
				allTraitsAndStatuses.Remove(status.name);
				removeFrom.traitProcessor.OnTraitRemoved(removeFrom, status, removedBy);
				RemoveScheduleTicket(status.name, bySchedule);
			}
		}
		else if (stacks.ContainsKey(status.name))
		{
			if (stacks[status.name] > 1)
			{
				stacks[status.name]--;
				removeFrom.traitProcessor.OnStatusUnstack(removeFrom, status, removedBy, bySchedule);
				RemoveScheduleTicket(status.name, bySchedule);
				flag = true;
			}
			else
			{
				flag = statuses.Remove(status);
				if (flag)
				{
					allTraitsAndStatuses.Remove(status.name);
					stacks.Remove(status.name);
					removeFrom.traitProcessor.OnTraitRemoved(removeFrom, status, removedBy);
					RemoveScheduleTicket(status.name, bySchedule);
				}
			}
		}
		return flag;
	}

	public bool RemoveTrait(ITraitable removeFrom, int index, Character removedBy = null)
	{
		bool result = true;
		if (index < 0 || index >= allTraitsAndStatuses.Count)
		{
			result = false;
		}
		else
		{
			Trait trait = traits[index];
			traits.RemoveAt(index);
			allTraitsAndStatuses.Remove(trait.name);
			removeFrom.traitProcessor.OnTraitRemoved(removeFrom, trait, removedBy);
			RemoveScheduleTicket(trait.name);
		}
		return result;
	}

	public void RemoveTrait(ITraitable removeFrom, List<Trait> traits)
	{
		for (int i = 0; i < traits.Count; i++)
		{
			RemoveTrait(removeFrom, traits[i]);
		}
	}

	public List<Trait> RemoveAllTraitsAndStatusesByType(ITraitable removeFrom, TRAIT_TYPE traitType)
	{
		List<Trait> list = new List<Trait>();
		for (int i = 0; i < statuses.Count; i++)
		{
			Status status = statuses[i];
			if (status.type == traitType && RemoveStatusAndStacks(removeFrom, status))
			{
				list.Add(status);
				i--;
			}
		}
		for (int j = 0; j < traits.Count; j++)
		{
			Trait trait = traits[j];
			if (trait.type == traitType && RemoveTrait(removeFrom, j))
			{
				list.Add(trait);
				j--;
			}
		}
		return list;
	}

	public void RemoveAllTraitsByType(ITraitable removeFrom, TRAIT_TYPE traitType)
	{
		for (int i = 0; i < traits.Count; i++)
		{
			if (traits[i].type == traitType && RemoveTrait(removeFrom, i))
			{
				i--;
			}
		}
	}

	public void RemoveAllTraitsAndStatusesByName(ITraitable removeFrom, string name)
	{
		for (int i = 0; i < statuses.Count; i++)
		{
			Status status = statuses[i];
			if (status.name == name && RemoveStatusAndStacks(removeFrom, status))
			{
				i--;
			}
		}
		for (int j = 0; j < traits.Count; j++)
		{
			if (traits[j].name == name && RemoveTrait(removeFrom, j))
			{
				j--;
			}
		}
	}

	public bool RemoveTraitOnSchedule(ITraitable removeFrom, Trait trait)
	{
		if (RemoveTrait(removeFrom, trait, null, bySchedule: true))
		{
			if (trait is Status status)
			{
				status.OnRemoveStatusBySchedule(removeFrom);
			}
			return true;
		}
		return false;
	}

	public bool RemoveTraitOnSchedule(ITraitable removeFrom, string traitName)
	{
		if (HasTrait(traitName))
		{
			Trait traitOrStatus = GetTraitOrStatus<Trait>(traitName);
			return RemoveTraitOnSchedule(removeFrom, traitOrStatus);
		}
		return false;
	}

	public void RemoveAllNonPersistentTraitAndStatuses(ITraitable traitable)
	{
		for (int i = 0; i < statuses.Count; i++)
		{
			Status status = statuses[i];
			if (!status.isPersistent && RemoveStatusAndStacks(traitable, status))
			{
				i--;
			}
		}
		for (int j = 0; j < traits.Count; j++)
		{
			if (!traits[j].isPersistent && RemoveTrait(traitable, j))
			{
				j--;
			}
		}
	}

	public void RemoveAllTraitsAndStatuses(ITraitable traitable)
	{
		for (int i = 0; i < statuses.Count; i++)
		{
			if (RemoveStatusAndStacks(traitable, statuses[i]))
			{
				i--;
			}
		}
		for (int j = 0; j < traits.Count; j++)
		{
			if (RemoveTrait(traitable, j))
			{
				j--;
			}
		}
	}

	public void RemoveAllTraits(ITraitable traitable)
	{
		for (int i = 0; i < traits.Count; i++)
		{
			if (RemoveTrait(traitable, i))
			{
				i--;
			}
		}
	}

	public T GetTraitOrStatus<T>(string traitName) where T : Trait
	{
		if (HasTrait(traitName))
		{
			return allTraitsAndStatuses[traitName] as T;
		}
		return null;
	}

	public T GetTraitOrStatus<T>(string traitName1, string traitName2) where T : Trait
	{
		if (HasTrait(traitName1))
		{
			return allTraitsAndStatuses[traitName1] as T;
		}
		if (HasTrait(traitName2))
		{
			return allTraitsAndStatuses[traitName2] as T;
		}
		return null;
	}

	public bool HasTraitOf(TRAIT_TYPE traitType)
	{
		for (int i = 0; i < traits.Count; i++)
		{
			if (traits[i].type == traitType)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasTraitOrStatusOf(TRAIT_EFFECT traitEffect)
	{
		for (int i = 0; i < traits.Count; i++)
		{
			if (traits[i].effect == traitEffect)
			{
				return true;
			}
		}
		for (int j = 0; j < statuses.Count; j++)
		{
			if (statuses[j].effect == traitEffect)
			{
				return true;
			}
		}
		return false;
	}

	public List<Trait> GetAllTraitsOf(TRAIT_TYPE type)
	{
		List<Trait> list = new List<Trait>();
		for (int i = 0; i < list.Count; i++)
		{
			Trait trait = list[i];
			if (trait.type == type)
			{
				list.Add(trait);
			}
		}
		return list;
	}

	public int GetStacks(string traitName)
	{
		if (stacks.ContainsKey(traitName))
		{
			return stacks[traitName];
		}
		return 0;
	}

	public Trait GetRandomNonHiddenTrait()
	{
		Trait result = null;
		List<Trait> list = RuinarchListPool<Trait>.Claim();
		for (int i = 0; i < traits.Count; i++)
		{
			Trait trait = traits[i];
			if (!trait.isHidden)
			{
				list.Add(trait);
			}
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Trait>.Release(list);
		return result;
	}

	public PowerLockerTrait GetPowerLockerTraitWithNoAssignedPower()
	{
		for (int i = 0; i < traits.Count; i++)
		{
			if (traits[i] is PowerLockerTrait { lockedSkill: PLAYER_SKILL_TYPE.NONE } powerLockerTrait)
			{
				return powerLockerTrait;
			}
		}
		return null;
	}

	public void ProcessOnTickStarted(ITraitable owner)
	{
		List<Trait> list = GetTraitOverrideFunctions("Tick_Started_Trait");
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[i].OnTickStarted(owner);
			}
		}
	}

	public void ProcessOnTickEnded(ITraitable owner)
	{
		List<Trait> list = GetTraitOverrideFunctions("Tick_Ended_Trait");
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[i].OnTickEnded(owner);
			}
		}
	}

	public void ProcessOnHourStarted(ITraitable owner)
	{
		List<Trait> list = GetTraitOverrideFunctions("Hour_Started_Trait");
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[i].OnHourStarted(owner);
			}
		}
	}

	public void AddScheduleTicket(string traitName, string ticket, GameDate removeDate)
	{
		TraitRemoveSchedule traitRemoveSchedule = ObjectPoolManager.Instance.CreateNewTraitRemoveSchedule();
		traitRemoveSchedule.removeDate = removeDate;
		traitRemoveSchedule.ticket = ticket;
		if (scheduleTickets.ContainsKey(traitName))
		{
			scheduleTickets[traitName].Add(traitRemoveSchedule);
			return;
		}
		scheduleTickets.Add(traitName, new List<TraitRemoveSchedule> { traitRemoveSchedule });
	}

	public void RemoveScheduleTicket(string traitName, bool bySchedule = false)
	{
		if (scheduleTickets.ContainsKey(traitName) && scheduleTickets[traitName].Count > 0)
		{
			TraitRemoveSchedule traitRemoveSchedule = scheduleTickets[traitName][0];
			if (!bySchedule && traitRemoveSchedule != null)
			{
				SchedulingManager.Instance.RemoveSpecificEntry(traitRemoveSchedule.ticket);
			}
			scheduleTickets[traitName].RemoveAt(0);
			ObjectPoolManager.Instance.ReturnTraitRemoveScheduleToPool(traitRemoveSchedule);
		}
	}

	public bool HasScheduleTicket(string p_traitName)
	{
		if (scheduleTickets.ContainsKey(p_traitName))
		{
			return scheduleTickets[p_traitName].Count > 0;
		}
		return false;
	}

	public void RescheduleLatestTraitRemoval(ITraitable p_traitable, Trait p_trait, GameDate p_newRemoveDate)
	{
		string name = p_trait.name;
		if (scheduleTickets.ContainsKey(name))
		{
			TraitRemoveSchedule traitRemoveSchedule = null;
			if (scheduleTickets[name].Count > 0)
			{
				traitRemoveSchedule = scheduleTickets[name].Last();
			}
			if (traitRemoveSchedule != null)
			{
				SchedulingManager.Instance.RemoveSpecificEntry(traitRemoveSchedule.ticket);
				scheduleTickets[name].RemoveAt(scheduleTickets[name].IndexOf(traitRemoveSchedule));
				ObjectPoolManager.Instance.ReturnTraitRemoveScheduleToPool(traitRemoveSchedule);
			}
			string ticket = SchedulingManager.Instance.AddEntry(p_newRemoveDate, delegate
			{
				p_traitable.traitContainer.RemoveTraitOnSchedule(p_traitable, p_trait);
			}, p_traitable);
			p_traitable.traitContainer.AddScheduleTicket(p_trait.name, ticket, p_newRemoveDate);
			if (p_traitable is Character character)
			{
				character.moodComponent.RescheduleMoodEffect(p_trait, p_newRemoveDate);
			}
		}
	}

	public GameDate GetLatestExpiryDate(string p_traitName)
	{
		if (scheduleTickets.ContainsKey(p_traitName))
		{
			TraitRemoveSchedule traitRemoveSchedule = null;
			if (scheduleTickets[p_traitName].Count > 0)
			{
				traitRemoveSchedule = scheduleTickets[p_traitName].Last();
			}
			if (traitRemoveSchedule != null)
			{
				return traitRemoveSchedule.removeDate;
			}
		}
		return default(GameDate);
	}

	public bool HasTrait(string traitName)
	{
		return allTraitsAndStatuses.ContainsKey(traitName);
	}

	public bool HasTrait(string traitName1, string traitName2)
	{
		if (allTraitsAndStatuses.ContainsKey(traitName1) || allTraitsAndStatuses.ContainsKey(traitName2))
		{
			return true;
		}
		return false;
	}

	public bool HasTrait(string traitName1, string traitName2, string traitName3)
	{
		if (allTraitsAndStatuses.ContainsKey(traitName1) || allTraitsAndStatuses.ContainsKey(traitName2) || allTraitsAndStatuses.ContainsKey(traitName3))
		{
			return true;
		}
		return false;
	}

	public bool HasTrait(string traitName1, string traitName2, string traitName3, string traitName4)
	{
		if (allTraitsAndStatuses.ContainsKey(traitName1) || allTraitsAndStatuses.ContainsKey(traitName2) || allTraitsAndStatuses.ContainsKey(traitName3) || allTraitsAndStatuses.ContainsKey(traitName4))
		{
			return true;
		}
		return false;
	}

	public bool HasTrait(string traitName1, string traitName2, string traitName3, string traitName4, string traitName5)
	{
		if (allTraitsAndStatuses.ContainsKey(traitName1) || allTraitsAndStatuses.ContainsKey(traitName2) || allTraitsAndStatuses.ContainsKey(traitName3) || allTraitsAndStatuses.ContainsKey(traitName4) || allTraitsAndStatuses.ContainsKey(traitName5))
		{
			return true;
		}
		return false;
	}

	public bool HasTrait(string[] traitNames)
	{
		for (int i = 0; i < traitNames.Length; i++)
		{
			if (allTraitsAndStatuses.ContainsKey(traitNames[i]))
			{
				return true;
			}
		}
		return false;
	}

	public void AddTraitOverrideFunction(string identifier, Trait trait)
	{
		if (traitOverrideFunctions.ContainsKey(identifier))
		{
			traitOverrideFunctions[identifier].Add(trait);
			return;
		}
		traitOverrideFunctions.Add(identifier, new List<Trait> { trait });
	}

	public void RemoveTraitOverrideFunction(string identifier, Trait trait)
	{
		if (traitOverrideFunctions.ContainsKey(identifier))
		{
			traitOverrideFunctions[identifier].Remove(trait);
		}
	}

	public List<Trait> GetTraitOverrideFunctions(string identifier)
	{
		if (traitOverrideFunctions.ContainsKey(identifier))
		{
			return traitOverrideFunctions[identifier];
		}
		return null;
	}

	public bool HasTangibleStatus()
	{
		for (int i = 0; i < statuses.Count; i++)
		{
			if (statuses[i].isTangible)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyNotHiddenTrait()
	{
		for (int i = 0; i < traits.Count; i++)
		{
			if (!traits[i].isHidden)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsBlessed()
	{
		if (!HasTrait("Blessed"))
		{
			return HasTrait("Dark Blessing");
		}
		return true;
	}

	public bool IsReligiousCultist()
	{
		if (!HasTrait("Cleric") && !HasTrait("Witch"))
		{
			return HasTrait("Demon Cultist");
		}
		return true;
	}

	public bool IsReligiousCultist(RELIGION p_religion)
	{
		string cultistTraitNameForReligion = p_religion.GetCultistTraitNameForReligion();
		if (!string.IsNullOrEmpty(cultistTraitNameForReligion))
		{
			return HasTrait(cultistTraitNameForReligion);
		}
		return false;
	}

	public bool IsReligiousCultist(out RELIGION p_religion)
	{
		if (HasTrait("Demon Cultist"))
		{
			p_religion = RELIGION.Demon_Worship;
			return true;
		}
		if (HasTrait("Cleric"))
		{
			p_religion = RELIGION.Divine_Worship;
			return true;
		}
		if (HasTrait("Witch"))
		{
			p_religion = RELIGION.Nature_Worship;
			return true;
		}
		p_religion = RELIGION.None;
		return false;
	}

	public void RemoveReligiousCultistTrait(ITraitable traitable)
	{
		if (IsReligiousCultist(out var p_religion))
		{
			RemoveTrait(traitable, p_religion.GetCultistTraitNameForReligion());
		}
	}

	public bool IsResponsibleForTrait(string p_traitName, Character p_character)
	{
		return GetTraitOrStatus<Trait>(p_traitName)?.IsResponsibleForTrait(p_character) ?? false;
	}

	private bool LoadUnInstancedTrait(ITraitable addTo, string traitName)
	{
		Trait trait = TraitManager.Instance.allTraits[traitName];
		if (addTo.traitContainer.HasTrait(trait.name))
		{
			return false;
		}
		return LoadTraitRoot(addTo, trait);
	}

	private bool LoadInstancedTrait(ITraitable addTo, Trait trait)
	{
		return LoadTraitRoot(addTo, trait);
	}

	private bool LoadTraitRoot(ITraitable addTo, Trait trait)
	{
		if (trait is Status item)
		{
			statuses.Add(item);
		}
		else
		{
			traits.Add(trait);
		}
		trait.LoadTraitOnLoadTraitContainer(addTo);
		if (allTraitsAndStatuses.ContainsKey(trait.name))
		{
			Debug.LogError($"Trait {trait.name} already exists in {addTo}'s traits!");
		}
		else
		{
			allTraitsAndStatuses.Add(trait.name, trait);
		}
		if (trait.traitOverrideFunctionIdentifiers != null && trait.traitOverrideFunctionIdentifiers.Count > 0)
		{
			for (int i = 0; i < trait.traitOverrideFunctionIdentifiers.Count; i++)
			{
				string identifier = trait.traitOverrideFunctionIdentifiers[i];
				AddTraitOverrideFunction(identifier, trait);
			}
		}
		return true;
	}

	public void Load(ITraitable owner, SaveDataTraitContainer saveDataTraitContainer)
	{
		for (int i = 0; i < saveDataTraitContainer.nonInstancedTraits.Count; i++)
		{
			string traitName = saveDataTraitContainer.nonInstancedTraits[i];
			LoadUnInstancedTrait(owner, traitName);
		}
		for (int j = 0; j < saveDataTraitContainer.instancedTraitsIDs.Count; j++)
		{
			string id = saveDataTraitContainer.instancedTraitsIDs[j];
			Trait traitByPersistentID = DatabaseManager.Instance.traitDatabase.GetTraitByPersistentID(id);
			LoadInstancedTrait(owner, traitByPersistentID);
		}
		stacks.Clear();
		foreach (KeyValuePair<string, int> stack in saveDataTraitContainer.stacks)
		{
			stacks.Add(stack.Key, stack.Value);
		}
		foreach (KeyValuePair<string, List<GameDate>> ticket in saveDataTraitContainer.scheduleTickets)
		{
			for (int k = 0; k < ticket.Value.Count; k++)
			{
				GameDate gameDate = ticket.Value[k];
				string ticket2 = SchedulingManager.Instance.AddEntry(gameDate, delegate
				{
					RemoveTraitOnSchedule(owner, ticket.Key);
				}, owner);
				AddScheduleTicket(ticket.Key, ticket2, gameDate);
			}
		}
	}

	public void CleanUp()
	{
		allTraitsAndStatuses?.Clear();
		traits?.Clear();
		statuses?.Clear();
		traitOverrideFunctions?.Clear();
		stacks?.Clear();
		scheduleTickets?.Clear();
	}

	public void DisconnectFromCharacter(IPointOfInterest p_owner, Character p_character)
	{
		if (allTraitsAndStatuses.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<string, Trait> item in new Dictionary<string, Trait>(allTraitsAndStatuses))
		{
			item.Value.DisconnectFromCharacter(p_owner, p_character);
		}
	}
}
