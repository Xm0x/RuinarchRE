using System.Collections.Generic;
using Traits;

public class Mummify : GoapAction
{
	public Mummify()
		: base(INTERACTION_TYPE.MUMMIFY)
	{
		base.actionIconString = GoapActionStateDB.Magic_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Mummify Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		Character character = target as Character;
		reactions.Add(EMOTION.Repulsed);
		if (character != null && witness.relationshipContainer.IsFriendsWith(character))
		{
			reactions.Add(EMOTION.Resentment);
		}
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job) && poiTarget is Character character)
		{
			if (actor != poiTarget && character.hasMarker)
			{
				return character.gridTileLocation != null;
			}
			return false;
		}
		return false;
	}

	public void AfterMummifySuccess(ActualGoapNode goapNode)
	{
		Character character = goapNode.poiTarget as Character;
		character.traitContainer.AddTrait(character, "Mummified");
		if (goapNode.actor.homeStructure != null)
		{
			Obsessed traitOrStatus = goapNode.actor.traitContainer.GetTraitOrStatus<Obsessed>("Obsessed");
			if (traitOrStatus != null && traitOrStatus.targetCharacter == character)
			{
				goapNode.actor.jobComponent.TryTriggerMoveCharacter(character, goapNode.actor.homeStructure);
			}
		}
		character.ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.BURY);
		character.ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.BURY_IN_ACTIVE_PARTY);
		if (character.homeSettlement != null)
		{
			character.homeSettlement.ForceCancelJobTypesTargetingPOI(JOB_TYPE.BURY, character);
			character.homeSettlement.ForceCancelJobTypesTargetingPOI(JOB_TYPE.BURY_IN_ACTIVE_PARTY, character);
		}
		if (character.previousCharacterDataComponent.previousHomeSettlement != null)
		{
			character.previousCharacterDataComponent.previousHomeSettlement.ForceCancelJobTypesTargetingPOI(JOB_TYPE.BURY, character);
			character.previousCharacterDataComponent.previousHomeSettlement.ForceCancelJobTypesTargetingPOI(JOB_TYPE.BURY_IN_ACTIVE_PARTY, character);
		}
		if (goapNode.actor.homeSettlement != null)
		{
			goapNode.actor.homeSettlement.ForceCancelJobTypesTargetingPOI(JOB_TYPE.BURY, character);
			goapNode.actor.homeSettlement.ForceCancelJobTypesTargetingPOI(JOB_TYPE.BURY_IN_ACTIVE_PARTY, character);
		}
	}
}
