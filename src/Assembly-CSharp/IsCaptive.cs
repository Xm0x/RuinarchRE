using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Traits;
using UtilityScripts;

public class IsCaptive : GoapAction
{
	public IsCaptive()
		: base(INTERACTION_TYPE.IS_CAPTIVE)
	{
		base.actionIconString = GoapActionStateDB.Hostile_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Crimes };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Captive Success", goapNode);
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
			bool flag = true;
			if (witness.isFactionLeader || witness.isSettlementRuler)
			{
				bool flag2 = false;
				BaseSettlement currentSettlement = character.currentSettlement;
				if (currentSettlement != null)
				{
					if (currentSettlement.owner != null && !witness.faction.IsHostileWith(currentSettlement.owner))
					{
						if (witness.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Warmonger))
						{
							if (witness.faction == character.faction)
							{
								switch (opinionLabel)
								{
								case "Acquaintance":
								case "Friend":
								case "Close Friend":
									flag2 = true;
									break;
								}
							}
							else if (opinionLabel == "Close Friend")
							{
								flag2 = true;
							}
						}
						else if (!witness.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Peaceful) && opinionLabel == "Close Friend")
						{
							flag2 = true;
						}
					}
					if (flag2)
					{
						reactions.Add(EMOTION.Concern);
						flag = false;
					}
				}
			}
			if (!flag)
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
					reactions.Add(EMOTION.Concern);
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
				reactions.Add(EMOTION.Concern);
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
			Prisoner traitOrStatus = character.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
			if (traitOrStatus != null)
			{
				if (traitOrStatus.IsFactionPrisonerOf(witness.faction))
				{
					text = "Faction_Prisoner";
					flag = false;
				}
				else if (traitOrStatus.IsPersonalPrisonerOf(witness))
				{
					text = "Personal_Prisoner";
					flag = false;
				}
			}
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
				if (witness.isFactionLeader || witness.isSettlementRuler)
				{
					bool flag2 = false;
					if (currentSettlement != null)
					{
						Faction owner2 = currentSettlement.owner;
						if (owner2 != null && !witness.faction.IsHostileWith(currentSettlement.owner))
						{
							if (witness.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Warmonger))
							{
								if (witness.faction == character.faction)
								{
									switch (opinionLabel)
									{
									case "Acquaintance":
									case "Friend":
									case "Close Friend":
										flag2 = true;
										break;
									}
								}
								else if (opinionLabel == "Close Friend")
								{
									flag2 = true;
								}
							}
							else if (!witness.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Peaceful) && opinionLabel == "Close Friend")
							{
								flag2 = true;
							}
						}
						if (flag2)
						{
							DeclareWarAndPostRescueQuest(witness, character, owner2);
							text = "This_Means_War";
							flag = false;
						}
					}
				}
				if (flag)
				{
					if (witness.faction == character.faction)
					{
						switch (opinionLabel)
						{
						case "Acquaintance":
						case "Friend":
						case "Close Friend":
							if (ShouldOrganizeRescueParty(witness, character))
							{
								PostRescueQuest(witness, character);
								text = "Organize_Rescue_Party";
							}
							else
							{
								witness.jobComponent.TriggerReleaseJob(character);
								text = "Personal_Rescue";
							}
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
							if (ShouldOrganizeRescueParty(witness, character))
							{
								PostRescueQuest(witness, character);
								text = "Organize_Rescue_Party";
							}
							else
							{
								witness.jobComponent.TriggerReleaseJob(character);
								text = "Personal_Rescue";
							}
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
		}
		List<EMOTION> list = RuinarchListPool<EMOTION>.Claim(5);
		PopulateEmotionReactionsToActor(list, actor, target, witness, node, status);
		int p_totalOpinionReduction = 0;
		string p_lastStrawReasonKey = string.Empty;
		bool flag3 = !string.IsNullOrEmpty(text);
		for (int i = 0; i < list.Count; i++)
		{
			string text2 = CharacterManager.Instance.TriggerEmotion(list[i], witness, actor, status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, node, "", p_triggerOpinionChangesEffect: false);
			if (!flag3)
			{
				text += text2;
			}
		}
		if (p_totalOpinionReduction < 0 && !witness.reactionComponent.isDisguised && !actor.reactionComponent.isDisguised)
		{
			witness.relationshipContainer.CreateJobsOnOpinionReduced(witness, actor, p_lastStrawReasonKey, p_totalOpinionReduction);
		}
		RuinarchListPool<EMOTION>.Release(list);
		return text;
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Neutral;
	}

	public void PreCaptiveSuccess(ActualGoapNode goapNode)
	{
	}

	public void PerTickCaptiveSuccess(ActualGoapNode goapNode)
	{
	}

	public void AfterCaptiveSuccess(ActualGoapNode goapNode)
	{
	}

	private void PostRescueQuest(Character p_rescuer, Character p_rescuedCharacter)
	{
		if (p_rescuer.faction != null && !p_rescuer.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Rescue, p_rescuedCharacter) && !p_rescuer.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Demon_Rescue, p_rescuedCharacter))
		{
			p_rescuer.faction.partyQuestBoard.CreateRescuePartyQuest(p_rescuer, p_rescuer.homeSettlement, p_rescuedCharacter);
		}
	}

	private void DeclareWarAndPostRescueQuest(Character p_rescuer, Character p_rescuedCharacter, Faction p_targetFaction)
	{
		if (p_rescuer.faction != null)
		{
			if (p_targetFaction.leader is Character targetPOI)
			{
				p_rescuer.interruptComponent.TriggerInterrupt(INTERRUPT.Declare_War, targetPOI);
			}
			PostRescueQuest(p_rescuer, p_rescuedCharacter);
		}
	}

	private bool ShouldOrganizeRescueParty(Character p_rescuer, Character p_rescuedCharacter)
	{
		if (p_rescuedCharacter.gridTileLocation != null && !p_rescuedCharacter.IsAtHome() && !p_rescuedCharacter.gridTileLocation.IsInHomeOf(p_rescuer) && p_rescuedCharacter.gridTileLocation.structure.structureType != STRUCTURE_TYPE.WILDERNESS)
		{
			return true;
		}
		return false;
	}
}
