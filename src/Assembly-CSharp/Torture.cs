using UnityEngine;
using UtilityScripts;

public class Torture : GoapAction
{
	public Torture()
		: base(INTERACTION_TYPE.TORTURE)
	{
		base.actionIconString = GoapActionStateDB.Anger_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Torture Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public void PerTickTortureSuccess(ActualGoapNode goapNode)
	{
		int maxHP = goapNode.poiTarget.maxHP;
		int num = Mathf.RoundToInt(0.03f * (float)maxHP);
		goapNode.poiTarget.AdjustHP(-num, ELEMENTAL_TYPE.Normal, triggerDeath: false, goapNode.actor, null, showHPBar: true);
	}

	public void AfterTortureSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		if (!(goapNode.poiTarget is Character character))
		{
			return;
		}
		if (!character.HasHealth())
		{
			character.Death("normal", goapNode, goapNode.actor, null, null, null, null, isPlayerSource: false, goapNode.actor);
			return;
		}
		string empty = string.Empty;
		int num = GameUtilities.RandomBetweenTwoNumbers(0, 99);
		if (num < 45)
		{
			empty = "nothing";
		}
		else if (num >= 45 && num < 80)
		{
			empty = "enslave";
			character.traitContainer.AddTrait(character, "Enslaved", actor);
		}
		else
		{
			empty = "injure";
			character.traitContainer.AddTrait(character, "Injured", actor);
		}
		if (empty != string.Empty)
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.goapName + " " + empty, LOG_TAG.Life_Changes, goapNode);
			log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddLogToDatabase(releaseLogAfter: true);
		}
	}
}
