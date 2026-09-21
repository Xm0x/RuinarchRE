using System.Collections.Generic;
using Interrupts;
using Object_Pools;
using UnityEngine;
using UtilityScripts;

public class ShareInformation : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.VERBAL;

	public ShareInformation()
		: base(INTERACTION_TYPE.SHARE_INFORMATION)
	{
		base.actionIconString = GoapActionStateDB.Gossip_Icon;
		base.doesNotStopTargetCharacter = true;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Informed,
			LOG_TAG.Social
		};
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Share Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		Character character = node.actor;
		IPointOfInterest pointOfInterest = node.poiTarget;
		if (node.disguisedActor != null)
		{
			character = node.disguisedActor;
		}
		if (node.disguisedTarget != null)
		{
			pointOfInterest = node.disguisedTarget;
		}
		OtherData[] otherData = node.otherData;
		if (otherData.Length != 1 || !(otherData[0].obj is IReactable))
		{
			return;
		}
		IReactable reactable = otherData[0].obj as IReactable;
		string text = string.Empty;
		if (reactable is ActualGoapNode actualGoapNode && actualGoapNode.action.goapType == INTERACTION_TYPE.SHARE_INFORMATION && actualGoapNode.otherData != null && actualGoapNode.otherData.Length == 1)
		{
			IReactable reactable2 = actualGoapNode.otherData[0].obj as IReactable;
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("GoapActionsStrings_Table", "Sharing");
			if (reactable2 is Rumor)
			{
				localizedValue = LocalizationManager.Instance.GetLocalizedValue("GoapActionsStrings_Table", "Spreading");
			}
			string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("GoapActionsStrings_Table", reactable2.classificationName);
			Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Actions", "GoapActionsStrings_Table", "Share_Information_Log");
			log2.AddToFillers(actualGoapNode.actor, actualGoapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log2.AddToFillers(null, localizedValue, LOG_IDENTIFIER.STRING_1);
			log2.AddToFillers(null, localizedValue2, LOG_IDENTIFIER.STRING_2);
			log2.AddToFillers(reactable2.actor, reactable2.actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			text = log2.logText;
			LogPool.Release(log2);
		}
		if (text == string.Empty)
		{
			text = reactable.informationLog.logText;
		}
		log.AddToFillers(character, character.name, LOG_IDENTIFIER.OTHER);
		log.AddToFillers(pointOfInterest, pointOfInterest.name, LOG_IDENTIFIER.OTHER_2);
		log.AddToFillers(null, text, LOG_IDENTIFIER.STRING_1);
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		IPointOfInterest poiTarget = node.poiTarget;
		if (!goapActionInvalidity.isInvalid)
		{
			Character character = poiTarget as Character;
			if (!character.carryComponent.IsNotBeingCarried())
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "target_carried";
			}
			else if (!character.limiterComponent.canWitness)
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "target_inactive";
			}
		}
		return goapActionInvalidity;
	}

	public override string ReactionToActor(Character actor, IPointOfInterest poiTarget, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		string result = base.ReactionToActor(actor, poiTarget, witness, node, status);
		IReactable reactable = node.otherData[0].obj as IReactable;
		if (reactable.name != "Share Information")
		{
			ProcessInformation(node.actor, witness, reactable, node);
		}
		return result;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		IReactable reactable = node.otherData[0].obj as IReactable;
		if (reactable.GetReactableEffect(witness) == REACTABLE_EFFECT.Negative)
		{
			if (witness == reactable.actor)
			{
				if (reactable is ActualGoapNode { isRumor: false, isIllusion: false })
				{
					reactions.Add(EMOTION.Embarassment);
				}
				else
				{
					reactions.Add(EMOTION.Anger);
				}
				if (witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE) || witness.relationshipContainer.IsFriendsWith(actor))
				{
					reactions.Add(EMOTION.Betrayal);
				}
			}
			else if (witness.relationshipContainer.HasRelationshipWith(reactable.actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE) || witness.relationshipContainer.IsFriendsWith(reactable.actor))
			{
				reactions.Add(EMOTION.Anger);
			}
			else if (reactable.target is Character character && (witness.relationshipContainer.HasRelationshipWith(character, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE) || witness.relationshipContainer.IsFriendsWith(character)))
			{
				reactions.Add(EMOTION.Anger);
			}
		}
		else if (witness == reactable.actor)
		{
			if (reactable is ActualGoapNode)
			{
				reactions.Add(EMOTION.Approval);
			}
			else
			{
				reactions.Add(EMOTION.Embarassment);
			}
		}
		else if (witness.relationshipContainer.IsEnemiesWith(reactable.actor))
		{
			reactions.Add(EMOTION.Disapproval);
		}
		else if (reactable.target is Character character2 && witness.relationshipContainer.IsEnemiesWith(character2))
		{
			reactions.Add(EMOTION.Disapproval);
		}
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		if ((node.otherData[0].obj as IReactable).GetReactableEffect(witness) == REACTABLE_EFFECT.Negative)
		{
			return REACTABLE_EFFECT.Negative;
		}
		return REACTABLE_EFFECT.Neutral;
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		return CRIME_TYPE.Rumormongering;
	}

	public void AfterShareSuccess(ActualGoapNode goapNode)
	{
		IReactable reactable = goapNode.otherData[0].obj as IReactable;
		Character actor = goapNode.actor;
		Character recipient = goapNode.poiTarget as Character;
		ProcessInformation(actor, recipient, reactable, goapNode);
	}

	private void ProcessInformation(Character sharer, Character recipient, IReactable reactable, ActualGoapNode shareActionItself)
	{
		Character character = reactable.actor;
		_ = reactable.target;
		if (reactable.disguisedActor != null)
		{
			character = reactable.disguisedActor;
		}
		if (reactable.disguisedTarget != null)
		{
			_ = reactable.disguisedTarget;
		}
		if (character == recipient)
		{
			return;
		}
		WeightedDictionary<string> weightedDictionary = new WeightedDictionary<string>();
		int num = 50;
		int num2 = 50;
		string opinionLabel = recipient.relationshipContainer.GetOpinionLabel(sharer);
		string opinionLabel2 = recipient.relationshipContainer.GetOpinionLabel(character);
		if (sharer.traitContainer.HasTrait("Persuasive"))
		{
			num += 500;
		}
		if ((reactable is Rumor || reactable is Assumption) && recipient.traitContainer.HasTrait("Suspicious"))
		{
			num2 += 2000;
		}
		else if (reactable is ActualGoapNode || reactable is InterruptHolder)
		{
			num += 100;
		}
		switch (opinionLabel)
		{
		case "Friend":
			num += 100;
			break;
		case "Close Friend":
			num += 250;
			break;
		case "Enemy":
			num2 += 100;
			break;
		case "Rival":
			num2 += 250;
			break;
		}
		switch (reactable.GetReactableEffect(recipient))
		{
		case REACTABLE_EFFECT.Positive:
			switch (opinionLabel2)
			{
			case "Friend":
			case "Close Friend":
				num += 500;
				break;
			case "Enemy":
				num2 += 250;
				break;
			case "Rival":
				num2 += 500;
				break;
			}
			break;
		case REACTABLE_EFFECT.Negative:
			switch (opinionLabel2)
			{
			case "Enemy":
			case "Rival":
				num += 250;
				break;
			case "Friend":
				num2 += 250;
				break;
			case "Close Friend":
				num2 += 500;
				break;
			}
			break;
		}
		weightedDictionary.AddElement("Belief", num);
		weightedDictionary.AddElement("Disbelief", num2);
		string text = weightedDictionary.PickRandomElementGivenWeights();
		if (text == "Belief")
		{
			recipient.reactionComponent.ReactTo(reactable, REACTION_STATUS.INFORMED, addLog: false);
		}
		else
		{
			CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, recipient, sharer, REACTION_STATUS.INFORMED, reactable as ActualGoapNode);
			if (Random.Range(0, 100) < 35)
			{
				recipient.jobComponent.CreateConfirmRumorJob(character, shareActionItself);
			}
		}
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.goapName + " " + text, LOG_TAG.Informed);
		log.AddToFillers(sharer, sharer.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(recipient, recipient.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		log.AddLogToDatabase(releaseLogAfter: true);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			Character character = poiTarget as Character;
			if (actor != character)
			{
				return !GameUtilities.IsRaceBeast(character.race);
			}
			return false;
		}
		return false;
	}
}
