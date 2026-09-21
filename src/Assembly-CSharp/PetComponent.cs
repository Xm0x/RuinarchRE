using System.Collections.Generic;
using Characters.Components;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class PetComponent : CharacterComponent, CharacterEventDispatcher.IDeathListener
{
	private OwnedPetsData _ownedPetsData { get; set; }

	private PetOwnerData _petOwnerData { get; set; }

	public List<Character> ownedPets => _ownedPetsData.pets;

	public Wyvern wyvernPet => _ownedPetsData.GetWyvernPet();

	public Character petOwner => _petOwnerData.petOwner;

	public OwnedPetsData ownedPetsData => _ownedPetsData;

	public PetComponent()
	{
		_ownedPetsData = new OwnedPetsData();
		_petOwnerData = new PetOwnerData();
		ownedPetsData.SetBaseMaximumPetCapacity(1);
	}

	public void LoadReferences(SaveDataPetComponent p_data)
	{
		if (p_data.ownedPets != null)
		{
			List<Character> list = SaveUtilities.ConvertIDListToCharacters(p_data.ownedPets);
			Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(p_data.wyvernPet);
			_ownedPetsData.LoadPets(list, characterByPersistentID);
			for (int i = 0; i < list.Count; i++)
			{
				Character p_pet = list[i];
				SubscribeToPetEvents(p_pet);
			}
		}
		if (p_data.petOwner != null)
		{
			_petOwnerData.LoadPetOwner(DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(p_data.petOwner));
		}
		ownedPetsData.SetBaseMaximumPetCapacity(p_data.baseMaxPetCapacity);
	}

	public void TameCharacter(Character p_target, bool p_setRelationship)
	{
		AddPet(p_target, p_setRelationship);
		p_target.CancelAllJobs();
		if (p_target.hasMarker)
		{
			for (int i = 0; i < p_target.marker.inVisionCharacters.Count; i++)
			{
				Character character = p_target.marker.inVisionCharacters[i];
				character.combatComponent.RemoveHostileInRange(p_target);
				character.combatComponent.RemoveAvoidInRange(p_target);
			}
		}
		p_target.combatComponent.ClearHostilesInRange();
		p_target.combatComponent.ClearAvoidInRange();
		p_target.behaviourComponent.RemoveBehaviourComponent(typeof(AttackVillageBehaviour));
		Messenger.Broadcast(JobSignals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, (IPointOfInterest)p_target);
		Messenger.Broadcast(CharacterSignals.ON_CHARACTER_TAMED, p_target);
	}

	public void AddPet(Character p_pet, bool p_setRelationship)
	{
		if (!_ownedPetsData.AddPet(p_pet))
		{
			return;
		}
		if (p_setRelationship)
		{
			RelationshipManager.Instance.CreateNewRelationshipBetween(base.owner, p_pet, RELATIONSHIP_TYPE.PET);
		}
		p_pet.petComponent.SetPetOwner(base.owner);
		if (p_pet.faction != base.owner.faction)
		{
			p_pet.ChangeFactionTo(base.owner.faction, bypassIdeologyChecking: true);
		}
		if (p_pet.homeStructure != base.owner.homeStructure)
		{
			p_pet.MigrateHomeStructureTo(base.owner.homeStructure);
		}
		if (p_pet.territory != base.owner.territory)
		{
			if (base.owner.territory == null)
			{
				p_pet.ClearTerritory();
			}
			else
			{
				p_pet.SetTerritory(base.owner.territory);
			}
		}
		SubscribeToPetEvents(p_pet);
	}

	public void RemovePet(Character p_pet)
	{
		if (_ownedPetsData.RemovePet(p_pet))
		{
			p_pet.petComponent.SetPetOwner(null);
			UnsubscribeToPetEvents(p_pet);
		}
	}

	public bool HasPetOfType(SUMMON_TYPE p_monsterType)
	{
		if (_ownedPetsData.pets != null)
		{
			for (int i = 0; i < _ownedPetsData.pets.Count; i++)
			{
				if (_ownedPetsData.pets[i] is Summon summon && summon.summonType == p_monsterType)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void SetPetOwner(Character p_petOwner)
	{
		_petOwnerData.SetPetOwner(p_petOwner);
		base.owner.behaviourComponent.UpdateDefaultBehaviourSet();
	}

	public bool HasPetOwner()
	{
		return _petOwnerData.petOwner != null;
	}

	private void SubscribeToPetEvents(Character p_pet)
	{
		p_pet.eventDispatcher.SubscribeToCharacterDied(this);
	}

	private void UnsubscribeToPetEvents(Character p_pet)
	{
		p_pet.eventDispatcher.UnsubscribeToCharacterDied(this);
	}

	public void OnComponentOwnerDied()
	{
		if (ownedPets == null)
		{
			return;
		}
		List<Character> list = RuinarchListPool<Character>.Claim();
		list.AddRange(ownedPets);
		for (int i = 0; i < list.Count; i++)
		{
			Character character = list[i];
			RemovePet(character);
			bool flag = true;
			if (base.owner.faction != null)
			{
				if (character.race == RACE.WYVERN && base.owner.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Wyvern_Tamers))
				{
					flag = false;
				}
				else if (character.raceSetting.category == CHARACTER_CATEGORY.Beast && base.owner.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Breeders))
				{
					flag = false;
				}
			}
			if (flag)
			{
				character.ChangeToDefaultFaction();
			}
			if (character.traitContainer.HasTrait("Temporal"))
			{
				character.Death();
			}
		}
		RuinarchListPool<Character>.Release(list);
	}

	public void OnComponentOwnerChangedFaction(Faction p_faction)
	{
		if (ownedPets == null)
		{
			return;
		}
		for (int i = 0; i < ownedPets.Count; i++)
		{
			Character character = ownedPets[i];
			if (p_faction == null)
			{
				base.owner.faction?.LeaveFaction(character);
			}
			else
			{
				character.ChangeFactionTo(p_faction, bypassIdeologyChecking: true);
			}
		}
	}

	public void OnComponentOwnerChangedHomeStructure(LocationStructure p_homeStructure)
	{
		if (ownedPets != null)
		{
			for (int i = 0; i < ownedPets.Count; i++)
			{
				ownedPets[i].MigrateHomeStructureTo(p_homeStructure);
			}
		}
	}

	public bool HasWyvernPet()
	{
		return _ownedPetsData.HasWyvernPet();
	}

	public void OnCharacterSubscribedToDied(Character p_character)
	{
		if (ownedPets.Contains(p_character))
		{
			RemovePet(p_character);
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		if (ownedPets != null)
		{
			ownedPets.Contains(p_character);
		}
		_ = wyvernPet;
		_ = petOwner;
	}
}
