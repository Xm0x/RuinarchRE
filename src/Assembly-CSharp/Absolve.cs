using Traits;

public class Absolve : GoapAction
{
	public Absolve()
		: base(INTERACTION_TYPE.ABSOLVE)
	{
		base.actionIconString = GoapActionStateDB.Judge_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Criminal", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Absolve Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public void AfterAbsolveSuccess(ActualGoapNode goapNode)
	{
		Character character = goapNode.target as Character;
		if (character.traitContainer.HasTrait("Criminal"))
		{
			character.traitContainer.GetTraitOrStatus<Criminal>("Criminal").SetIsImprisoned(state: false);
		}
		character.crimeComponent.SetDecisionAndJudgeToAllUnpunishedCrimesWantedBy(character.faction, CRIME_STATUS.Absolved, goapNode.actor);
		character.crimeComponent.RemoveAllCrimesWantedBy(goapNode.actor.faction);
		character.traitContainer.RemoveRestrainAndImprison(character, goapNode.actor);
	}
}
