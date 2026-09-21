using System.Collections.Generic;
using UtilityScripts;

public class IsCultist : GoapAction
{
	public override bool isTargetSelf => true;

	public IsCultist()
		: base(INTERACTION_TYPE.IS_CULTIST)
	{
		base.actionIconString = GoapActionStateDB.Hostile_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Crimes };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Cultist Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		bool flag = false;
		if (CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, CRIME_TYPE.Demon_Worship).IsConsideredACrime())
		{
			flag = true;
		}
		if (!witness.traitContainer.IsReligiousCultist(RELIGION.Demon_Worship) && flag)
		{
			reactions.Add(EMOTION.Repulsed);
			if (witness.traitContainer.HasTrait("Coward"))
			{
				reactions.Add(EMOTION.Fear);
			}
			else
			{
				if (witness.traitContainer.HasTrait("Psychopath"))
				{
					return;
				}
				if (witness.classComponent.IsTargetRecognizedAsCultistByStalker(actor))
				{
					reactions.Add(EMOTION.Disapproval);
					return;
				}
				reactions.Add(EMOTION.Threatened);
				string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
				if (opinionLabel == "Close Friend")
				{
					reactions.Add(EMOTION.Despair);
				}
				else if (opinionLabel == "Friend")
				{
					reactions.Add(EMOTION.Shock);
				}
			}
		}
		else
		{
			reactions.Add(EMOTION.Approval);
			if (RelationshipManager.IsSexuallyCompatibleOneSided(witness, actor) && GameUtilities.RollChance(10 * witness.relationshipContainer.GetCompatibility(actor)))
			{
				reactions.Add(EMOTION.Arousal);
			}
		}
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		return CRIME_TYPE.Demon_Worship;
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Demon_Worship;
	}
}
