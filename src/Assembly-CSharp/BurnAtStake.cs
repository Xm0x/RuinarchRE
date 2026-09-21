using Traits;

public class BurnAtStake : GoapAction
{
	public BurnAtStake()
		: base(INTERACTION_TYPE.BURN_AT_STAKE)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.actionIconString = GoapActionStateDB.Burn_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), CanDoBurnAtStake);
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Burn Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (actor != poiTarget && poiTarget is Character character)
			{
				if (!character.interruptComponent.isInterrupted && character.gridTileLocation != null)
				{
					return actor.homeSettlement != null;
				}
				return false;
			}
			return false;
		}
		return false;
	}

	private bool CanDoBurnAtStake(Character actor, IPointOfInterest target, object[] otherData, JOB_TYPE jobType)
	{
		if (target is Character character && target.traitContainer.HasTrait("Restrained"))
		{
			return character.gridTileLocation.structure.structureType == STRUCTURE_TYPE.WILDERNESS;
		}
		return false;
	}

	public void AfterBurnSuccess(ActualGoapNode goapNode)
	{
		Character character = goapNode.target as Character;
		if (character.traitContainer.HasTrait("Criminal"))
		{
			character.traitContainer.GetTraitOrStatus<Criminal>("Criminal").SetIsImprisoned(state: false);
		}
		character.crimeComponent.SetDecisionAndJudgeToAllUnpunishedCrimesWantedBy(character.faction, CRIME_STATUS.Burned_At_Stake, goapNode.actor);
		OtherData[] otherData = goapNode.otherData;
		if (otherData != null && otherData[0].obj is CrimeData crimeData && crimeData.IsCrimeFabricated())
		{
			crimeData.TryTriggerGrudgeAgainstJudgeOrReporter();
		}
		character.crimeComponent.RemoveAllCrimesWantedBy(goapNode.actor.faction);
		character.traitContainer.RemoveRestrainAndImprison(character, goapNode.actor);
		character.faction.KickOutCharacter(character);
		character.MigrateHomeStructureTo(null);
		character.ClearTerritory();
		character.interruptComponent.TriggerInterrupt(INTERRUPT.Burning_At_Stake, goapNode.actor);
	}
}
