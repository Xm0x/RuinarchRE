using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UtilityScripts;

public class IsImprisoned : GoapAction
{
	public IsImprisoned()
		: base(INTERACTION_TYPE.IS_IMPRISONED)
	{
		base.actionIconString = GoapActionStateDB.Hostile_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Crimes };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Imprisoned Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		if (node.otherData.Length == 1 && node.otherData[0].obj is LocationStructure locationStructure)
		{
			log.AddToFillers(locationStructure, locationStructure.GetNameRelativeTo(node.actor), LOG_IDENTIFIER.LANDMARK_1, replaceExisting: true, overrideStringValue: true);
		}
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		Character character = target as Character;
		if (!character.traitContainer.HasTrait("Restrained", "Prisoner"))
		{
			return;
		}
		BaseSettlement currentSettlement = character.currentSettlement;
		if (currentSettlement != null)
		{
			Faction owner = currentSettlement.owner;
			if (owner != null && owner.isPlayerFaction && witness.isAlliedWithPlayer)
			{
				return;
			}
		}
		string opinionLabel = witness.relationshipContainer.GetOpinionLabel(character);
		if (opinionLabel == "Enemy" || opinionLabel == "Rival")
		{
			reactions.Add(EMOTION.Scorn);
		}
		else
		{
			if (witness.faction == null || character.faction == null)
			{
				return;
			}
			if (witness.faction == character.faction)
			{
				switch (opinionLabel)
				{
				case "Acquaintance":
				case "Friend":
				case "Close Friend":
					reactions.Add(EMOTION.Distraught);
					break;
				}
			}
			else if (witness.faction.IsHostileWith(character.faction))
			{
				if (!witness.IsHostileWith(character))
				{
					reactions.Add(EMOTION.Concern);
				}
			}
			else if (opinionLabel == "Friend" || opinionLabel == "Close Friend")
			{
				reactions.Add(EMOTION.Distraught);
			}
		}
	}

	public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		string text = string.Empty;
		Character character = target as Character;
		if (!character.traitContainer.HasTrait("Restrained", "Prisoner"))
		{
			return "Already_Free";
		}
		string opinionLabel = witness.relationshipContainer.GetOpinionLabel(character);
		if (opinionLabel == "Enemy" || opinionLabel == "Rival")
		{
			text = "Target_Rots";
		}
		else
		{
			bool flag = true;
			BaseSettlement currentSettlement = character.currentSettlement;
			if (currentSettlement != null)
			{
				Faction owner = currentSettlement.owner;
				if (owner != null && owner.isPlayerFaction && witness.isAlliedWithPlayer)
				{
					text = "Do_What_You_Want";
					flag = false;
				}
			}
			if (flag && witness.faction != null && character.faction != null)
			{
				if (witness.faction == character.faction)
				{
					switch (opinionLabel)
					{
					case "Acquaintance":
					case "Friend":
					case "Close Friend":
						text = ((!Distraught.WillWitnessCreateRescueQuestWhenFelt(witness, character)) ? "Dont_Hurt" : "Organize_Rescue_Party");
						break;
					}
				}
				else if (witness.faction.IsHostileWith(character.faction))
				{
					text = (witness.IsHostileWith(character) ? "Stop_Telling_News" : "Cannot_Do_Anything");
				}
				else
				{
					switch (opinionLabel)
					{
					case "Friend":
					case "Close Friend":
						text = ((!Distraught.WillWitnessCreateRescueQuestWhenFelt(witness, character)) ? "Dont_Hurt" : "Organize_Rescue_Party");
						break;
					default:
						if (witness.relationshipContainer.IsKnown(character))
						{
							break;
						}
						goto case "Acquaintance";
					case "Acquaintance":
						text = string.Empty;
						break;
					}
				}
			}
		}
		List<EMOTION> list = RuinarchListPool<EMOTION>.Claim(5);
		PopulateEmotionReactionsToActor(list, actor, target, witness, node, status);
		int p_totalOpinionReduction = 0;
		string p_lastStrawReasonKey = string.Empty;
		bool flag2 = !string.IsNullOrEmpty(text);
		for (int i = 0; i < list.Count; i++)
		{
			string text2 = CharacterManager.Instance.TriggerEmotion(list[i], witness, actor, status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, node, "", p_triggerOpinionChangesEffect: false);
			if (!flag2)
			{
				text += text2;
			}
		}
		if (p_totalOpinionReduction < 0 && !witness.reactionComponent.isDisguised && !actor.reactionComponent.isDisguised)
		{
			witness.relationshipContainer.CreateJobsOnOpinionReduced(witness, actor, p_lastStrawReasonKey, p_totalOpinionReduction);
		}
		return text;
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Neutral;
	}

	public override BLACKMAIL_TYPE GetBlackMailTypeConsideringTarget(ActualGoapNode p_goapNode, Character p_targetCharacter)
	{
		Character actor = p_goapNode.actor;
		if (actor != p_targetCharacter)
		{
			if (p_targetCharacter.relationshipContainer.HasSpecialPositiveRelationshipWith(actor) && !p_targetCharacter.relationshipContainer.IsEnemiesWith(actor))
			{
				return BLACKMAIL_TYPE.Strong;
			}
			if (p_targetCharacter.relationshipContainer.IsFriendsWith(actor))
			{
				return BLACKMAIL_TYPE.Normal;
			}
		}
		return base.GetBlackMailTypeConsideringTarget(p_goapNode, p_targetCharacter);
	}

	public void PreImprisonedSuccess(ActualGoapNode goapNode)
	{
	}

	public void PerTickImprisonedSuccess(ActualGoapNode goapNode)
	{
	}

	public void AfterImprisonedSuccess(ActualGoapNode goapNode)
	{
	}
}
