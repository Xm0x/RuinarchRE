public class AbsorbWisp : GoapAction
{
	public AbsorbWisp()
		: base(INTERACTION_TYPE.ABSORB_WISP)
	{
		base.actionIconString = GoapActionStateDB.Magic_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Absorb Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (actor != poiTarget && (bool)poiTarget.mapObjectVisual)
			{
				return poiTarget is Wisp;
			}
			return false;
		}
		return false;
	}

	public void PreAbsorbSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.piercingAndResistancesComponent.AdjustBasePiercing(5f);
		RESISTANCE randomResistance = CharacterManager.Instance.GetRandomResistance(shouldIncludeNone: false);
		goapNode.actor.piercingAndResistancesComponent.AdjustResistance(randomResistance, 10f);
		string value = randomResistance.LocalizedName();
		goapNode.descriptionLog.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
	}

	public void AfterAbsorbSuccess(ActualGoapNode goapNode)
	{
		Character character = goapNode.poiTarget as Character;
		if (character.hasMarker)
		{
			character.SetDestroyMarkerOnDeath(state: true);
		}
		if (!character.isDead)
		{
			character.Death("absorbed", goapNode, goapNode.actor, null, null, null, null, isPlayerSource: false, goapNode.actor);
		}
	}
}
