using System.Collections.Generic;
using UtilityScripts;

namespace Traits;

public class Unfaithful : Trait
{
	public Unfaithful()
	{
		name = "Unfaithful";
		description = "Cannot commit to a monogamous relationship.";
		type = TRAIT_TYPE.FLAW;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = 0;
		canBeTriggered = true;
	}

	public override string TriggerFlaw(Character character, bool isTriggeredByPlayer = true)
	{
		string result = base.TriggerFlaw(character);
		int firstAliveOrUnspawnedRelationshipID = character.relationshipContainer.GetFirstAliveOrUnspawnedRelationshipID(RELATIONSHIP_TYPE.LOVER);
		if (firstAliveOrUnspawnedRelationshipID != -1)
		{
			List<int> list = RuinarchListPool<int>.Claim();
			character.relationshipContainer.PopulateAllRelatableIDWithRelationship(list, RELATIONSHIP_TYPE.AFFAIR);
			Character character2 = null;
			for (int i = 0; i < list.Count; i++)
			{
				int id = list[i];
				Character characterByID = CharacterManager.Instance.GetCharacterByID(id);
				if (characterByID != null && !characterByID.isDead)
				{
					character2 = characterByID;
					break;
				}
			}
			RuinarchListPool<int>.Release(list);
			if (character2 == null)
			{
				if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW))
				{
					List<Character> list2 = new List<Character>();
					foreach (KeyValuePair<int, IRelationshipData> relationship in character.relationshipContainer.relationships)
					{
						if (relationship.Key != firstAliveOrUnspawnedRelationshipID && !relationship.Value.HasRelationship(RELATIONSHIP_TYPE.AFFAIR))
						{
							Character characterByID2 = CharacterManager.Instance.GetCharacterByID(relationship.Key);
							if (characterByID2 != null && !characterByID2.isDead && RelationshipManager.IsSexuallyCompatible(character, characterByID2) && RelationshipManager.Instance.GetValidator(character).CanHaveRelationship(character, characterByID2, RELATIONSHIP_TYPE.AFFAIR))
							{
								list2.Add(characterByID2);
							}
						}
					}
					if (list2.Count > 0)
					{
						Character randomElement = CollectionUtilities.GetRandomElement(list2);
						GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.HAVE_AFFAIR, randomElement, character);
						goapPlanJob.SetIsTriggeredByPlayer(isTriggeredByPlayer);
						character.jobQueue.AddJobInQueue(goapPlanJob);
						return result;
					}
					return "fail_no_paramour";
				}
			}
			else if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW))
			{
				GoapPlanJob goapPlanJob2 = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.MAKE_LOVE, character2, character);
				goapPlanJob2.SetIsTriggeredByPlayer(isTriggeredByPlayer);
				character.jobQueue.AddJobInQueue(goapPlanJob2);
			}
			return result;
		}
		return "fail";
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			character.behaviourComponent.AddBehaviourComponent(typeof(UnfaithfulBehaviour));
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character.behaviourComponent.RemoveBehaviourComponent(typeof(UnfaithfulBehaviour));
		}
	}

	public bool IsCompatibleBasedOnSexualityAndOpinions(Character p_character1, Character p_character2)
	{
		if (p_character1.HasAfflictedByPlayerWith(this))
		{
			if (PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.UNFAITHFULNESS).HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Wild_Multiple_Affair))
			{
				return !p_character1.relationshipContainer.IsEnemiesWith(p_character2);
			}
			if (RelationshipManager.IsSexuallyCompatible(p_character1, p_character2))
			{
				return !p_character1.relationshipContainer.IsEnemiesWith(p_character2);
			}
			return false;
		}
		if (RelationshipManager.IsSexuallyCompatible(p_character1, p_character2))
		{
			return !p_character1.relationshipContainer.IsEnemiesWith(p_character2);
		}
		return false;
	}

	public bool CanBeLoverOrAffairBasedOnPersonalConstraints(Character p_character1, Character p_character2)
	{
		if (p_character1.HasAfflictedByPlayerWith(this))
		{
			AfflictData afflictionData = PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.UNFAITHFULNESS);
			if (afflictionData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Wild_Multiple_Affair))
			{
				return true;
			}
			if (afflictionData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Multiple_Affair))
			{
				if (p_character2.raceSetting.category == CHARACTER_CATEGORY.Beast || p_character2.raceSetting.category == CHARACTER_CATEGORY.Undead)
				{
					return false;
				}
				return !p_character1.relationshipContainer.IsFamilyMember(p_character2);
			}
			if (p_character2.raceSetting.category == CHARACTER_CATEGORY.Beast || p_character2.raceSetting.category == CHARACTER_CATEGORY.Undead)
			{
				return false;
			}
			if (p_character1.relationshipContainer.HasAliveOrUnspawnedRelationship(RELATIONSHIP_TYPE.AFFAIR))
			{
				return false;
			}
			return !p_character1.relationshipContainer.IsFamilyMember(p_character2);
		}
		if (p_character2.raceSetting.category == CHARACTER_CATEGORY.Beast || p_character2.raceSetting.category == CHARACTER_CATEGORY.Undead)
		{
			return false;
		}
		if (p_character1.relationshipContainer.HasAliveOrUnspawnedRelationship(RELATIONSHIP_TYPE.AFFAIR))
		{
			return false;
		}
		return !p_character1.relationshipContainer.IsFamilyMember(p_character2);
	}
}
