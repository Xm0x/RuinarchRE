using System;
using System.Collections.Generic;
using Inner_Maps;
using Object_Pools;
using UtilityScripts;

namespace Traits;

public class Vampire : Trait
{
	private Character _owner;

	public bool dislikedBeingVampire { get; private set; }

	public int numOfConvertedVillagers { get; private set; }

	public List<Character> awareCharacters { get; private set; }

	public bool isInVampireBatForm { get; private set; }

	public bool isTraversingUnwalkableAsBat { get; private set; }

	public bool hasAlreadyBecomeVampireLord { get; private set; }

	public override Type serializedData => typeof(SaveDataVampire);

	public Vampire()
	{
		name = "Vampire";
		description = "Sustains itself by drinking other's blood.";
		type = TRAIT_TYPE.FLAW;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		canBeTriggered = true;
		advertisedInteractions = new List<INTERACTION_TYPE>
		{
			INTERACTION_TYPE.FEED_SELF,
			INTERACTION_TYPE.DISPEL
		};
		awareCharacters = new List<Character>();
		AddTraitOverrideFunctionIdentifier("See_Poi_Trait");
		AddTraitOverrideFunctionIdentifier("Before_Start_Flee");
		AddTraitOverrideFunctionIdentifier("After_Exiting_Combat");
		AddTraitOverrideFunctionIdentifier("Tick_Started_Trait");
		AddTraitOverrideFunctionIdentifier("Per_Tick_While_Stationary_Unoccupied");
	}

	public override void OnAddTrait(ITraitable sourceCharacter)
	{
		base.OnAddTrait(sourceCharacter);
		if (sourceCharacter is Character character)
		{
			_owner = character;
			character.jobQueue.CancelAllJobs(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT, JOB_TYPE.ENERGY_RECOVERY_NORMAL, JOB_TYPE.ENERGY_RECOVERY_URGENT);
			character.needsComponent.AdjustDoNotGetTired(1);
			character.needsComponent.ResetTirednessMeter();
			DetermineIfDesireOrDislike(character);
			character.behaviourComponent.AddBehaviourComponent(typeof(VampireBehaviour));
		}
	}

