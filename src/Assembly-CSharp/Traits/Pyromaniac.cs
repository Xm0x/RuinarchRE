using System;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

namespace Traits;

public class Pyromaniac : Trait
{
	public List<BurningSource> seenBurningSources { get; }

	public override Type serializedData => typeof(SaveDataPyromaniac);

	public Pyromaniac()
	{
		name = "Pyromaniac";
		description = "Can't wait to set things on fire!";
		type = TRAIT_TYPE.FLAW;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		canBeTriggered = true;
		mutuallyExclusive = new string[1] { "Pyrophobic" };
		seenBurningSources = new List<BurningSource>();
		AddTraitOverrideFunctionIdentifier("See_Poi_Trait");
	}

	public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
		SaveDataPyromaniac saveDataPyromaniac = p_saveDataTrait as SaveDataPyromaniac;
		for (int i = 0; i < saveDataPyromaniac.seenBurningSources.Count; i++)
		{
			string id = saveDataPyromaniac.seenBurningSources[i];
			BurningSource orCreateBurningSourceWithID = DatabaseManager.Instance.burningSourceDatabase.GetOrCreateBurningSourceWithID(id);
			seenBurningSources.Add(orCreateBurningSourceWithID);
		}
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character)
		{
			Messenger.AddListener<BurningSource>(InnerMapSignals.BURNING_SOURCE_INACTIVE, OnBurningSourceInactive);
		}
	}

	public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob)
	{
		Burning traitOrStatus = targetPOI.traitContainer.GetTraitOrStatus<Burning>("Burning");
		if (traitOrStatus != null)
		{
			AddKnownBurningSource(traitOrStatus.sourceOfBurning, targetPOI, characterThatWillDoJob);
		}
		return base.OnSeePOI(targetPOI, characterThatWillDoJob);
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			if (character.traitContainer.HasTrait("Burning"))
			{
				character.traitContainer.GetTraitOrStatus<Burning>("Burning").CharacterBurningProcess(character);
			}
			Messenger.AddListener<BurningSource>(InnerMapSignals.BURNING_SOURCE_INACTIVE, OnBurningSourceInactive);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character)
		{
			Messenger.RemoveListener<BurningSource>(InnerMapSignals.BURNING_SOURCE_INACTIVE, OnBurningSourceInactive);
		}
	}

	public override string TriggerFlaw(Character character, bool isTriggeredByPlayer = true)
	{
		if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW))
		{
			List<TileObject> list = RuinarchListPool<TileObject>.Claim();
			if (character.currentStructure != null && (character.currentStructure.structureType.IsVillageStructure() || character.currentStructure.structureType.IsInterior()))
			{
				character.currentStructure.PopulateTileObjectsWithTraitThatActorCanReach(list, "Flammable", character);
			}
			if (list.Count <= 0 && character.currentSettlement != null)
			{
				character.currentSettlement.PopulateTileObjectsWithTraitThatActorCanReach("Flammable", list, character);
			}
			if (list.Count <= 0)
			{
				character.areaLocation.tileObjectComponent.PopulateTileObjectsWithTraitThatActorCanReach("Flammable", list, character);
			}
			TileObject randomElement = CollectionUtilities.GetRandomElement(list);
			RuinarchListPool<TileObject>.Release(list);
			if (randomElement == null)
			{
				return "no_target";
			}
			character.jobComponent.TriggerArson(randomElement, JOB_TYPE.TRIGGER_FLAW)?.SetIsTriggeredByPlayer(isTriggeredByPlayer);
		}
		return base.TriggerFlaw(character);
	}

	private bool AddKnownBurningSource(BurningSource burningSource, IPointOfInterest burningPOI, Character owner)
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
		if (flag)
		{
			owner.needsComponent.AdjustHappiness(20f);
		}
		return flag;
	}

	private void RemoveKnownBurningSource(BurningSource burningSource)
	{
		seenBurningSources.Remove(burningSource);
	}

	private void OnBurningSourceInactive(BurningSource burningSource)
	{
		RemoveKnownBurningSource(burningSource);
	}
}
