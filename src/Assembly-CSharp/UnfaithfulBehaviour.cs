using System.Collections.Generic;
using Traits;
using UtilityScripts;

public class UnfaithfulBehaviour : CharacterBehaviour
{
	public UnfaithfulBehaviour()
	{
		base.priority = 20;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		int firstAliveOrUnspawnedRelationshipID = character.relationshipContainer.GetFirstAliveOrUnspawnedRelationshipID(RELATIONSHIP_TYPE.LOVER);
		if (firstAliveOrUnspawnedRelationshipID != -1 && ChanceData.RollChance(CHANCE_TYPE.Unfaithful_Active_Search_Affair))
		{
			POWER_ADDED_EFFECT p_afflictionSpecificBehaviour = POWER_ADDED_EFFECT.None;
			AfflictData afflictionData = PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.UNFAITHFULNESS);
			if (afflictionData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Wild_Multiple_Affair))
			{
				p_afflictionSpecificBehaviour = POWER_ADDED_EFFECT.Wild_Multiple_Affair;
			}
			else if (afflictionData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Multiple_Affair))
			{
				p_afflictionSpecificBehaviour = POWER_ADDED_EFFECT.Multiple_Affair;
			}
			else if (afflictionData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Active_Search_Affair))
			{
				p_afflictionSpecificBehaviour = POWER_ADDED_EFFECT.Active_Search_Affair;
			}
			List<Character> affairChoices = GetAffairChoices(p_afflictionSpecificBehaviour, character, firstAliveOrUnspawnedRelationshipID);
			if (affairChoices.Count > 0)
			{
				Character randomElement = CollectionUtilities.GetRandomElement(affairChoices);
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FIND_AFFAIR, INTERACTION_TYPE.HAVE_AFFAIR, randomElement, character);
				producedJob = goapPlanJob;
				RuinarchListPool<Character>.Release(affairChoices);
				return true;
			}
			RuinarchListPool<Character>.Release(affairChoices);
		}
		producedJob = null;
		return false;
	}

	private List<Character> GetAffairChoices(POWER_ADDED_EFFECT p_afflictionSpecificBehaviour, Character character, int loverID)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		Unfaithful traitOrStatus = character.traitContainer.GetTraitOrStatus<Unfaithful>("Unfaithful");
		switch (p_afflictionSpecificBehaviour)
		{
		case POWER_ADDED_EFFECT.Active_Search_Affair:
		case POWER_ADDED_EFFECT.Multiple_Affair:
			foreach (KeyValuePair<int, IRelationshipData> relationship in character.relationshipContainer.relationships)
			{
				if (relationship.Key != loverID && !relationship.Value.HasRelationship(RELATIONSHIP_TYPE.AFFAIR))
				{
					Character characterByID2 = CharacterManager.Instance.GetCharacterByID(relationship.Key);
					if (characterByID2 != null && !characterByID2.isDead && traitOrStatus.IsCompatibleBasedOnSexualityAndOpinions(character, characterByID2) && !character.IsHostileWith(characterByID2) && (characterByID2.race.IsSapient() || characterByID2.race == RACE.RATMAN) && RelationshipManager.Instance.GetValidator(character).CanHaveRelationship(characterByID2, character, RELATIONSHIP_TYPE.AFFAIR) && RelationshipManager.Instance.GetValidator(character).CanHaveRelationship(character, characterByID2, RELATIONSHIP_TYPE.AFFAIR))
					{
						list.Add(characterByID2);
					}
				}
			}
			break;
		case POWER_ADDED_EFFECT.Wild_Multiple_Affair:
		{
			foreach (KeyValuePair<int, IRelationshipData> relationship2 in character.relationshipContainer.relationships)
			{
				if (relationship2.Key != loverID && !relationship2.Value.HasRelationship(RELATIONSHIP_TYPE.AFFAIR))
				{
					Character characterByID = CharacterManager.Instance.GetCharacterByID(relationship2.Key);
					if (characterByID != null && !characterByID.isDead && traitOrStatus.IsCompatibleBasedOnSexualityAndOpinions(character, characterByID) && (!character.IsHostileWith(characterByID) || characterByID.combatComponent.combatMode == COMBAT_MODE.Passive) && RelationshipManager.Instance.GetValidator(character).CanHaveRelationship(characterByID, character, RELATIONSHIP_TYPE.AFFAIR) && RelationshipManager.Instance.GetValidator(character).CanHaveRelationship(character, characterByID, RELATIONSHIP_TYPE.AFFAIR))
					{
						list.Add(characterByID);
					}
				}
			}
			if (character.currentSettlement == null)
			{
				break;
			}
			for (int i = 0; i < character.currentSettlement.areas.Count; i++)
			{
				Area area = character.currentSettlement.areas[i];
				for (int j = 0; j < area.locationCharacterTracker.charactersAtLocation.Count; j++)
				{
					Character character2 = area.locationCharacterTracker.charactersAtLocation[j];
					if (!list.Contains(character2) && character2 != character && character2.id != loverID && !character.relationshipContainer.HasRelationshipWith(character2, RELATIONSHIP_TYPE.AFFAIR) && !character2.isDead && traitOrStatus.IsCompatibleBasedOnSexualityAndOpinions(character, character2) && (!character.IsHostileWith(character2) || character2.combatComponent.combatMode == COMBAT_MODE.Passive) && RelationshipManager.Instance.GetValidator(character).CanHaveRelationship(character2, character, RELATIONSHIP_TYPE.AFFAIR) && RelationshipManager.Instance.GetValidator(character).CanHaveRelationship(character, character2, RELATIONSHIP_TYPE.AFFAIR))
					{
						list.Add(character2);
					}
				}
			}
			break;
		}
		}
		return list;
	}
}
