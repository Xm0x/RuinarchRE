using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class Pickpocket : GoapAction
{
	public Pickpocket()
		: base(INTERACTION_TYPE.PICKPOCKET)
	{
		base.actionIconString = GoapActionStateDB.Steal_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Crimes };
		base.doesNotStopTargetCharacter = true;
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_POI, GOAP_EFFECT_TARGET.ACTOR));
		AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, GOAP_EFFECT_TARGET.ACTOR));
	}

	protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, OtherData[] otherData, out bool isOverridden)
	{
		List<GoapEffect> list = RuinarchListPool<GoapEffect>.Claim(4);
		AddBaseExpectedEffectsToList(list);
		if (actor.traitContainer.HasTrait("Kleptomaniac"))
		{
			list.Add(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
		}
		isOverridden = true;
		return list;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Pickpocket Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		if (actor.traitContainer.HasTrait("Enslaved") && (target.gridTileLocation == null || !target.gridTileLocation.IsInHomeOf(actor)))
		{
			return 2000;
		}
		int num = Utilities.Rng.Next(300, 351);
		if (actor.traitContainer.HasTrait("Kleptomaniac"))
		{
			num = Utilities.Rng.Next(90, 151);
		}
		else if (target is Character target2)
		{
			string opinionLabel = actor.relationshipContainer.GetOpinionLabel(target2);
			if (actor.moodComponent.moodState == MOOD_STATE.Normal || opinionLabel == "Acquaintance" || opinionLabel == "Friend" || opinionLabel == "Close Friend")
			{
				num += 2000;
			}
			else if (actor.moodComponent.moodState == MOOD_STATE.Bad)
			{
				num += Utilities.Rng.Next(500, 601);
			}
			else if (actor.moodComponent.moodState == MOOD_STATE.Critical)
			{
				num += Utilities.Rng.Next(120, 201);
			}
		}
		return num;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		string stateName = "Target Missing";
		GoapActionInvalidity invalidity = node.invalidity;
		invalidity.isInvalid = false;
		invalidity.stateName = stateName;
		invalidity.reason = string.Empty;
		return invalidity;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (!witness.traitContainer.HasTrait("Demon Cultist"))
		{
			reactions.Add(EMOTION.Disapproval);
			if (witness.relationshipContainer.IsFriendsWith(actor))
			{
				reactions.Add(EMOTION.Disappointment);
				reactions.Add(EMOTION.Shock);
			}
		}
		else if (witness == target || (target is TileObject tileObject && tileObject.IsOwnedBy(witness)))
		{
			reactions.Add(EMOTION.Betrayal);
		}
	}

	public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
		if (target is Character character)
		{
			reactions.Add(EMOTION.Disappointment);
			if (character.traitContainer.HasTrait("Hothead") || Random.Range(0, 100) < 35)
			{
				reactions.Add(EMOTION.Anger);
			}
		}
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		return CRIME_TYPE.Theft;
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Theft;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			Character character = poiTarget as Character;
			if (actor != character)
			{
				if (otherData != null && otherData.Length == 1 && otherData[0].obj is TileObject item)
				{
					return character.HasItemOrEquipment(item);
				}
				return character.items.Count > 0;
			}
		}
		return false;
	}

	public void AfterPickpocketSuccess(ActualGoapNode goapNode)
	{
		OtherData[] otherData = goapNode.otherData;
		TileObject tileObject = null;
		tileObject = ((otherData == null || otherData.Length != 1 || !(otherData[0].obj is TileObject tileObject2)) ? (goapNode.poiTarget as Character).GetRandomItem() : tileObject2);
		if (tileObject != null)
		{
			goapNode.actor.PickUpItem(tileObject);
		}
		if (goapNode.actor.traitContainer.HasTrait("Kleptomaniac"))
		{
			goapNode.actor.needsComponent.AdjustHappiness(10f);
		}
	}
}
