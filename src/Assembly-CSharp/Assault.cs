using System.Collections.Generic;
using UtilityScripts;

public class Assault : GoapAction
{
	public Assault()
		: base(INTERACTION_TYPE.ASSAULT)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.TARGET_IN_VISION;
		base.actionIconString = GoapActionStateDB.Hostile_Icon;
		base.doesNotStopTargetCharacter = true;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Combat };
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.STARTS_COMBAT, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		Character actor = node.actor;
		_ = node.poiTarget;
		if (!goapActionInvalidity.isInvalid && actor.IsHealthCriticallyLow() && !actor.traitContainer.HasTrait("Berserked"))
		{
			goapActionInvalidity.isInvalid = true;
			goapActionInvalidity.reason = "low_health";
		}
		return goapActionInvalidity;
	}

	public override void Perform(ActualGoapNode actionNode)
	{
		base.Perform(actionNode);
		SetState("Combat Start", actionNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 50;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (node.crimeType == CRIME_TYPE.Vampire)
		{
			if (!(target is Character character))
			{
				return;
			}
			string opinionLabel = witness.relationshipContainer.GetOpinionLabel(character);
			if (CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, node.crimeType).IsConsideredACrime())
			{
				if (witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(character))
				{
					reactions.Add(EMOTION.Rage);
					reactions.Add(EMOTION.Threatened);
				}
				else if (opinionLabel == "Close Friend")
				{
					reactions.Add(EMOTION.Disapproval);
					reactions.Add(EMOTION.Anger);
					reactions.Add(EMOTION.Threatened);
				}
				else if (CharacterManager.Instance.IsCultistOfSameReligion(witness, actor))
				{
					reactions.Add(EMOTION.Approval);
					if (RelationshipManager.IsSexuallyCompatible(witness, actor) && GameUtilities.RollChance(10 * witness.relationshipContainer.GetCompatibility(actor)))
					{
						reactions.Add(EMOTION.Arousal);
					}
				}
				else if (opinionLabel == "Friend" || opinionLabel == "Acquaintance")
				{
					reactions.Add(EMOTION.Disapproval);
					reactions.Add(EMOTION.Threatened);
				}
				else if (character == witness)
				{
					if (GameUtilities.RollChance(50))
					{
						reactions.Add(EMOTION.Anger);
					}
					else
					{
						reactions.Add(EMOTION.Resentment);
					}
				}
			}
			else if (witness.traitContainer.HasTrait("Hemophiliac"))
			{
				if (RelationshipManager.IsSexuallyCompatible(witness, actor))
				{
					reactions.Add(EMOTION.Arousal);
				}
				else
				{
					reactions.Add(EMOTION.Approval);
				}
			}
			else if (witness.traitContainer.HasTrait("Hemophobic"))
			{
				reactions.Add(EMOTION.Threatened);
			}
		}
		else
		{
			if (actor.faction == null || !actor.faction.isMajorNonPlayer || actor.IsHostileWith(witness))
			{
				return;
			}
			if (target is Character character2)
			{
				string opinionLabel2 = witness.relationshipContainer.GetOpinionLabel(character2);
				if (node.associatedJobType == JOB_TYPE.KNOCKOUT)
				{
					reactions.Add(EMOTION.Approval);
				}
				else if (node.associatedJobType.IsApprehendTypeJob())
				{
					if (character2.crimeComponent.HasCrime(CRIME_SEVERITY.Serious, CRIME_SEVERITY.Heinous))
					{
						if (opinionLabel2 == "Close Friend")
						{
							reactions.Add(EMOTION.Resentment);
						}
						else if (witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(character2))
						{
							reactions.Add(EMOTION.Resentment);
						}
						else if (CharacterManager.Instance.IsCultistOfSameReligion(witness, actor))
						{
							reactions.Add(EMOTION.Approval);
							if (RelationshipManager.IsSexuallyCompatible(witness, actor) && GameUtilities.RollChance(10 * witness.relationshipContainer.GetCompatibility(actor)))
							{
								reactions.Add(EMOTION.Arousal);
							}
						}
						else if (opinionLabel2 == "Friend")
						{
							reactions.Add(EMOTION.Resentment);
						}
						else
						{
							reactions.Add(EMOTION.Approval);
						}
					}
					else if (opinionLabel2 == "Close Friend")
					{
						reactions.Add(EMOTION.Disapproval);
						reactions.Add(EMOTION.Anger);
					}
					else if (witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(character2))
					{
						reactions.Add(EMOTION.Disapproval);
						reactions.Add(EMOTION.Anger);
					}
					else if (CharacterManager.Instance.IsCultistOfSameReligion(witness, actor))
					{
						reactions.Add(EMOTION.Approval);
						if (RelationshipManager.IsSexuallyCompatible(witness, actor) && GameUtilities.RollChance(10 * witness.relationshipContainer.GetCompatibility(actor)))
						{
							reactions.Add(EMOTION.Arousal);
						}
					}
					else if (opinionLabel2 == "Friend")
					{
						reactions.Add(EMOTION.Disapproval);
						reactions.Add(EMOTION.Anger);
					}
					else
					{
						reactions.Add(EMOTION.Approval);
					}
				}
				else if (node.associatedJobType == JOB_TYPE.STALKER_HUNT)
				{
					if (CharacterManager.Instance.IsCultistOfSameReligion(witness, character2))
					{
						reactions.Add(EMOTION.Disapproval);
						reactions.Add(EMOTION.Anger);
					}
					else if (witness.relationshipContainer.IsFriendsWith(actor) || character2.crimeComponent.IsWantedBy(witness.faction, CRIME_TYPE.Vampire, CRIME_TYPE.Werewolf, CRIME_TYPE.Demon_Worship, CRIME_TYPE.Nature_Worship, CRIME_TYPE.Divine_Worship))
					{
						reactions.Add(EMOTION.Approval);
					}
					else if (witness.relationshipContainer.IsFriendsWith(character2))
					{
						reactions.Add(EMOTION.Disapproval);
						reactions.Add(EMOTION.Anger);
					}
					else if (!character2.isNormalCharacter)
					{
						reactions.Add(EMOTION.Approval);
					}
				}
				else if (opinionLabel2 == "Close Friend")
				{
					reactions.Add(EMOTION.Disapproval);
					reactions.Add(EMOTION.Anger);
					reactions.Add(EMOTION.Threatened);
				}
				else if (witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(character2))
				{
					reactions.Add(EMOTION.Disapproval);
					reactions.Add(EMOTION.Anger);
				}
				else
				{
					if (!CharacterManager.Instance.IsCultistOfSameReligion(witness, actor))
					{
						return;
					}
					reactions.Add(EMOTION.Approval);
					if (RelationshipManager.IsSexuallyCompatible(witness, actor))
					{
						if (GameUtilities.RollChance(10 * witness.relationshipContainer.GetCompatibility(actor)))
						{
							reactions.Add(EMOTION.Arousal);
						}
						return;
					}
					switch (opinionLabel2)
					{
					case "Friend":
					case "Acquaintance":
						reactions.Add(EMOTION.Disapproval);
						return;
					case "Enemy":
					case "Rival":
						reactions.Add(EMOTION.Approval);
						return;
					}
					if (!character2.isNormalCharacter)
					{
						reactions.Add(EMOTION.Disinterest);
					}
				}
			}
			else
			{
				if (!(target is TileObject tileObject))
				{
					return;
				}
				if (tileObject.IsOwnedBy(witness))
				{
					reactions.Add(EMOTION.Resentment);
					reactions.Add(EMOTION.Anger);
				}
				else if (CharacterManager.Instance.IsCultistOfSameReligion(witness, actor))
				{
					reactions.Add(EMOTION.Disinterest);
				}
				else if (tileObject.tileObjectType == TILE_OBJECT_TYPE.MONSTER_SPAWNER)
				{
					if (witness.isAlliedWithPlayer)
					{
						reactions.Add(EMOTION.Anger);
					}
					else
					{
						reactions.Add(EMOTION.Approval);
					}
				}
				else if (tileObject.tileObjectType == TILE_OBJECT_TYPE.TOMBSTONE)
				{
					Character character3 = null;
					if (tileObject is Tombstone tombstone)
					{
						character3 = tombstone.character;
					}
					string opinionLabel3 = witness.relationshipContainer.GetOpinionLabel(character3);
					switch (opinionLabel3)
					{
					case "Acquaintance":
						reactions.Add(EMOTION.Resentment);
						return;
					case "Friend":
					case "Close Friend":
						reactions.Add(EMOTION.Resentment);
						reactions.Add(EMOTION.Rage);
						return;
					}
					if (witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(character3))
					{
						reactions.Add(EMOTION.Resentment);
						reactions.Add(EMOTION.Rage);
					}
					else if (opinionLabel3 == "Enemy" || opinionLabel3 == "Rival")
					{
						reactions.Add(EMOTION.Disapproval);
					}
				}
				else
				{
					string opinionLabel4 = witness.relationshipContainer.GetOpinionLabel(actor);
					if (opinionLabel4 == "Friend" || opinionLabel4 == "Close Friend")
					{
						reactions.Add(EMOTION.Concern);
					}
					else if (witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(actor))
					{
						reactions.Add(EMOTION.Concern);
					}
					else if (witness.homeSettlement != null && tileObject.gridTileLocation != null && tileObject.gridTileLocation.IsPartOfSettlement(witness.homeSettlement))
					{
						reactions.Add(EMOTION.Disapproval);
					}
				}
			}
		}
	}

	public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);
		if (!(target is Character { faction: not null } character) || !character.faction.isMajorNonPlayer || witness.IsHostileWith(character))
		{
			return;
		}
		if (node.associatedJobType.IsApprehendTypeJob())
		{
			string opinionLabel = witness.relationshipContainer.GetOpinionLabel(character);
			bool num = character.crimeComponent.HasCrime(CRIME_SEVERITY.Serious, CRIME_SEVERITY.Heinous);
			bool flag = character.crimeComponent.HasCrime(CRIME_SEVERITY.Misdemeanor);
			if (num)
			{
				switch (opinionLabel)
				{
				case "Acquaintance":
					reactions.Add(EMOTION.Disappointment);
					return;
				case "Friend":
				case "Close Friend":
					reactions.Add(EMOTION.Disappointment);
					reactions.Add(EMOTION.Shock);
					return;
				}
				if (witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(character))
				{
					reactions.Add(EMOTION.Disappointment);
					reactions.Add(EMOTION.Anger);
				}
				else
				{
					reactions.Add(EMOTION.Disgust);
				}
			}
			else if (flag)
			{
				if (opinionLabel == "Friend" || opinionLabel == "Close Friend")
				{
					reactions.Add(EMOTION.Disappointment);
					reactions.Add(EMOTION.Concern);
				}
				else if (witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(character))
				{
					reactions.Add(EMOTION.Disappointment);
					reactions.Add(EMOTION.Concern);
				}
			}
		}
		else
		{
			reactions.Add(EMOTION.Concern);
		}
	}

	public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
		if (target is Character character && node.crimeType != CRIME_TYPE.None && node.crimeType != CRIME_TYPE.Unset && CrimeManager.Instance.GetCrimeSeverity(character, actor, target, node.crimeType).IsConsideredACrime())
		{
			reactions.Add(EMOTION.Resentment);
			if (character.relationshipContainer.IsFriendsWith(actor))
			{
				reactions.Add(EMOTION.Betrayal);
			}
			else if (character.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(actor))
			{
				reactions.Add(EMOTION.Betrayal);
			}
		}
	}

	public override void OnStoppedInterrupt(ActualGoapNode node)
	{
		base.OnStoppedInterrupt(node);
		node.actor.combatComponent.RemoveHostileInRange(node.poiTarget);
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		if (node.associatedJobType.IsApprehendTypeJob() || node.associatedJobType == JOB_TYPE.KNOCKOUT || (node.poiTarget is Character character && node.actor.faction != null && character.faction != null && node.actor.faction.IsHostileWith(node.actor.faction)))
		{
			return REACTABLE_EFFECT.Neutral;
		}
		return REACTABLE_EFFECT.Negative;
	}

	public override bool IsInvalidOnVision(ActualGoapNode node, out string reason)
	{
		reason = string.Empty;
		return false;
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		if (crime.associatedJobType == JOB_TYPE.SNATCH)
		{
			return CRIME_TYPE.Assault;
		}
		if (target is Character character)
		{
			if (character.race.IsSapient())
			{
				if (crime.associatedJobType == JOB_TYPE.ASSASSINATE)
				{
					return CRIME_TYPE.Murder;
				}
				if (!crime.associatedJobType.IsApprehendTypeJob() && crime.associatedJobType != JOB_TYPE.RESTRAIN && crime.associatedJobType != JOB_TYPE.STALKER_HUNT && crime.associatedJobType != JOB_TYPE.KNOCKOUT)
				{
					if (crime.associatedJobType.IsFullnessRecoveryTypeJob() || crime.associatedJobType == JOB_TYPE.IMPRISON_BLOOD_SOURCE)
					{
						return CRIME_TYPE.Vampire;
					}
					CombatData combatData = actor.combatComponent.GetCombatData(character);
					if (combatData != null && (combatData.reasonForCombat == "Retaliation" || combatData.reasonForCombat == "Hostility"))
					{
						return CRIME_TYPE.None;
					}
					if (actor.partyComponent.currentParty != null && actor.partyComponent.currentParty.currentQuest is BountyHuntPartyQuest bountyHuntPartyQuest && bountyHuntPartyQuest.targetCharacter == character)
					{
						return CRIME_TYPE.None;
					}
					if (character.crimeComponent.IsAnActiveCrimeWitnessedBy(actor, CRIME_STATUS.Unpunished))
					{
						return CRIME_TYPE.None;
					}
					return CRIME_TYPE.Assault;
				}
			}
		}
		else if (target is TileObject { characterOwner: not null } tileObject && !tileObject.IsOwnedBy(actor))
		{
			return CRIME_TYPE.Disturbances;
		}
		return base.GetCrimeType(actor, target, crime);
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Assault;
	}

	public override string GetActionIconString(ActualGoapNode node)
	{
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		return actor.combatComponent.GetCombatStateIconString(poiTarget, node);
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		string reason = GetReason(node);
		log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_1);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget == null || poiTarget.mapObjectVisual == null)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public void PreCombatStart(ActualGoapNode goapNode)
	{
		string reason = "Action";
		bool isLethal = goapNode.associatedJobType.IsJobLethal();
		if (goapNode.associatedJobType == JOB_TYPE.DEMON_KILL)
		{
			reason = "Assassination";
		}
		goapNode.actor.combatComponent.Fight(goapNode.poiTarget, reason, goapNode, isLethal);
		if (!(goapNode.poiTarget is Character character))
		{
			return;
		}
		List<Character> ownedPets = character.petComponent.ownedPets;
		if (ownedPets == null || ownedPets.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < character.petComponent.ownedPets.Count; i++)
		{
			Character character2 = character.petComponent.ownedPets[i];
			if (goapNode.actor.hasMarker && goapNode.actor.marker.IsPOIInVision(character2))
			{
				goapNode.actor.combatComponent.Fight(character2, reason, goapNode, isLethal);
			}
		}
	}

	private string GetReason(ActualGoapNode action)
	{
		string combatActionReason = action.actor.combatComponent.GetCombatActionReason(action, action.poiTarget);
		JOB_TYPE associatedJobType = action.associatedJobType;
		string result = LocalizationManager.Instance.GetLocalizedValue("CharacterCombat_Table", combatActionReason);
		if (string.IsNullOrEmpty(combatActionReason) || !LocalizationManager.Instance.HasLocalizedValue(result))
		{
			result = LocalizationManager.Instance.GetLocalizedValue("Jobs_Table", associatedJobType.ToStringEnum()) + ".";
		}
		return result;
	}
}
