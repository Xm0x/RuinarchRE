using Locations.Settlements;

public class GodDayRitual : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.VERBAL;

	public GodDayRitual()
		: base(INTERACTION_TYPE.GOD_DAY_RITUAL)
	{
		base.actionIconString = GoapActionStateDB.Magic_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Ritual Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, target, otherData, job))
		{
			return target.gridTileLocation != null;
		}
		return false;
	}

	public void AfterRitualSuccess(ActualGoapNode goapNode)
	{
		BaseSettlement homeSettlement = goapNode.actor.homeSettlement;
		if (homeSettlement != null)
		{
			for (int i = 0; i < homeSettlement.residents.Count; i++)
			{
				Character character = homeSettlement.residents[i];
				character.traitContainer.RemoveTrait(character, "Favored");
				character.traitContainer.AddTrait(character, "Favored");
			}
		}
	}
}
