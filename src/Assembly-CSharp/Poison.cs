using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UtilityScripts;

public class Poison : GoapAction
{
	public Poison()
		: base(INTERACTION_TYPE.POISON)
	{
		base.actionIconString = GoapActionStateDB.Poison_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Crimes };
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Poisoned", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Poison Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return Utilities.Rng.Next(80, 121);
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		Character character = null;
		if (target is TileObject tileObject)
		{
			character = tileObject.characterOwner;
		}
		if (character != null && character == witness)
		{
			if (witness.traitContainer.HasTrait("Coward"))
			{
				reactions.Add(EMOTION.Fear);
			}
			else
			{
				reactions.Add(EMOTION.Threatened);
			}
			if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.RELATIVE))
			{
				reactions.Add(EMOTION.Betrayal);
			}
		}
		else if (witness.traitContainer.IsReligiousCultist(RELIGION.Demon_Worship) && actor.traitContainer.IsReligiousCultist(RELIGION.Demon_Worship))
		{
			reactions.Add(EMOTION.Approval);
		}
		else if (character != null && (witness.relationshipContainer.IsFriendsWith(character) || witness.relationshipContainer.HasRelationshipWith(character, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.RELATIVE)))
		{
			if (witness.traitContainer.HasTrait("Coward"))
			{
				reactions.Add(EMOTION.Fear);
				return;
			}
			reactions.Add(EMOTION.Disapproval);
			if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.RELATIVE))
			{
				reactions.Add(EMOTION.Anger);
			}
		}
		else
		{
			reactions.Add(EMOTION.Disapproval);
			if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.RELATIVE))
			{
				reactions.Add(EMOTION.Anger);
			}
		}
	}

	public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		target.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned")?.AddAwareCharacter(witness);
		return base.ReactionToActor(actor, target, witness, node, status);
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		return CRIME_TYPE.Assault;
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Assault;
	}

	public void PrePoisonSuccess(ActualGoapNode goapNode)
	{
		goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, "Poisoned", goapNode.actor, bypassElementalChance: true, -1, 0f, ELEMENTAL_TYPE.Poison);
		goapNode.poiTarget.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned")?.SetIsPlayerSource(goapNode.associatedJobType == JOB_TYPE.CULTIST_INSTRUCTION);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null)
			{
				return false;
			}
			LocationGridTile gridTileLocation = poiTarget.gridTileLocation;
			if (gridTileLocation.structure.isDwelling)
			{
				if (!gridTileLocation.structure.IsOccupied())
				{
					return false;
				}
				Poisoned traitOrStatus = poiTarget.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned");
				if (traitOrStatus?.responsibleCharacters != null && traitOrStatus.responsibleCharacters.Contains(actor))
				{
					return false;
				}
				return !gridTileLocation.structure.IsResident(actor);
			}
		}
		return false;
	}
}
