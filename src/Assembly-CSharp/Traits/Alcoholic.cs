using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

namespace Traits;

public class Alcoholic : Trait
{
	private bool _hasDrankWithinTheDay;

	private Character owner;

	public bool hasDrankWithinTheDay => _hasDrankWithinTheDay;

	public override Type serializedData => typeof(SaveDataAlcoholic);

	public Alcoholic()
	{
		name = "Alcoholic";
		description = "More than just a social drinker.";
		type = TRAIT_TYPE.FLAW;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = 0;
		canBeTriggered = true;
		_hasDrankWithinTheDay = true;
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataAlcoholic saveDataAlcoholic = saveDataTrait as SaveDataAlcoholic;
		_hasDrankWithinTheDay = saveDataAlcoholic.hasDrankWithinTheDay;
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character)
		{
			owner = addedTo as Character;
		}
		Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
		Messenger.AddListener<ActualGoapNode>(JobSignals.STARTED_PERFORMING_ACTION, OnPerformAction);
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character character)
		{
			owner = character;
			if (!character.isDead)
			{
				Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
				Messenger.AddListener<ActualGoapNode>(JobSignals.STARTED_PERFORMING_ACTION, OnPerformAction);
			}
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		Messenger.RemoveListener(Signals.DAY_STARTED, OnDayStarted);
		Messenger.RemoveListener<ActualGoapNode>(JobSignals.STARTED_PERFORMING_ACTION, OnPerformAction);
		owner = null;
		base.OnRemoveTrait(removedFrom, removedBy);
	}

	public override string TriggerFlaw(Character character, bool isTriggeredByPlayer = true)
	{
		if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW))
		{
			bool flag = false;
			Heartbroken traitOrStatus = character.traitContainer.GetTraitOrStatus<Heartbroken>("Heartbroken");
			if (traitOrStatus != null)
			{
				flag = UnityEngine.Random.Range(0, 100) < 25 * owner.traitContainer.stacks[traitOrStatus.name];
			}
			if (!flag)
			{
				if (character.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY))
				{
					character.jobQueue.CancelAllJobs(JOB_TYPE.HAPPINESS_RECOVERY);
				}
				if (character.homeSettlement == null || !character.homeSettlement.HasStructure(STRUCTURE_TYPE.TAVERN))
				{
					return "no_target";
				}
				LocationStructure firstStructureOfType = character.homeSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.TAVERN);
				List<TileObject> list = RuinarchListPool<TileObject>.Claim(5);
				firstStructureOfType.PopulateTileObjectsThatAdvertise(list, INTERACTION_TYPE.DRINK);
				TileObject tileObject = null;
				if (list.Count > 0)
				{
					tileObject = CollectionUtilities.GetRandomElement(list);
				}
				RuinarchListPool<TileObject>.Release(list);
				if (tileObject == null)
				{
					return "no_target";
				}
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.DRINK, tileObject, character);
				goapPlanJob.SetIsTriggeredByPlayer(isTriggeredByPlayer);
				character.jobQueue.AddJobInQueue(goapPlanJob);
			}
			else
			{
				traitOrStatus.TriggerBrokenhearted();
			}
			return base.TriggerFlaw(character);
		}
		return "has_trigger_flaw";
	}

	public override void ExecuteCostModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, ref int cost)
	{
		base.ExecuteCostModification(action, actor, poiTarget, otherData, ref cost);
		if (action == INTERACTION_TYPE.DRINK)
		{
			cost = Utilities.Rng.Next(5, 20);
		}
	}

	private void OnDayStarted()
	{
		if (!hasDrankWithinTheDay)
		{
			owner.traitContainer.AddTrait(owner, "Withdrawal");
		}
		_hasDrankWithinTheDay = false;
	}

	private void OnPerformAction(ActualGoapNode node)
	{
		if (node.action.goapType == INTERACTION_TYPE.DRINK && !hasDrankWithinTheDay)
		{
			_hasDrankWithinTheDay = true;
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
	}
}
