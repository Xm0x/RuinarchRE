using System.Collections.Generic;

public class IsCannibal : GoapAction
{
	public override bool isTargetSelf => true;

	public IsCannibal()
		: base(INTERACTION_TYPE.IS_CANNIBAL)
	{
		base.actionIconString = GoapActionStateDB.Hostile_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Crimes };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Cannibal Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		bool flag = false;
		if (CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, CRIME_TYPE.Cannibalism).IsConsideredACrime())
		{
			flag = true;
		}
		if (!(!witness.traitContainer.HasTrait("Cannibal") && flag))
		{
			return;
		}
		reactions.Add(EMOTION.Repulsed);
		if (witness.traitContainer.HasTrait("Coward"))
		{
			reactions.Add(EMOTION.Fear);
		}
		else if (!witness.traitContainer.HasTrait("Psychopath"))
		{
			reactions.Add(EMOTION.Threatened);
			switch (witness.relationshipContainer.GetOpinionLabel(actor))
			{
			case "Close Friend":
				reactions.Add(EMOTION.Despair);
				break;
			case "Acquaintance":
			case "Friend":
				reactions.Add(EMOTION.Shock);
				break;
			}
		}
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		return CRIME_TYPE.Cannibalism;
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Cannibalism;
	}
}
