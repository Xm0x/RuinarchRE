using System.Collections.Generic;

public class Recruit : GoapAction
{
	public Recruit()
		: base(INTERACTION_TYPE.RECRUIT)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.actionIconString = GoapActionStateDB.Work_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Recruit Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		if (!goapActionInvalidity.isInvalid)
		{
			Character character = node.poiTarget as Character;
			WeightedDictionary<bool> weightedDictionary = new WeightedDictionary<bool>();
			int num = 100;
			int weight = 200;
			if (node.actor.traitContainer.HasTrait("Inspiring"))
			{
				num += 100;
			}
			if (node.actor.traitContainer.HasTrait("Persuasive"))
			{
				num += 500;
			}
			if (node.actor.traitContainer.HasTrait("Vampire") && character.traitContainer.HasTrait("Hemophiliac"))
			{
				num += 1000;
			}
			weightedDictionary.AddElement(newElement: true, num);
			weightedDictionary.AddElement(newElement: false, weight);
			if (!weightedDictionary.PickRandomElementGivenWeights())
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.stateName = "Recruit Fail";
			}
		}
		return goapActionInvalidity;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (target is Character { prevFaction: not null } character && character.prevFaction.leader == witness)
		{
			reactions.Add(EMOTION.Anger);
		}
	}

	public void AfterRecruitSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		Character character = goapNode.poiTarget as Character;
		character.ChangeFactionTo(actor.faction, bypassIdeologyChecking: true);
		character.MigrateHomeTo(actor.homeSettlement);
		if (character is FireElemental)
		{
			character.MigrateHomeStructureTo(null, broadcast: true, addToRegionResidents: true, affectSettlement: false);
		}
		else if (character is VengefulGhost)
		{
			character.behaviourComponent.SetIsAttackingDemonicStructure(state: false, null);
			character.behaviourComponent.UpdateDefaultBehaviourSet();
		}
		character.traitContainer.RemoveTrait(character, "Criminal");
		character.traitContainer.RemoveRestrainAndImprison(character, goapNode.actor);
		if (character is Summon)
		{
			character.AdjustHP(character.maxHP, ELEMENTAL_TYPE.Normal, triggerDeath: false, null, null, showHPBar: true);
		}
		character.SetIsRecruitedByAMajorFaction(p_state: true);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (actor.faction != poiTarget.factionOwner)
			{
				return actor.homeSettlement != null;
			}
			return false;
		}
		return false;
	}
}
