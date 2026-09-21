using System.Collections.Generic;
using Traits;
using UtilityScripts;

public class IsVampire : GoapAction
{
	public override bool isTargetSelf => true;

	public IsVampire()
		: base(INTERACTION_TYPE.IS_VAMPIRE)
	{
		base.actionIconString = GoapActionStateDB.Drink_Blood_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Crimes };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Vampire Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		string result = base.ReactionToActor(actor, target, witness, node, status);
		actor.traitContainer.GetTraitOrStatus<Vampire>("Vampire")?.AddAwareCharacter(witness);
		return result;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, CRIME_TYPE.Vampire).IsConsideredACrime())
		{
			if (witness.characterClass.className == "Stalker")
			{
				reactions.Add(EMOTION.Disapproval);
				return;
			}
			if (witness.traitContainer.IsReligiousCultist(RELIGION.Demon_Worship) && actor.traitContainer.IsReligiousCultist(RELIGION.Demon_Worship))
			{
				reactions.Add(EMOTION.Approval);
				if (RelationshipManager.IsSexuallyCompatibleOneSided(witness, actor) && GameUtilities.RollChance(10 * witness.relationshipContainer.GetCompatibility(actor)))
				{
					reactions.Add(EMOTION.Arousal);
				}
				return;
			}
			if (witness.traitContainer.HasTrait("Coward", "Hemophobic"))
			{
				reactions.Add(EMOTION.Fear);
			}
			else if (!witness.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Threatened);
				if (witness.relationshipContainer.GetOpinionLabel(actor) == "Close Friend")
				{
					reactions.Add(EMOTION.Despair);
				}
				else
				{
					reactions.Add(EMOTION.Shock);
				}
			}
			if (target is Character character)
			{
				if (witness.relationshipContainer.IsFriendsWith(character) || witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(character))
				{
					reactions.Add(EMOTION.Anger);
				}
				else if (!witness.relationshipContainer.IsEnemiesWith(character) && (witness.relationshipContainer.GetOpinionLabel(character) == "Acquaintance" || (witness.faction != null && witness.faction == character.faction) || (witness.homeSettlement != null && witness.homeSettlement == character.homeSettlement)))
				{
					reactions.Add(EMOTION.Anger);
				}
			}
		}
		else if (witness.traitContainer.HasTrait("Hemophiliac"))
		{
			if (RelationshipManager.IsSexuallyCompatibleOneSided(witness, actor))
			{
				reactions.Add(EMOTION.Arousal);
			}
			else
			{
				reactions.Add(EMOTION.Approval);
			}
		}
		else if (witness.traitContainer.HasTrait("Hemophobic"))
		{
			reactions.Add(EMOTION.Threatened);
		}
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		return CRIME_TYPE.Vampire;
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Vampire;
	}

	public void PreVampireSuccess(ActualGoapNode goapNode)
	{
	}

	public void PerTickVampireSuccess(ActualGoapNode goapNode)
	{
	}

	public void AfterVampireSuccess(ActualGoapNode goapNode)
	{
	}
}
