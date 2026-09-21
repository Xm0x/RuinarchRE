using System;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

namespace Traits;

public class Pyrophobic : Trait
{
	private Character owner;

	public List<BurningSource> seenBurningSources { get; }

	public override Type serializedData => typeof(SaveDataPyrophobic);

	public Pyrophobic()
	{
		name = "Pyrophobic";
		description = "Will almost always flee when it sees a Fire.";
		type = TRAIT_TYPE.FLAW;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		mutuallyExclusive = new string[1] { "Pyromaniac" };
		seenBurningSources = new List<BurningSource>();
		canBeTriggered = false;
		AddTraitOverrideFunctionIdentifier("See_Poi_Trait");
	}

	public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
		SaveDataPyrophobic saveDataPyrophobic = p_saveDataTrait as SaveDataPyrophobic;
		for (int i = 0; i < saveDataPyrophobic.seenBurningSources.Count; i++)
		{
			string id = saveDataPyrophobic.seenBurningSources[i];
			BurningSource orCreateBurningSourceWithID = DatabaseManager.Instance.burningSourceDatabase.GetOrCreateBurningSourceWithID(id);
			seenBurningSources.Add(orCreateBurningSourceWithID);
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			owner = character;
			if (character.traitContainer.HasTrait("Burning"))
			{
				character.traitContainer.GetTraitOrStatus<Burning>("Burning").CharacterBurningProcess(character);
			}
			Messenger.AddListener<BurningSource>(InnerMapSignals.BURNING_SOURCE_INACTIVE, OnBurningSourceInactive);
		}
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character character)
		{
			owner = character;
			Messenger.AddListener<BurningSource>(InnerMapSignals.BURNING_SOURCE_INACTIVE, OnBurningSourceInactive);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character)
		{
			Messenger.RemoveListener<BurningSource>(InnerMapSignals.BURNING_SOURCE_INACTIVE, OnBurningSourceInactive);
			owner = null;
		}
	}

	public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob)
	{
		Burning traitOrStatus = targetPOI.traitContainer.GetTraitOrStatus<Burning>("Burning");
		if (traitOrStatus != null)
		{
			AddKnownBurningSource(traitOrStatus.sourceOfBurning, targetPOI);
		}
		return base.OnSeePOI(targetPOI, characterThatWillDoJob);
	}

	public bool AddKnownBurningSource(BurningSource burningSource, IPointOfInterest burningPOI)
	{
		if (burningSource == null)
		{
			Debug.LogWarning(owner.name + " saw the fire of " + burningPOI?.nameWithID + " but it has no burning source!");
			return false;
		}
		bool flag = false;
		if (!seenBurningSources.Contains(burningSource))
		{
			seenBurningSources.Add(burningSource);
			flag = true;
		}
		bool flag2 = true;
		bool flag3 = false;
		bool flag4 = false;
		if (owner.HasAfflictedByPlayerWith(name))
		{
			AfflictData afflictionData = PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.PYROPHOBIA);
			flag2 = afflictionData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Make_Anxious);
			flag3 = afflictionData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Pass_Out_From_Fright);
			flag4 = afflictionData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.May_Suffer_Heart_Attack);
		}
		if (flag4 && GameUtilities.RollChance(10) && owner.interruptComponent.TriggerInterrupt(INTERRUPT.Heart_Attack, owner, "", null, "Saw_Fire"))
		{
			return flag;
		}
		if (flag2)
		{
			owner.traitContainer.AddTrait(owner, "Anxious");
			owner.traitContainer.GetTraitOrStatus<Anxious>("Anxious").AddSourceOfAnxiety(burningPOI);
		}
		if (flag3 && GameUtilities.RollChance(20) && owner.interruptComponent.TriggerInterrupt(INTERRUPT.Pass_Out, owner, "", null, "Saw_Fire"))
		{
			return flag;
		}
		if (flag)
		{
			TriggerReactionToFireOnFirstTimeSeeing(burningPOI);
		}
		else
		{
			owner.combatComponent.Flight(burningPOI);
		}
		return flag;
	}

	private void RemoveKnownBurningSource(BurningSource burningSource)
	{
		seenBurningSources.Remove(burningSource);
	}

	private void TriggerReactionToFireOnFirstTimeSeeing(IPointOfInterest burningPOI)
	{
		owner.needsComponent.AdjustHappiness(-20f);
		if (GameUtilities.RollChance(10))
		{
			owner.traitContainer.AddTrait(owner, "Catatonic");
		}
		else if (GameUtilities.RollChance(15))
		{
			owner.traitContainer.AddTrait(owner, "Berserked");
		}
		else if (GameUtilities.RollChance(15))
		{
			owner.interruptComponent.TriggerInterrupt(INTERRUPT.Seizure, owner);
		}
		else if (GameUtilities.RollChance(10) && (owner.characterClass.className == "Druid" || owner.characterClass.className == "Shaman" || owner.characterClass.className == "Mage"))
		{
			owner.interruptComponent.TriggerInterrupt(INTERRUPT.Loss_Of_Control, owner);
		}
		else
		{
			owner.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, owner, "", null, "Saw_Fire");
		}
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Trait", "Traits_Table", "Pyrophobic on_see_first", LOG_TAG.Combat);
		log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddLogToDatabase(releaseLogAfter: true);
	}

	private void OnBurningSourceInactive(BurningSource burningSource)
	{
		RemoveKnownBurningSource(burningSource);
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
	}
}