	public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy)
	{
		if (sourceCharacter is Character)
		{
			Character character = sourceCharacter as Character;
			character.jobQueue.CancelAllJobs(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT);
			character.needsComponent.AdjustDoNotGetTired(-1);
			character.behaviourComponent.RemoveBehaviourComponent(typeof(VampireBehaviour));
			RevertFromVampireBatForm(character);
			if (character.characterClass.className == "Vampire Lord")
			{
				character.classComponent.AssignClass("Butcher");
			}
			_owner = null;
		}
		base.OnRemoveTrait(sourceCharacter, removedBy);
	}

	public override string TriggerFlaw(Character character, bool isTriggeredByPlayer = true)
	{
		if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW))
		{
			WeightedDictionary<Character> vampiricEmbraceTargetWeights = VampireBehaviour.GetVampiricEmbraceTargetWeights(character);
			if (vampiricEmbraceTargetWeights.GetTotalOfWeights() > 0)
			{
				Character target = vampiricEmbraceTargetWeights.PickRandomElementGivenWeights();
				character.jobComponent.CreateVampiricEmbraceJob(JOB_TYPE.TRIGGER_FLAW, target)?.SetIsTriggeredByPlayer(isTriggeredByPlayer);
				return base.TriggerFlaw(character);
			}
			return "no_victim";
		}
		return "has_trigger_flaw";
	}

	public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob)
	{
		if (targetPOI is Character character)
		{
			if (character.advertisedActions.Contains(INTERACTION_TYPE.DRINK_BLOOD) && characterThatWillDoJob.needsComponent.isStarving && characterThatWillDoJob.limiterComponent.canDoFullnessRecovery)
			{
				if (!characterThatWillDoJob.relationshipContainer.IsFriendsWith(character) && !characterThatWillDoJob.relationshipContainer.IsFamilyMember(character) && !characterThatWillDoJob.relationshipContainer.HasSpecialPositiveRelationshipWith(character))
				{
					characterThatWillDoJob.jobComponent.CreateDrinkBloodJob(JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT, character);
				}
			}
			else if (characterThatWillDoJob.faction != null && characterThatWillDoJob.faction.factionType.type == FACTION_TYPE.Vampire_Clan && character.traitContainer.HasTrait("Hemophiliac") && ChanceData.RollChance(CHANCE_TYPE.Vampire_Recruit_Hemophiliac) && !characterThatWillDoJob.faction.IsCharacterBannedFromJoining(character) && characterThatWillDoJob.faction.ideologyComponent.DoesCharacterFitCurrentIdeologies(character) && !character.crimeComponent.HasWantedCrimeBy(characterThatWillDoJob.faction, CRIME_SEVERITY.Serious, CRIME_SEVERITY.Heinous))
			{
				characterThatWillDoJob.jobComponent.TriggerRecruitJob(character);
			}
		}
		return base.OnSeePOI(targetPOI, characterThatWillDoJob);
	}

	public override void OnBeforeStartFlee(ITraitable traitable)
	{
		base.OnBeforeStartFlee(traitable);
		if (traitable is Character character && !isInVampireBatForm && CanTransformIntoBat() && !character.crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(CRIME_TYPE.Vampire))
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Transform_To_Bat, character);
		}
	}

	public override void OnAfterExitingCombat(ITraitable traitable)
	{
		base.OnAfterExitingCombat(traitable);
		if (traitable is Character character && isInVampireBatForm)
		{
			if (!character.crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(CRIME_TYPE.Vampire))
			{
				character.interruptComponent.TriggerInterrupt(INTERRUPT.Revert_From_Bat, character);
			}
			else
			{
				character.crimeComponent.FleeToAllNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(character, CRIME_TYPE.Vampire);
			}
		}
	}

	public override void OnTickStarted(ITraitable traitable)
	{
		base.OnTickStarted(traitable);
		if (!isInVampireBatForm)
		{
			return;
		}
		if (isTraversingUnwalkableAsBat)
		{
			if (!_owner.marker || _owner.gridTileLocation == null)
			{
				return;
			}
			LocationGridTile destinationTile = _owner.marker.GetDestinationTile();
			if (destinationTile != null && PathfindingManager.Instance.HasPathEvenDiffRegion(_owner.gridTileLocation, destinationTile) && _owner.gridTileLocation.IsPassable())
			{
				if ((bool)_owner.marker && _owner.marker.hasFleePath)
				{
					SetIsTraversingUnwalkableAsBat(state: false);
				}
				else
				{
					_owner.interruptComponent.TriggerInterrupt(INTERRUPT.Revert_From_Bat, _owner);
				}
			}
		}
		else if (_owner.stateComponent.currentState is CombatState { isAttacking: not false })
		{
			_owner.interruptComponent.TriggerInterrupt(INTERRUPT.Revert_From_Bat, _owner);
		}
	}

	public override string GetTestingData(ITraitable traitable = null)
	{
		return string.Concat(base.GetTestingData(traitable) + "Converted Villagers: " + numOfConvertedVillagers, "\nHas Become Vampire Lord: ", hasAlreadyBecomeVampireLord.ToString());
	}

	protected override string GetDescriptionInUI()
	{
		string text = base.GetDescriptionInUI();
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Traits", "Traits_Table", dislikedBeingVampire ? "Loathes_Trait" : "Enjoys_Trait");
		log.AddToFillers(_owner, _owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(null, base.localizedName, LOG_IDENTIFIER.STRING_1);
		text = text + "\n" + log.logText;
		LogPool.Release(log);
		if (awareCharacters.Count > 0)
		{
			text = text + "\n" + LocalizationManager.Instance.GetLocalizedValue("Traits_Table", "Aware") + ": ";
			for (int i = 0; i < awareCharacters.Count; i++)
			{
				Character character = awareCharacters[i];
				text += character.visuals.GetCharacterNameWithIconAndColor();
				if (!CollectionUtilities.IsLastIndex(awareCharacters, i))
				{
					text += ", ";
				}
			}
		}
		return text;
	}

	public override bool PerTickWhileStationaryOrUnoccupied(Character p_character)
	{
		if (dislikedBeingVampire && GameUtilities.RollChance(1) && _owner.currentJob != null && _owner.currentJob.jobType.IsFullnessRecoveryTypeJob())
		{
			_owner.currentJob.ForceCancelJob("Resisted_Hunger");
			_owner.traitContainer.AddTrait(_owner, "Ashamed");
			_owner.traitContainer.AddTrait(_owner, "Abstain Fullness");
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Trait", "Traits_Table", "Vampire resist_hunger", LOG_TAG.Needs);
			log.AddToFillers(_owner, _owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddLogToDatabase(releaseLogAfter: true);
		}
		return false;
	}

	public override void DisconnectFromCharacter(IPointOfInterest p_owner, Character p_character)
	{
		base.DisconnectFromCharacter(p_owner, p_character);
		awareCharacters.Remove(p_character);
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataVampire saveDataVampire = saveDataTrait as SaveDataVampire;
		dislikedBeingVampire = saveDataVampire.dislikedBeingVampire;
		numOfConvertedVillagers = saveDataVampire.numOfConvertedVillagers;
		isInVampireBatForm = saveDataVampire.isInVampireBatForm;
		isTraversingUnwalkableAsBat = saveDataVampire.isTraversingUnwalkableAsBat;
		hasAlreadyBecomeVampireLord = saveDataVampire.hasAlreadyBecomeVampireLord;
	}

	public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
		SaveDataVampire saveDataVampire = p_saveDataTrait as SaveDataVampire;
		awareCharacters.AddRange(SaveUtilities.ConvertIDListToCharacters(saveDataVampire.awareCharacters));
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character owner)
		{
			_owner = owner;
		}
	}

	public bool CanTransformIntoBat()
	{
		if (PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.VAMPIRISM).HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Transform_To_Bat) && !_owner.traitContainer.HasTrait("Polymorphed"))
		{
			return !_owner.mountComponent.IsMounting();
		}
		return false;
	}

	public void SetIsInVampireBatForm(bool state)
	{
		isInVampireBatForm = state;
		SetIsTraversingUnwalkableAsBat(state);
	}

	public void SetIsTraversingUnwalkableAsBat(bool state)
	{
		isTraversingUnwalkableAsBat = state;
	}

	private void DetermineIfDesireOrDislike(Character character)
	{
		if (character.traitContainer.HasTrait("Hemophobic", "Chaste"))
		{
			dislikedBeingVampire = true;
		}
		else if (character.traitContainer.HasTrait("Hemophiliac"))
		{
			dislikedBeingVampire = false;
		}
		else if (character.traitContainer.HasTrait("Demon Cultist") && GameUtilities.RollChance(75))
		{
			dislikedBeingVampire = false;
		}
		else if (character.characterClass.className == "Hero" && GameUtilities.RollChance(75))
		{
			dislikedBeingVampire = true;
		}
		else if (character.characterClass.className == "Shaman" && GameUtilities.RollChance(80))
		{
			dislikedBeingVampire = true;
		}
		else if (character.traitContainer.HasTrait("Lycanthrope") && GameUtilities.RollChance(80))
		{
			dislikedBeingVampire = true;
		}
		else if (character.traitContainer.HasTrait("Evil", "Treacherous") && GameUtilities.RollChance(75))
		{
			dislikedBeingVampire = false;
		}
		else if (GameUtilities.RollChance(50))
		{
			dislikedBeingVampire = false;
		}
		else
		{
			dislikedBeingVampire = true;
		}
	}

	public void AdjustNumOfConvertedVillagers(int amount)
	{
		numOfConvertedVillagers += amount;
	}

	public void AddAwareCharacter(Character character)
	{
		if (!awareCharacters.Contains(character))
		{
			awareCharacters.Add(character);
			if (character.traitContainer.HasTrait("Hemophiliac"))
			{
				character.traitContainer.GetTraitOrStatus<Hemophiliac>("Hemophiliac").OnBecomeAwareOfVampire(_owner);
			}
			else if (character.traitContainer.HasTrait("Hemophobic"))
			{
				character.traitContainer.GetTraitOrStatus<Hemophobic>("Hemophobic").OnBecomeAwareOfVampire(_owner);
			}
		}
	}

	public bool DoesCharacterKnowThisVampire(Character character)
	{
		return awareCharacters.Contains(character);
	}

	public bool DoesFactionKnowThisVampire(Faction faction, bool includeDeadMembersInChecking = true)
	{
		for (int i = 0; i < faction.characters.Count; i++)
		{
			Character character = faction.characters[i];
			if (character != _owner && (includeDeadMembersInChecking || !character.isDead) && DoesCharacterKnowThisVampire(character))
			{
				return true;
			}
		}
		return false;
	}

	private Character GetDrinkBloodTarget(Character vampire)
	{
		List<Character> list = null;
		if (vampire.currentRegion != null)
		{
			for (int i = 0; i < vampire.currentRegion.charactersAtLocation.Count; i++)
			{
				Character character = vampire.currentRegion.charactersAtLocation[i];
				if (vampire != character && !character.traitContainer.HasTrait("Vampire") && character.isNormalCharacter && character.carryComponent.IsNotBeingCarried() && character.Advertises(INTERACTION_TYPE.DRINK_BLOOD) && !character.isDead && !vampire.relationshipContainer.IsFriendsWith(character) && vampire.movementComponent.HasPathToEvenIfDiffRegion(character.gridTileLocation))
				{
					if (list == null)
					{
						list = new List<Character>();
					}
					list.Add(character);
				}
			}
		}
		if (list != null && list.Count > 0)
		{
			return CollectionUtilities.GetRandomElement(list);
		}
		return null;
	}

	public void SetHasBecomeVampireLord(bool state)
	{
		hasAlreadyBecomeVampireLord = state;
	}

	public void RevertFromVampireBatForm(Character p_character)
	{
		SetIsInVampireBatForm(state: false);
		p_character.movementComponent.AdjustSpeedModifier(-0.2f);
		p_character.movementComponent.SetToNonFlying();
		if (p_character.visuals != null)
		{
			p_character.visuals.UpdateAllVisuals(p_character);
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = _owner;
		awareCharacters.Contains(p_character);
	}
}
