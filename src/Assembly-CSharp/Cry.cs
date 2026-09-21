using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class Cry : GoapAction
{
	private readonly string[] _costTraits = new string[7] { "Worried", "Exhausted", "Traumatized", "Heartbroken", "Betrayed", "Dolorous", "Griefstricken" };

	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;

	public Cry()
		: base(INTERACTION_TYPE.CRY)
	{
		base.actionIconString = GoapActionStateDB.Sad_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Cry Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		int num = Utilities.Rng.Next(100, 116);
		int num2 = 20 * actor.jobComponent.GetNumOfTimesActionDone(this);
		num += num2;
		if (actor.traitContainer.HasTrait(_costTraits))
		{
			for (int i = 0; i < _costTraits.Length; i++)
			{
				string traitName = _costTraits[i];
				if (actor.traitContainer.HasTrait(traitName))
				{
					int num3 = Utilities.Rng.Next(10, 31);
					num -= num3;
				}
			}
		}
		else
		{
			num += 2000;
		}
		return num;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		switch (witness.relationshipContainer.GetOpinionLabel(actor))
		{
		case "Enemy":
		case "Rival":
			if (Random.Range(0, 2) == 0)
			{
				reactions.Add(EMOTION.Scorn);
			}
			break;
		case "Friend":
		case "Close Friend":
			if (!witness.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Concern);
			}
			break;
		case "Acquaintance":
			if (!witness.traitContainer.HasTrait("Psychopath") && Random.Range(0, 2) == 0)
			{
				reactions.Add(EMOTION.Concern);
			}
			break;
		}
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public override bool IsHappinessRecoveryAction()
	{
		return true;
	}

	public void PreCrySuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(base.goapType);
		string value = (goapNode.actor.traitContainer.HasTrait("Griefstricken") ? LocalizationManager.Instance.GetLocalizedValue("GoapActionsStrings_Table", "Cry_Grieving") : (goapNode.actor.traitContainer.HasTrait("Heartbroken") ? LocalizationManager.Instance.GetLocalizedValue("GoapActionsStrings_Table", "Cry_Heartbroken") : (goapNode.actor.traitContainer.HasTrait("Worried") ? LocalizationManager.Instance.GetLocalizedValue("GoapActionsStrings_Table", "Cry_Worried") : (goapNode.actor.traitContainer.HasTrait("Traumatized") ? LocalizationManager.Instance.GetLocalizedValue("GoapActionsStrings_Table", "Cry_Traumatized") : ((!goapNode.actor.traitContainer.HasTrait("Betrayed")) ? LocalizationManager.Instance.GetLocalizedValue("GoapActionsStrings_Table", "Cry_Sad") : LocalizationManager.Instance.GetLocalizedValue("GoapActionsStrings_Table", "Cry_Betrayed"))))));
		goapNode.descriptionLog.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
	}

	public void PerTickCrySuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.needsComponent.AdjustHappiness(-2f);
	}

	public void AfterCrySuccess(ActualGoapNode goapNode)
	{
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return actor == poiTarget;
		}
		return false;
	}
}
