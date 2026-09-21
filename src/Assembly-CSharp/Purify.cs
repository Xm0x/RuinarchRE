using Traits;
using UnityEngine;
using UtilityScripts;

public class Purify : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public Purify()
		: base(INTERACTION_TYPE.PURIFY)
	{
		base.actionIconString = GoapActionStateDB.Divine_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.logTags = new LOG_TAG[1];
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Purify Success", goapNode);
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
			int num = 100;
			if (node.poiTarget is Character character)
			{
				bool flag = false;
				bool flag2 = false;
				Vampire traitOrStatus = character.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
				if (traitOrStatus != null)
				{
					flag = traitOrStatus.dislikedBeingVampire;
				}
				if (character.isLycanthrope)
				{
					flag2 = character.lycanData.dislikesBeingLycan;
				}
				if (!flag && !flag2)
				{
					num -= Mathf.RoundToInt(character.piercingAndResistancesComponent.GetResistanceValue(RESISTANCE.Mental));
				}
			}
			if (!GameUtilities.RollChance(num))
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "failed_purify";
			}
		}
		return goapActionInvalidity;
	}

	public override void AfterInvalidAction(ActualGoapNode node, GoapActionInvalidity invalidity)
	{
		base.AfterInvalidAction(node, invalidity);
		if (invalidity.reason == "failed_purify" && node.poiTarget is Character character)
		{
			character.combatComponent.Fight(node.actor, "Threatened", null, isLethal: false);
		}
	}

	public void AfterPurifySuccess(ActualGoapNode goapNode)
	{
		IPointOfInterest poiTarget = goapNode.poiTarget;
		poiTarget.traitContainer.RemoveTrait(goapNode.poiTarget, "Lycanthrope");
		poiTarget.traitContainer.RemoveTrait(goapNode.poiTarget, "Vampire");
		poiTarget.traitContainer.RemoveTrait(goapNode.poiTarget, "Demon Cultist");
		poiTarget.traitContainer.RemoveTrait(goapNode.poiTarget, "Cleric");
		poiTarget.traitContainer.RemoveTrait(goapNode.poiTarget, "Witch");
		if (poiTarget is Character character)
		{
			if (character.characterClass.IsReligiousCultLeaderClass())
			{
				character.classComponent.AssignClass(character.classComponent.previousClassName);
			}
			character.religionComponent.DecreaseBeliefPoints(character.religionComponent.religion, 30);
			character.CancelAllJobs();
		}
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null)
			{
				return false;
			}
			return poiTarget.traitContainer.HasTrait("Lycanthrope", "Vampire", "Demon Cultist", "Cleric", "Witch");
		}
		return false;
	}
}
