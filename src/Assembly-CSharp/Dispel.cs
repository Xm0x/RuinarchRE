using System.Collections.Generic;
using Traits;

public class Dispel : GoapAction
{
	public Dispel()
		: base(INTERACTION_TYPE.DISPEL)
	{
		base.actionIconString = GoapActionStateDB.Magic_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Work,
			LOG_TAG.Life_Changes
		};
		base.showNotification = true;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Dispel Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Positive;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		string traitName = (string)node.otherData[0].obj;
		string traitLogString = GetTraitLogString(traitName);
		log.AddToFillers(null, traitLogString, LOG_IDENTIFIER.STRING_1);
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		string text = (string)node.otherData[0].obj;
		if (text == "Lycanthrope")
		{
			if (witness.traitContainer.HasTrait("Lycanphobic"))
			{
				reactions.Add(EMOTION.Approval);
				return;
			}
			if (witness.traitContainer.HasTrait("Lycanphiliac"))
			{
				reactions.Add(EMOTION.Rage);
				return;
			}
			bool flag = false;
			if (CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, CRIME_TYPE.Werewolf).IsConsideredACrime())
			{
				flag = true;
			}
			if (flag)
			{
				reactions.Add(EMOTION.Approval);
			}
			else if (witness.isLycanthrope && !witness.lycanData.dislikesBeingLycan)
			{
				reactions.Add(EMOTION.Threatened);
			}
			else
			{
				reactions.Add(EMOTION.Disinterest);
			}
		}
		else
		{
			if (!(text == "Vampire"))
			{
				return;
			}
			if (witness.traitContainer.HasTrait("Hemophobic"))
			{
				reactions.Add(EMOTION.Approval);
				return;
			}
			if (witness.traitContainer.HasTrait("Hemophiliac"))
			{
				reactions.Add(EMOTION.Rage);
				return;
			}
			bool flag2 = false;
			if (CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, CRIME_TYPE.Vampire).IsConsideredACrime())
			{
				flag2 = true;
			}
			Vampire traitOrStatus = witness.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
			if (flag2)
			{
				reactions.Add(EMOTION.Approval);
			}
			else if (traitOrStatus != null && !traitOrStatus.dislikedBeingVampire)
			{
				reactions.Add(EMOTION.Threatened);
			}
			else
			{
				reactions.Add(EMOTION.Disinterest);
			}
		}
	}

	public void PreDispelSuccess(ActualGoapNode goapNode)
	{
		string text = (string)goapNode.otherData[0].obj;
		if (!goapNode.poiTarget.traitContainer.HasTrait(text))
		{
			return;
		}
		Character character = goapNode.poiTarget as Character;
		string traitLogString = GetTraitLogString(text);
		if (text == "Lycanthrope")
		{
			if (character.lycanData != null && character.lycanData.isMaster && !character.lycanData.dislikesBeingLycan)
			{
				CreateResultLog(character.limiterComponent.canPerform ? "refuse" : "success", traitLogString, goapNode);
			}
			else
			{
				CreateResultLog("success", traitLogString, goapNode);
			}
		}
		else if (text == "Vampire")
		{
			Vampire traitOrStatus = character.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
			if (traitOrStatus != null && !traitOrStatus.dislikedBeingVampire)
			{
				CreateResultLog(character.limiterComponent.canPerform ? "refuse" : "success", traitLogString, goapNode);
			}
			else
			{
				CreateResultLog("success", traitLogString, goapNode);
			}
		}
	}

	public void AfterDispelSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.moneyComponent.AdjustCoins(83);
		string text = (string)goapNode.otherData[0].obj;
		if (goapNode.poiTarget.traitContainer.HasTrait(text))
		{
			Character character = goapNode.poiTarget as Character;
			Character actor = goapNode.actor;
			if (text == "Lycanthrope")
			{
				if (character.interruptComponent.isInterrupted && (character.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Transform_To_Wolf || character.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Transform_To_Werewolf))
				{
					character.interruptComponent.ForceEndNonSimultaneousInterrupt();
				}
				if (character.lycanData.isMaster && !character.lycanData.dislikesBeingLycan)
				{
					if (character.limiterComponent.canPerform)
					{
						actor.relationshipContainer.AdjustOpinion(actor, character, "Refused_Help", -8);
					}
					else
					{
						character.traitContainer.RemoveTrait(character, text, actor);
						character.relationshipContainer.AdjustOpinion(character, actor, "Removed_Lycanthropy", -15);
					}
				}
				else
				{
					character.traitContainer.RemoveTrait(character, text, actor);
					character.relationshipContainer.AdjustOpinion(character, actor, "Removed_Lycanthropy", 15);
				}
			}
			else if (text == "Vampire")
			{
				Vampire traitOrStatus = character.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
				if (traitOrStatus != null && !traitOrStatus.dislikedBeingVampire)
				{
					if (character.limiterComponent.canPerform)
					{
						actor.relationshipContainer.AdjustOpinion(actor, character, "Refused_Help", -8);
					}
					else
					{
						character.traitContainer.RemoveTrait(character, text, actor);
						character.relationshipContainer.AdjustOpinion(character, actor, "Removed_Vampirism", -15);
					}
				}
				else
				{
					character.traitContainer.RemoveTrait(character, text, actor);
					character.relationshipContainer.AdjustOpinion(character, actor, "Removed_Vampirism", 15);
				}
			}
		}
		goapNode.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Healing_Magic).AdjustExperience(10, goapNode.actor);
	}

	private string GetTraitLogString(string traitName)
	{
		if (traitName == "Lycanthrope")
		{
			return LocalizationManager.Instance.GetLocalizedValue("Afflictions_Table", "Lycanthropy");
		}
		return LocalizationManager.Instance.GetLocalizedValue("Afflictions_Table", "Vampirism");
	}

	private void CreateResultLog(string result, string logTraitName, ActualGoapNode goapNode)
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", goapNode.action.goapName + " " + result, base.logTags, goapNode);
		log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		log.AddToFillers(null, logTraitName, LOG_IDENTIFIER.STRING_1);
		goapNode.OverrideDescriptionLog(log);
	}
}
