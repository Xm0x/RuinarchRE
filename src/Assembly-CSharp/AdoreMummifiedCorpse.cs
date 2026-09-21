using System.Collections.Generic;

public class AdoreMummifiedCorpse : GoapAction
{
	public AdoreMummifiedCorpse()
		: base(INTERACTION_TYPE.ADORE_MUMMIFIED_CORPSE)
	{
		base.actionIconString = GoapActionStateDB.Happy_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Adore Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		Character character = target as Character;
		reactions.Add(EMOTION.Disgust);
		if (witness.relationshipContainer.IsFriendsOrAcquaintancesWith(character))
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

	public void AfterAdoreSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.needsComponent.AdjustHappiness(10f);
	}
}
