using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Traits;
using UnityEngine;
using UtilityScripts;

public class BaseRelationshipContainer : IRelationshipContainer
{
	public Dictionary<int, IRelationshipData> relationships { get; private set; }

	public List<Character> charactersWithOpinion { get; private set; }

	public BaseRelationshipContainer()
	{
		relationships = new Dictionary<int, IRelationshipData>();
		charactersWithOpinion = new List<Character>();
		Messenger.AddListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnNewVillagerArrived);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
	}

	public BaseRelationshipContainer(SaveDataBaseRelationshipContainer data)
	{
		relationships = new Dictionary<int, IRelationshipData>(data.relationships);
		charactersWithOpinion = SaveUtilities.ConvertIDListToCharacters(data.charactersWithOpinion);
		foreach (KeyValuePair<int, List<string>> sharedOpinionModifier in data.sharedOpinionModifiers)
		{
			for (int i = 0; i < sharedOpinionModifier.Value.Count; i++)
			{
				string id = sharedOpinionModifier.Value[i];
				SharedOpinionModifier opinionModifierByPersistentID = DatabaseManager.Instance.sharedOpinionDatabase.GetOpinionModifierByPersistentID(id);
				relationships[sharedOpinionModifier.Key].opinions.LoadSharedOpinion(opinionModifierByPersistentID);
			}
		}
		Messenger.AddListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnNewVillagerArrived);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
	}

	public void OnNewVillagerArrived(Character character)
	{
		if (HasRelationshipWith(character) && !charactersWithOpinion.Contains(character))
		{
			charactersWithOpinion.Add(character);
		}
	}

	public void AddRelationship(Relatable owner, Relatable relatable, RELATIONSHIP_TYPE relType)
	{
		if (!HasRelationshipWith(relatable))
		{
			CreateNewRelationship(owner, relatable);
		}
		relationships[relatable.id].AddRelationship(relType);
		Messenger.Broadcast(CharacterSignals.RELATIONSHIP_TYPE_ADDED, owner, relatable);
	}

	public IRelationshipData CreateNewRelationship(Relatable owner, Relatable relatable)
	{
		IRelationshipData relationshipData = new BaseRelationshipData();
		relationshipData.SetTargetName(relatable.relatableName);
		relationshipData.SetTargetGender(relatable.gender);
		relationships.Add(relatable.id, relationshipData);
		if (relatable is Character item && !charactersWithOpinion.Contains(item))
		{
			charactersWithOpinion.Add(item);
		}
		Messenger.Broadcast(CharacterSignals.RELATIONSHIP_CREATED, owner, relatable);
		return relationshipData;
	}

	public IRelationshipData CreateNewRelationship(Relatable owner, int id, string name, GENDER gender)
	{
		IRelationshipData relationshipData = new BaseRelationshipData();
		relationshipData.SetTargetName(name);
		relationshipData.SetTargetGender(gender);
		relationships.Add(id, relationshipData);
		Character characterByID = CharacterManager.Instance.GetCharacterByID(id);
		if (characterByID != null && !charactersWithOpinion.Contains(characterByID))
		{
			charactersWithOpinion.Add(characterByID);
		}
		Messenger.Broadcast<Relatable, Relatable>(CharacterSignals.RELATIONSHIP_CREATED, owner, null);
		return relationshipData;
	}

	public IRelationshipData GetOrCreateRelationshipDataWith(Relatable owner, Relatable relatable)
	{
		if (!HasRelationshipWith(relatable))
		{
			CreateNewRelationship(owner, relatable);
		}
		return relationships[relatable.id];
	}

	public IRelationshipData GetOrCreateRelationshipDataWith(Relatable owner, int id, string name, GENDER gender)
	{
		if (!HasRelationshipWith(id))
		{
			CreateNewRelationship(owner, id, name, gender);
		}
		return relationships[id];
	}

	private bool TryGetRelationshipDataWith(Relatable relatable, out IRelationshipData data)
	{
		return TryGetRelationshipDataWith(relatable.id, out data);
	}

	private bool TryGetRelationshipDataWith(int id, out IRelationshipData data)
	{
		return relationships.TryGetValue(id, out data);
	}

	public void RemoveRelationship(Relatable relatable, RELATIONSHIP_TYPE rel)
	{
		relationships[relatable.id].RemoveRelationship(rel);
	}

	public bool HasRelationshipWith(Relatable relatable)
	{
		return HasRelationshipWith(relatable.id);
	}

	public bool HasRelationshipWith(int id)
	{
		return relationships.ContainsKey(id);
	}

	public bool HasSpecialRelationshipWith(Relatable relatable)
	{
		IRelationshipData relationshipDataWith = GetRelationshipDataWith(relatable);
		if (relationshipDataWith != null && relationshipDataWith.relationships != null && relationshipDataWith.relationships.Count > 0)
		{
			return true;
		}
		return false;
	}

	public bool HasRelationshipWith(Relatable relatable, RELATIONSHIP_TYPE relType)
	{
		if (HasRelationshipWith(relatable))
		{
			return relationships[relatable.id].HasRelationship(relType);
		}
		return false;
	}

	public bool HasRelationshipWith(Relatable relatable, RELATIONSHIP_TYPE relType1, RELATIONSHIP_TYPE relType2)
	{
		if (HasRelationshipWith(relatable))
		{
			return relationships[relatable.id].HasRelationship(relType1, relType2);
		}
		return false;
	}

	public bool HasRelationshipWith(Relatable relatable, RELATIONSHIP_TYPE relType1, RELATIONSHIP_TYPE relType2, RELATIONSHIP_TYPE relType3)
	{
		if (HasRelationshipWith(relatable))
		{
			return relationships[relatable.id].HasRelationship(relType1, relType2, relType3);
		}
		return false;
	}

	public bool HasRelationshipWith(Relatable relatable, RELATIONSHIP_TYPE relType1, RELATIONSHIP_TYPE relType2, RELATIONSHIP_TYPE relType3, RELATIONSHIP_TYPE relType4, RELATIONSHIP_TYPE relType5)
	{
		if (HasRelationshipWith(relatable))
		{
			return relationships[relatable.id].HasRelationship(relType1, relType2, relType3, relType4, relType5);
		}
		return false;
	}

	public bool HasGrudgeAgainst(Relatable p_target)
	{
		return GetRelationshipDataWith(p_target)?.hasGrudge ?? false;
	}

	public bool HasGrudgeAgainstAliveCharactersWithPath(Character p_source)
	{
		for (int i = 0; i < charactersWithOpinion.Count; i++)
		{
			Character character = charactersWithOpinion[i];
			IRelationshipData relationshipDataWith = GetRelationshipDataWith(character);
			if (relationshipDataWith != null && relationshipDataWith.hasGrudge && !character.isDead && p_source.movementComponent.HasPathToEvenIfDiffRegion(character.gridTileLocation))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsFamilyMember(Character target)
	{
		if (HasRelationshipWith(target))
		{
			return GetRelationshipDataWith(target).IsFamilyMember();
		}
		return false;
	}

	public bool IsLoverOrAffair(Character target)
	{
		if (HasRelationshipWith(target))
		{
			return GetRelationshipDataWith(target).IsLoverOrAffair();
		}
		return false;
	}

	public bool HasRelationship(RELATIONSHIP_TYPE type)
	{
		foreach (KeyValuePair<int, IRelationshipData> relationship in relationships)
		{
			if (relationship.Value.HasRelationship(type))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAliveOrUnspawnedRelationship(RELATIONSHIP_TYPE type)
	{
		foreach (KeyValuePair<int, IRelationshipData> relationship in relationships)
		{
			if (relationship.Value.HasRelationship(type))
			{
				Character characterByID = CharacterManager.Instance.GetCharacterByID(relationship.Key);
				if (characterByID == null || !characterByID.isDead)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasRelationshipWithAliveCharacter(RELATIONSHIP_TYPE type)
	{
		if (GetFirstAliveCharacterWithRelationship(type) != null)
		{
			return true;
		}
		return false;
	}

	public bool HasRelationshipWithSpawnedCharacter(RELATIONSHIP_TYPE type1, RELATIONSHIP_TYPE type2)
	{
		if (GetFirstCharacterWithRelationship(type1, type2) != null)
		{
			return true;
		}
		return false;
	}

	public Character GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE type1, RELATIONSHIP_TYPE type2)
	{
		foreach (KeyValuePair<int, IRelationshipData> relationship in relationships)
		{
			if (relationship.Value.HasRelationship(type1, type2))
			{
				Character characterByID = CharacterManager.Instance.GetCharacterByID(relationship.Key);
				if (characterByID != null)
				{
					return characterByID;
				}
			}
		}
		return null;
	}

	public Character GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE type)
	{
		foreach (KeyValuePair<int, IRelationshipData> relationship in relationships)
		{
			if (relationship.Value.HasRelationship(type))
			{
				Character characterByID = CharacterManager.Instance.GetCharacterByID(relationship.Key);
				if (characterByID != null)
				{
					return characterByID;
				}
			}
		}
		return null;
	}

	public bool IsFamilyMemberOrLoverOrAffairAndNotRival(Character character)
	{
		if (IsFamilyMember(character) || IsLoverOrAffair(character))
		{
			return GetOpinionLabel(character) != "Rival";
		}
		return false;
	}

	public int GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE type)
	{
		foreach (KeyValuePair<int, IRelationshipData> relationship in relationships)
		{
			if (relationship.Value.HasRelationship(type))
			{
				return relationship.Key;
			}
		}
		return -1;
	}

	public int GetFirstAliveOrUnspawnedRelationshipID(RELATIONSHIP_TYPE type)
	{
		foreach (KeyValuePair<int, IRelationshipData> relationship in relationships)
		{
			if (relationship.Value.HasRelationship(type))
			{
				Character characterByID = CharacterManager.Instance.GetCharacterByID(relationship.Key);
				if (characterByID == null || !characterByID.isDead)
				{
					return relationship.Key;
				}
			}
		}
		return -1;
	}

	public Character GetFirstAliveCharacterWithRelationship(RELATIONSHIP_TYPE type)
	{
		foreach (KeyValuePair<int, IRelationshipData> relationship in relationships)
		{
			if (relationship.Value.HasRelationship(type))
			{
				Character characterByID = CharacterManager.Instance.GetCharacterByID(relationship.Key);
				if (characterByID != null && !characterByID.isDead)
				{
					return characterByID;
				}
			}
		}
		return null;
	}

	public void PopulateAllRelatableIDWithRelationship(List<int> ids, RELATIONSHIP_TYPE type)
	{
		foreach (KeyValuePair<int, IRelationshipData> relationship in relationships)
		{
			if (relationship.Value.HasRelationship(type))
			{
				ids.Add(relationship.Key);
			}
		}
	}

	public void PopulateAliveFamilyMembers(List<Character> p_characters)
	{
		foreach (KeyValuePair<int, IRelationshipData> relationship in relationships)
		{
			Character characterByID = CharacterManager.Instance.GetCharacterByID(relationship.Key);
			if (characterByID != null && IsFamilyMember(characterByID) && !characterByID.isDead)
			{
				p_characters.Add(characterByID);
			}
		}
	}

	public void PopulateAllCharactersWithRelationship(List<Character> p_characters, RELATIONSHIP_TYPE type)
	{
		foreach (KeyValuePair<int, IRelationshipData> relationship in relationships)
		{
			if (relationship.Value.HasRelationship(type))
			{
				Character characterByID = CharacterManager.Instance.GetCharacterByID(relationship.Key);
				if (characterByID != null)
				{
					p_characters.Add(characterByID);
				}
			}
		}
	}

	public void PopulateAliveCharactersWithRelationship(List<Character> p_characters, RELATIONSHIP_TYPE type)
	{
		foreach (KeyValuePair<int, IRelationshipData> relationship in relationships)
		{
			if (relationship.Value.HasRelationship(type))
			{
				Character characterByID = CharacterManager.Instance.GetCharacterByID(relationship.Key);
				if (characterByID != null && !characterByID.isDead)
				{
					p_characters.Add(characterByID);
				}
			}
		}
	}

	public int GetRelatablesWithRelationshipCount(RELATIONSHIP_TYPE type)
	{
		int num = 0;
		foreach (KeyValuePair<int, IRelationshipData> relationship in relationships)
		{
			if (relationship.Value.HasRelationship(type))
			{
				num++;
			}
		}
		return num;
	}

	public int GetRelatablesWithRelationshipCount(RELATIONSHIP_TYPE type1, RELATIONSHIP_TYPE type2)
	{
		int num = 0;
		foreach (KeyValuePair<int, IRelationshipData> relationship in relationships)
		{
			if (relationship.Value.HasRelationship(type1, type2))
			{
				num++;
			}
		}
		return num;
	}

	public int GetAliveOrUnspawnedRelatablesWithRelationshipCount(RELATIONSHIP_TYPE type1, RELATIONSHIP_TYPE type2)
	{
		int num = 0;
		foreach (KeyValuePair<int, IRelationshipData> relationship in relationships)
		{
			if (relationship.Value.HasRelationship(type1, type2))
			{
				Character characterByID = CharacterManager.Instance.GetCharacterByID(relationship.Key);
				if (characterByID == null || !characterByID.isDead)
				{
					num++;
				}
			}
		}
		return num;
	}

	public IRelationshipData GetRelationshipDataWith(Relatable relatable)
	{
		return GetRelationshipDataWith(relatable.id);
	}

	public IRelationshipData GetRelationshipDataWith(int id)
	{
		if (HasRelationshipWith(id))
		{
			return relationships[id];
		}
		return null;
	}

	public RELATIONSHIP_TYPE GetRelationshipFromParametersWith(Relatable relatable, RELATIONSHIP_TYPE relType1, RELATIONSHIP_TYPE relType2)
	{
		if (HasRelationshipWith(relatable))
		{
			IRelationshipData relationshipData = relationships[relatable.id];
			for (int i = 0; i < relationshipData.relationships.Count; i++)
			{
				RELATIONSHIP_TYPE rELATIONSHIP_TYPE = relationshipData.relationships[i];
				if (relType1 == rELATIONSHIP_TYPE || relType2 == rELATIONSHIP_TYPE)
				{
					return rELATIONSHIP_TYPE;
				}
			}
			return RELATIONSHIP_TYPE.NONE;
		}
		return RELATIONSHIP_TYPE.NONE;
	}

	public Character GetRandomMissingCharacterWithOpinion(Character p_source, string opinionLabel)
	{
		Character result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < charactersWithOpinion.Count; i++)
		{
			Character character = charactersWithOpinion[i];
			if (!character.isDead && !character.isInLimbo && GetAwarenessState(p_source, character) == AWARENESS_STATE.Missing && GetOpinionLabel(character) == opinionLabel)
			{
				list.Add(character);
			}
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public bool IsCharacterConsideredMissingOrPresumedDead(Character p_source, Character p_target)
	{
		AWARENESS_STATE awarenessState = GetAwarenessState(p_source, p_target);
		if (awarenessState != AWARENESS_STATE.Missing)
		{
			return awarenessState == AWARENESS_STATE.Presumed_Dead;
		}
		return true;
	}

	public Character GetRandomMissingCharacterThatIsFamilyMemberOrLoverAffairOf(Character p_source, Character p_character)
	{
		Character result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < charactersWithOpinion.Count; i++)
		{
			Character character = charactersWithOpinion[i];
			if (!character.isDead && !character.isInLimbo && GetAwarenessState(p_source, character) == AWARENESS_STATE.Missing && (p_character.relationshipContainer.IsFamilyMember(character) || p_character.relationshipContainer.IsLoverOrAffair(character)))
			{
				list.Add(character);
			}
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomAliveCharacterWithOpinion()
	{
		Character result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < charactersWithOpinion.Count; i++)
		{
			Character character = charactersWithOpinion[i];
			if (!character.isDead && !character.isInLimbo)
			{
				list.Add(character);
			}
		}
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomAliveNonLeaderCharacterWithOpinion(Character p_source)
	{
		Character result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < charactersWithOpinion.Count; i++)
		{
			Character character = charactersWithOpinion[i];
			if (!character.isDead && !character.isInLimbo && ((!character.isFactionLeader && !character.isSettlementRuler) || p_source.faction != character.faction))
			{
				list.Add(character);
			}
		}
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	private bool CanHaveOpinionsWithTargetCharacter(Character owner, Character target)
	{
		if (owner == target)
		{
			return false;
		}
		bool flag = false;
		if (target.minion != null || target is Summon)
		{
			if (target.raceSetting.category == CHARACTER_CATEGORY.Humanoid || target.raceSetting.category == CHARACTER_CATEGORY.Demonic)
			{
				flag = false;
			}
			else if (owner.HasAfflictedByPlayerWith(PLAYER_SKILL_TYPE.UNFAITHFULNESS) && PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.UNFAITHFULNESS).HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Wild_Multiple_Affair))
			{
				flag = false;
			}
			else if (!target.relationshipContainer.HasSpecialRelationshipWith(owner))
			{
				flag = true;
			}
		}
		if (flag)
		{
			return false;
		}
		return true;
	}

	public void AdjustOpinion(Character owner, Character target, string opinionText, int opinionValue, string lastStrawReason = "", bool createJobsOnReduce = true)
	{
		if (owner.hasBeenCleanedUp || target.hasBeenCleanedUp || !CanHaveOpinionsWithTargetCharacter(owner, target))
		{
			return;
		}
		IRelationshipData orCreateRelationshipDataWith = GetOrCreateRelationshipDataWith(owner, target);
		string opinionLabel = GetOpinionLabel(target);
		if (owner.traitContainer.HasTrait("Psychopath"))
		{
			owner.traitContainer.GetTraitOrStatus<Psychopath>("Psychopath").AdjustOpinion(target, opinionText, opinionValue);
			opinionValue = 0;
		}
		orCreateRelationshipDataWith.opinions.AdjustOpinion(opinionText, opinionValue);
		if (opinionValue > 0)
		{
			Messenger.Broadcast(CharacterSignals.OPINION_INCREASED, owner, target, lastStrawReason);
		}
		else if (opinionValue < 0)
		{
			if (createJobsOnReduce && !owner.reactionComponent.isDisguised && !target.reactionComponent.isDisguised)
			{
				CreateJobsOnOpinionReducedBase(owner, target, lastStrawReason, opinionValue);
			}
			Messenger.Broadcast(CharacterSignals.OPINION_DECREASED, owner, target, lastStrawReason);
		}
		string opinionLabel2 = GetOpinionLabel(target);
		if (opinionLabel != opinionLabel2 && opinionValue < 0)
		{
			Messenger.Broadcast(CharacterSignals.OPINION_LABEL_DECREASED, owner, target, opinionLabel2);
		}
		if (!target.relationshipContainer.HasRelationshipWith(owner))
		{
			target.relationshipContainer.CreateNewRelationship(target, owner);
		}
	}

	public void SetOpinion(Character owner, Character target, string opinionText, int opinionValue, string lastStrawReason = "", bool isInitial = false)
	{
		if (owner.hasBeenCleanedUp || target.hasBeenCleanedUp || !CanHaveOpinionsWithTargetCharacter(owner, target))
		{
			return;
		}
		IRelationshipData orCreateRelationshipDataWith = GetOrCreateRelationshipDataWith(owner, target);
		string opinionLabel = GetOpinionLabel(target);
		if (owner.traitContainer.HasTrait("Psychopath"))
		{
			opinionValue = 0;
		}
		orCreateRelationshipDataWith.opinions.SetOpinion(opinionText, opinionValue);
		if (opinionValue > 0)
		{
			Messenger.Broadcast(CharacterSignals.OPINION_INCREASED, owner, target, lastStrawReason);
		}
		else if (opinionValue < 0)
		{
			if (!isInitial)
			{
				CreateJobsOnOpinionReducedBase(owner, target, lastStrawReason, opinionValue);
			}
			Messenger.Broadcast(CharacterSignals.OPINION_DECREASED, owner, target, lastStrawReason);
		}
		string opinionLabel2 = GetOpinionLabel(target);
		if (opinionLabel != opinionLabel2 && opinionValue < 0)
		{
			Messenger.Broadcast(CharacterSignals.OPINION_LABEL_DECREASED, owner, target, opinionLabel2);
		}
		if (!target.relationshipContainer.HasRelationshipWith(owner))
		{
			target.relationshipContainer.CreateNewRelationship(target, owner);
		}
	}

	public void SetOpinion(Character owner, int targetID, string targetName, GENDER gender, string opinionText, int opinionValue, bool isInitial, string lastStrawReason = "")
	{
		if (owner.hasBeenCleanedUp || owner.minion != null || owner is Summon)
		{
			return;
		}
		Character characterByID = CharacterManager.Instance.GetCharacterByID(targetID);
		if (characterByID != null)
		{
			SetOpinion(owner, characterByID, opinionText, opinionValue, lastStrawReason, isInitial);
			return;
		}
		IRelationshipData orCreateRelationshipDataWith = GetOrCreateRelationshipDataWith(owner, targetID, targetName, gender);
		if (owner.traitContainer.HasTrait("Psychopath"))
		{
			opinionValue = 0;
		}
		orCreateRelationshipDataWith.opinions.SetOpinion(opinionText, opinionValue);
		if (!isInitial)
		{
			if (opinionValue > 0)
			{
				Messenger.Broadcast<Character, Character, string>(CharacterSignals.OPINION_INCREASED, owner, null, lastStrawReason);
			}
			else if (opinionValue < 0)
			{
				CreateJobsOnOpinionReducedBase(owner, characterByID, lastStrawReason, opinionValue);
				Messenger.Broadcast<Character, Character, string>(CharacterSignals.OPINION_DECREASED, owner, null, lastStrawReason);
			}
		}
	}

	public void RemoveOpinion(Character target, string opinionText)
	{
		if (TryGetRelationshipDataWith(target, out var data))
		{
			data.opinions.RemoveOpinion(opinionText);
		}
	}

	public void AddSharedOpinionModifier(Character owner, Character target, SharedOpinionModifier p_modifier)
	{
		if (owner != target)
		{
			GetOrCreateRelationshipDataWith(owner, target).opinions.AddSharedOpinion(p_modifier);
		}
	}

	private float ModifyChanceBasedOnMood(MOOD_STATE p_mood, float p_baseChance)
	{
		float num = p_baseChance;
		return Mathf.Clamp(p_mood switch
		{
			MOOD_STATE.Normal => num * 0.5f, 
			MOOD_STATE.Bad => num * 1f, 
			MOOD_STATE.Critical => num * 2f, 
			_ => throw new ArgumentOutOfRangeException("p_mood", p_mood, null), 
		}, 0f, 100f);
	}

	public void CreateJobsOnOpinionReduced(Character owner, Character targetCharacter, string reason, int amountReduced)
	{
		CreateJobsOnOpinionReducedBase(owner, targetCharacter, reason, amountReduced);
	}

	private void CreateJobsOnOpinionReducedBase(Character owner, Character targetCharacter, string reason, int amountReduced)
	{
		if (!GameManager.Instance.gameHasStarted || !owner.limiterComponent.canPerform)
		{
			return;
		}
		int num = Mathf.Abs(amountReduced);
		if (num >= 50)
		{
			OpinionReductionValueIs50OrAbove(owner, targetCharacter, reason);
		}
		else if (num >= 30)
		{
			OpinionReductionValueIs30OrAbove(owner, targetCharacter, reason);
		}
		else if (num >= 15)
		{
			OpinionReductionValueIs15OrAbove(owner, targetCharacter, reason);
		}
		else if (num > 10)
		{
			TryBreakUp(owner, targetCharacter, reason, 30f);
			if (owner.hasMarker && owner.marker.IsPOIInVision(targetCharacter))
			{
				owner.interruptComponent.TriggerInterrupt(INTERRUPT.Angry_Stare, targetCharacter);
			}
			else
			{
				owner.interruptComponent.TriggerInterrupt(INTERRUPT.Cursing, owner);
			}
		}
	}

	public bool HasOpinion(Character target, string opinionText)
	{
		if (TryGetRelationshipDataWith(target, out var data))
		{
			return data.opinions.HasOpinion(opinionText);
		}
		return false;
	}

	public bool HasOpinion(int id, string opinionText)
	{
		if (TryGetRelationshipDataWith(id, out var data))
		{
			return data.opinions.HasOpinion(opinionText);
		}
		return false;
	}

	public bool IsKnown(Character p_character)
	{
		return charactersWithOpinion.Contains(p_character);
	}

	public int GetTotalOpinion(Character target)
	{
		return GetTotalOpinion(target.id);
	}

	public int GetTotalOpinion(int id)
	{
		if (HasRelationshipWith(id))
		{
			return relationships[id].opinions?.totalOpinion ?? 0;
		}
		return 0;
	}

	public OpinionData GetOpinionData(Character target)
	{
		return GetOpinionData(target.id);
	}

	public OpinionData GetOpinionData(int id)
	{
		if (HasRelationshipWith(id))
		{
			return relationships[id].opinions;
		}
		return null;
	}

	public string GetOpinionLabel(Character target)
	{
		return GetOpinionLabel(target.id);
	}

	public string GetOpinionLabel(int id)
	{
		if (HasRelationshipWith(id))
		{
			return relationships[id].opinions.GetOpinionLabel();
		}
		return string.Empty;
	}

	public static string OpinionColor(int number)
	{
		if (number > 20)
		{
			return "green";
		}
		if (number > -21 && number <= 20)
		{
			return "#808080";
		}
		return "red";
	}

	public static string OpinionColorNoGray(int number)
	{
		if (number >= 0)
		{
			return "green";
		}
		return "red";
	}

	public bool IsFriendsWith(Character character)
	{
		string opinionLabel = GetOpinionLabel(character);
		if (!(opinionLabel == "Friend"))
		{
			return opinionLabel == "Close Friend";
		}
		return true;
	}

	public bool IsFriendsOrAcquaintancesWith(Character character)
	{
		string opinionLabel = GetOpinionLabel(character);
		if (!(opinionLabel == "Friend") && !(opinionLabel == "Close Friend"))
		{
			return opinionLabel == "Acquaintance";
		}
		return true;
	}

	public bool IsEnemiesWith(Character character)
	{
		string opinionLabel = GetOpinionLabel(character);
		if (!(opinionLabel == "Enemy"))
		{
			return opinionLabel == "Rival";
		}
		return true;
	}

	public Character GetFirstAliveEnemyCharacter()
	{
		for (int i = 0; i < charactersWithOpinion.Count; i++)
		{
			Character character = charactersWithOpinion[i];
			if (IsEnemiesWith(character) && !character.isInLimbo && !character.isDead)
			{
				return character;
			}
		}
		return null;
	}

	public void PopulateAliveEnemyCharacters(List<Character> characters)
	{
		for (int i = 0; i < charactersWithOpinion.Count; i++)
		{
			Character character = charactersWithOpinion[i];
			if (IsEnemiesWith(character) && !character.isInLimbo && !character.isDead)
			{
				characters.Add(character);
			}
		}
	}

	public Character GetRandomAliveEnemyCharacter()
	{
		Character result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < charactersWithOpinion.Count; i++)
		{
			Character character = charactersWithOpinion[i];
			if (IsEnemiesWith(character) && !character.isInLimbo && !character.isDead)
			{
				list.Add(character);
			}
		}
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomAliveEnemyCharacterThatIsNot(Character p_exclusion)
	{
		Character result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < charactersWithOpinion.Count; i++)
		{
			Character character = charactersWithOpinion[i];
			if (character != p_exclusion && !character.isInLimbo && IsEnemiesWith(character) && !character.isDead)
			{
				list.Add(character);
			}
		}
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetFirstAliveCharacterWithLowestOpinionInsideSettlement(Faction p_faction, BaseSettlement p_homeSettlement)
	{
		Character character = null;
		int num = 0;
		for (int i = 0; i < charactersWithOpinion.Count; i++)
		{
			Character character2 = charactersWithOpinion[i];
			if (!character2.isDead && character2.faction == p_faction && character2.homeSettlement == p_homeSettlement && character2.IsInHomeSettlement())
			{
				int totalOpinion = GetTotalOpinion(character2);
				if (character == null || totalOpinion < num)
				{
					character = character2;
					num = totalOpinion;
				}
			}
		}
		return character;
	}

	public void PopulateAliveFriendCharacters(List<Character> characters)
	{
		for (int i = 0; i < charactersWithOpinion.Count; i++)
		{
			Character character = charactersWithOpinion[i];
			if (IsFriendsWith(character) && !character.isInLimbo && !character.isDead)
			{
				characters.Add(character);
			}
		}
	}

	public bool HasOpinionLabelWithCharacter(Character character, string opinion)
	{
		if (HasRelationshipWith(character))
		{
			string opinionLabel = GetOpinionLabel(character);
			if (opinion == opinionLabel)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasOpinionLabelWithCharacter(Character character, string opinion1, string opinion2)
	{
		if (HasRelationshipWith(character))
		{
			string opinionLabel = GetOpinionLabel(character);
			if (opinion1 == opinionLabel || opinion2 == opinionLabel)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasOpinionLabelWithCharacter(Character character, string opinion1, string opinion2, string opinion3)
	{
		if (HasRelationshipWith(character))
		{
			string opinionLabel = GetOpinionLabel(character);
			if (opinion1 == opinionLabel || opinion2 == opinionLabel || opinion3 == opinionLabel)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasOpinionLabelWithCharacter(Character character, List<OPINIONS> opinions)
	{
		if (HasRelationshipWith(character))
		{
			string opinionLabel = GetOpinionLabel(character);
			for (int i = 0; i < opinions.Count; i++)
			{
				if (opinions[i].GetOpinionLabel() == opinionLabel)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasAliveEnemyCharacterThatIsNot(Character p_exclusion)
	{
		for (int i = 0; i < charactersWithOpinion.Count; i++)
		{
			Character character = charactersWithOpinion[i];
			if (character != p_exclusion && IsEnemiesWith(character) && !character.isDead)
			{
				return true;
			}
		}
		return false;
	}

	public int GetNumberOfFriendCharacters()
	{
		int num = 0;
		for (int i = 0; i < charactersWithOpinion.Count; i++)
		{
			Character character = charactersWithOpinion[i];
			if (IsFriendsWith(character))
			{
				num++;
			}
		}
		return num;
	}

	public RELATIONSHIP_EFFECT GetRelationshipEffectWith(Character character)
	{
		if (HasRelationshipWith(character))
		{
			int totalOpinion = GetTotalOpinion(character);
			if (totalOpinion > 0)
			{
				return RELATIONSHIP_EFFECT.POSITIVE;
			}
			if (totalOpinion < 0)
			{
				return RELATIONSHIP_EFFECT.NEGATIVE;
			}
		}
		return RELATIONSHIP_EFFECT.NONE;
	}

	public int GetCompatibility(Character target)
	{
		return GetCompatibility(target.id);
	}

	public int GetCompatibility(int targetID)
	{
		if (TryGetRelationshipDataWith(targetID, out var data))
		{
			return data.opinions.compatibilityValue;
		}
		return -1;
	}

	public bool HasSpecialPositiveRelationshipWith(Character characterThatDied)
	{
		if (TryGetRelationshipDataWith(characterThatDied.id, out var data))
		{
			RELATIONSHIP_TYPE firstMajorRelationship = data.GetFirstMajorRelationship();
			if ((uint)(firstMajorRelationship - 4) <= 1u || (uint)(firstMajorRelationship - 11) <= 2u)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public string GetLocalizedRelationshipNameWith(Character target)
	{
		return GetLocalizedRelationshipNameWith(target.id);
	}

	public string GetLocalizedRelationshipNameWith(int id)
	{
		string text = string.Empty;
		if (TryGetRelationshipDataWith(id, out var data))
		{
			RELATIONSHIP_TYPE firstMajorRelationship = data.GetFirstMajorRelationship();
			switch (firstMajorRelationship)
			{
			case RELATIONSHIP_TYPE.CHILD:
				text = ((data.targetGender == GENDER.MALE) ? "Son" : "Daughter");
				break;
			case RELATIONSHIP_TYPE.PARENT:
				text = ((data.targetGender == GENDER.MALE) ? "Father" : "Mother");
				break;
			case RELATIONSHIP_TYPE.SIBLING:
				text = ((data.targetGender == GENDER.MALE) ? "Brother" : "Sister");
				break;
			case RELATIONSHIP_TYPE.LOVER:
				text = ((data.targetGender == GENDER.MALE) ? "Husband" : "Wife");
				break;
			case RELATIONSHIP_TYPE.NONE:
			{
				string opinionLabel = GetOpinionLabel(id);
				text = ((!string.IsNullOrEmpty(opinionLabel)) ? opinionLabel : "Acquaintance");
				break;
			}
			default:
				text = firstMajorRelationship.ToStringEnumWithSpaceNormalizedUppercaseFirstLetterOnly();
				break;
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			text = "Acquaintance";
		}
		return LocalizationManager.Instance.GetLocalizedValue("Relationships_Table", text);
	}

	public AWARENESS_STATE GetAwarenessState(Character p_source, Character p_target)
	{
		if (p_source.faction != null)
		{
			return p_source.faction.charactersComponent.GetAwarenessState(p_target);
		}
		return AWARENESS_STATE.Available;
	}

	private bool TryBreakUp(Character owner, Character targetCharacter, string reason, float baseChance)
	{
		if (GameUtilities.RollChance(ModifyChanceBasedOnMood(owner.moodComponent.moodState, baseChance)) && IsLoverOrAffair(targetCharacter) && owner.relationshipContainer.IsEnemiesWith(targetCharacter))
		{
			owner.interruptComponent.TriggerInterrupt(INTERRUPT.Break_Up, targetCharacter, "", null, reason);
			return true;
		}
		return false;
	}

	public bool TryBreakUp(Character owner, Character targetCharacter, string reason)
	{
		if (IsLoverOrAffair(targetCharacter) && owner.relationshipContainer.IsEnemiesWith(targetCharacter))
		{
			owner.interruptComponent.TriggerInterrupt(INTERRUPT.Break_Up, targetCharacter, "", null, reason);
			return true;
		}
		return false;
	}

	private bool TryDestroyNearbyObject(Character owner)
	{
		if (owner.limiterComponent.canPerform && owner.limiterComponent.canMove && !owner.isDead && owner.hasMarker && owner.marker.inVisionTileObjects.Count > 0 && !owner.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.DESTROY) && !owner.jobQueue.HasJob(JOB_TYPE.DESTROY))
		{
			List<TileObject> list = RuinarchListPool<TileObject>.Claim();
			for (int i = 0; i < owner.marker.inVisionTileObjects.Count; i++)
			{
				TileObject tileObject = owner.marker.inVisionTileObjects[i];
				if (tileObject.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT && tileObject.tileObjectType != TILE_OBJECT_TYPE.STRUCTURE_TILE_OBJECT && tileObject.gridTileLocation.GetFirstNeighborThatIsPassable() != null)
				{
					list.Add(tileObject);
				}
			}
			if (list.Count > 0)
			{
				TileObject randomElement = CollectionUtilities.GetRandomElement(list);
				RuinarchListPool<TileObject>.Release(list);
				return owner.jobComponent.TriggerDestroy(randomElement, "Destroy_Angry");
			}
			RuinarchListPool<TileObject>.Release(list);
		}
		return false;
	}

	private TileObject GetRandomArsonTarget(Character arson, Character targetCharacter)
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		targetCharacter.homeStructure.PopulateBuiltTileObjects(list);
		List<TileObject> list2 = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < list.Count; i++)
		{
			TileObject tileObject = list[i];
			if (tileObject != null && !tileObject.traitContainer.HasTrait("Burning", "Fire Resistant") && tileObject.traitContainer.HasTrait("Flammable"))
			{
				list2.Add(tileObject);
			}
		}
		TileObject result = null;
		if (list2.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list2);
		}
		RuinarchListPool<TileObject>.Release(list2);
		return result;
	}

	public bool SetHasGrudgeAgainst(Character p_actor, Character p_target, bool p_state)
	{
		IRelationshipData relationshipDataWith = GetRelationshipDataWith(p_target);
		if (relationshipDataWith != null && relationshipDataWith.hasGrudge != p_state)
		{
			relationshipDataWith.SetHasGrudge(p_state);
			if (relationshipDataWith.hasGrudge)
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "add_grudge", LOG_TAG.Social);
				log.AddToFillers(p_actor, p_actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(p_target, p_target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log.AddLogToDatabase();
				PlayerManager.Instance.player.ShowNotificationFrom(p_actor, log, releaseLogAfter: true);
			}
			return true;
		}
		return false;
	}

	public void OnOwnerDied(Character p_owner)
	{
		Character firstAliveCharacterWithRelationship = GetFirstAliveCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
		Character firstAliveCharacterWithRelationship2 = GetFirstAliveCharacterWithRelationship(RELATIONSHIP_TYPE.AFFAIR);
		if (firstAliveCharacterWithRelationship != null)
		{
			string text = string.Empty;
			if (firstAliveCharacterWithRelationship.relationshipContainer.IsFriendsOrAcquaintancesWith(p_owner))
			{
				text = "Griefstricken";
			}
			else if (firstAliveCharacterWithRelationship.relationshipContainer.GetOpinionLabel(p_owner) == "Rival")
			{
				text = "Catharsis";
			}
			if (!string.IsNullOrEmpty(text))
			{
				firstAliveCharacterWithRelationship.traitContainer.AddTrait(firstAliveCharacterWithRelationship, text, p_owner);
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "Lover_Affair_Died", LOG_TAG.Life_Changes, LOG_TAG.Social);
				log.AddToFillers(firstAliveCharacterWithRelationship, firstAliveCharacterWithRelationship.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(p_owner, p_owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log.AddToFillers(null, LocalizationManager.Instance.GetLocalizedValue("Traits_Table", text), LOG_IDENTIFIER.STRING_1);
				log.AddLogToDatabase();
			}
		}
		List<Character> list = RuinarchListPool<Character>.Claim();
		PopulateAliveFamilyMembers(list);
		if (firstAliveCharacterWithRelationship2 != null)
		{
			list.Add(firstAliveCharacterWithRelationship2);
		}
		for (int i = 0; i < list.Count; i++)
		{
			Character character = list[i];
			string text2 = string.Empty;
			if (character.relationshipContainer.IsFriendsOrAcquaintancesWith(p_owner))
			{
				text2 = "Heartbroken";
			}
			else if (character.relationshipContainer.GetOpinionLabel(p_owner) == "Rival")
			{
				text2 = "Cheery";
			}
			if (!string.IsNullOrEmpty(text2))
			{
				character.traitContainer.AddTrait(character, text2, p_owner);
				Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "Lover_Affair_Died", LOG_TAG.Life_Changes, LOG_TAG.Social);
				log2.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log2.AddToFillers(p_owner, p_owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log2.AddToFillers(null, LocalizationManager.Instance.GetLocalizedValue("Traits_Table", text2), LOG_IDENTIFIER.STRING_1);
				log2.AddLogToDatabase();
			}
		}
		RuinarchListPool<Character>.Release(list);
	}

	private void OnCharacterChangedName(Character character)
	{
		if (HasRelationshipWith(character))
		{
			GetRelationshipDataWith(character).SetTargetName(character.name);
		}
	}

	private void OpinionReductionValueIs50OrAbove(Character owner, Character targetCharacter, string reason)
	{
		TryBreakUp(owner, targetCharacter, reason, 90f);
		if (owner.hasMarker && owner.marker.IsPOIInVision(targetCharacter))
		{
			if (!CreateInVisionJob(owner, targetCharacter, 35, 35, 35, 0))
			{
				if (GameUtilities.RollChance(ModifyChanceBasedOnMood(owner.moodComponent.moodState, 30f)))
				{
					owner.interruptComponent.TriggerInterrupt(INTERRUPT.Cursing, owner);
				}
				else
				{
					TryDestroyNearbyObject(owner);
				}
			}
		}
		else if (((!owner.traitContainer.HasTrait("Evil") && !owner.traitContainer.HasTrait("Treacherous") && !owner.traitContainer.HasTrait("Psychopath") && (!owner.traitContainer.HasTrait("Betrayed") || !owner.traitContainer.GetTraitOrStatus<Trait>("Betrayed").IsResponsibleForTrait(targetCharacter))) || !CreatePoisonFoodOrPlaceTrapJob(owner, targetCharacter, 70, 70)) && !CreateArsonJob(owner, targetCharacter, 100) && !CreateShareNegativeOrSpreadRumorJob(owner, targetCharacter, 80, 30) && !CreateDrinkJob(owner, 40))
		{
			owner.interruptComponent.TriggerInterrupt(INTERRUPT.Tantrum, owner);
		}
	}

	private void OpinionReductionValueIs30OrAbove(Character owner, Character targetCharacter, string reason)
	{
		TryBreakUp(owner, targetCharacter, reason, 50f);
		if (owner.hasMarker && owner.marker.IsPOIInVision(targetCharacter))
		{
			CreateInVisionJob(owner, targetCharacter, 25, 25, 10, 100);
		}
		else if (((!owner.traitContainer.HasTrait("Evil") && !owner.traitContainer.HasTrait("Treacherous") && !owner.traitContainer.HasTrait("Psychopath") && (!owner.traitContainer.HasTrait("Betrayed") || !owner.traitContainer.GetTraitOrStatus<Trait>("Betrayed").IsResponsibleForTrait(targetCharacter))) || !CreatePoisonFoodOrPlaceTrapJob(owner, targetCharacter, 50, 50)) && !CreateArsonJob(owner, targetCharacter, 50) && !CreateShareNegativeOrSpreadRumorJob(owner, targetCharacter, 60, 30) && !CreateDrinkJob(owner, 25))
		{
			owner.interruptComponent.TriggerInterrupt(INTERRUPT.Cursing, owner);
		}
	}

	private void OpinionReductionValueIs15OrAbove(Character owner, Character targetCharacter, string reason)
	{
		TryBreakUp(owner, targetCharacter, reason, 50f);
		if (owner.hasMarker && owner.marker.IsPOIInVision(targetCharacter))
		{
			CreateInVisionJob(owner, targetCharacter, 20, 20, 0, 100);
		}
		else if (((!owner.traitContainer.HasTrait("Evil") && !owner.traitContainer.HasTrait("Treacherous") && !owner.traitContainer.HasTrait("Psychopath") && (!owner.traitContainer.HasTrait("Betrayed") || !owner.traitContainer.GetTraitOrStatus<Trait>("Betrayed").IsResponsibleForTrait(targetCharacter))) || !CreatePoisonFoodOrPlaceTrapJob(owner, targetCharacter, 30, 30)) && !CreateArsonJob(owner, targetCharacter, 30) && !CreateShareNegativeOrSpreadRumorJob(owner, targetCharacter, 40, 30) && !CreateDrinkJob(owner, 25))
		{
			owner.interruptComponent.TriggerInterrupt(INTERRUPT.Cursing, owner);
		}
	}

	private bool CreateInVisionJob(Character p_actor, Character p_targetCharacter, int p_brawlChance, int p_cryChance, int p_berserkChance, int p_angryStareChance)
	{
		if (GameUtilities.RollChance(ModifyChanceBasedOnMood(p_actor.moodComponent.moodState, p_brawlChance)) && (p_targetCharacter.combatComponent.isInCombat || p_actor.traitContainer.HasTrait("Combatant")) && p_actor.jobComponent.CreateBrawlJob(p_targetCharacter))
		{
			return true;
		}
		if (GameUtilities.RollChance(ModifyChanceBasedOnMood(p_actor.moodComponent.moodState, p_cryChance)))
		{
			p_actor.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, p_targetCharacter, "", null, "Cry_Deteriorating_Relationship");
			return true;
		}
		if (GameUtilities.RollChance(ModifyChanceBasedOnMood(p_actor.moodComponent.moodState, p_berserkChance)))
		{
			p_actor.traitContainer.AddTrait(p_actor, "Berserked");
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Trait", "Traits_Table", "Berserked opinion_reduced", LOG_TAG.Life_Changes);
			log.AddToFillers(p_actor, p_actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(p_targetCharacter, p_targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddLogToDatabase(releaseLogAfter: true);
			return true;
		}
		if (GameUtilities.RollChance(ModifyChanceBasedOnMood(p_actor.moodComponent.moodState, p_angryStareChance)))
		{
			p_actor.interruptComponent.TriggerInterrupt(INTERRUPT.Angry_Stare, p_targetCharacter);
			return true;
		}
		return false;
	}

	private bool CreateDrinkJob(Character p_actor, int p_chance)
	{
		if (GameUtilities.RollChance(ModifyChanceBasedOnMood(p_actor.moodComponent.moodState, p_chance)) && p_actor.currentSettlement != null)
		{
			LocationStructure randomStructureOfType = p_actor.currentSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.TAVERN);
			if (randomStructureOfType != null)
			{
				List<TileObject> list = RuinarchListPool<TileObject>.Claim();
				randomStructureOfType.PopulateTileObjectsThatAdvertise(list, INTERACTION_TYPE.DRINK);
				TileObject tileObject = null;
				if (list.Count > 0)
				{
					tileObject = CollectionUtilities.GetRandomElement(list);
				}
				RuinarchListPool<TileObject>.Release(list);
				if (tileObject != null)
				{
					GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.OPINION_REDUCTION_REACTION, INTERACTION_TYPE.DRINK, tileObject, p_actor);
					return p_actor.jobQueue.AddJobInQueue(job);
				}
			}
		}
		return false;
	}

	private bool CreateShareNegativeOrSpreadRumorJob(Character p_actor, Character p_targetCharacter, int p_shareNegativeChance, int p_spreadRumorChance)
	{
		Character randomSpreadRumorOrNegativeInfoTarget = p_actor.rumorComponent.GetRandomSpreadRumorOrNegativeInfoTarget(p_targetCharacter);
		if (randomSpreadRumorOrNegativeInfoTarget != null)
		{
			if (GameUtilities.RollChance(ModifyChanceBasedOnMood(p_actor.moodComponent.moodState, p_shareNegativeChance)))
			{
				ActualGoapNode randomKnownNegativeInfo = p_actor.rumorComponent.GetRandomKnownNegativeInfo(randomSpreadRumorOrNegativeInfoTarget, p_targetCharacter);
				if (randomKnownNegativeInfo != null && p_actor.jobComponent.CreateSpreadNegativeInfoJob(JOB_TYPE.SHARE_NEGATIVE_INFO, randomSpreadRumorOrNegativeInfoTarget, randomKnownNegativeInfo))
				{
					return true;
				}
			}
			if (GameUtilities.RollChance(ModifyChanceBasedOnMood(p_actor.moodComponent.moodState, p_spreadRumorChance)))
			{
				Rumor rumor = p_actor.rumorComponent.GenerateNewRandomRumor(randomSpreadRumorOrNegativeInfoTarget, p_targetCharacter);
				if (rumor != null && p_actor.jobComponent.CreateSpreadRumorJob(randomSpreadRumorOrNegativeInfoTarget, rumor))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool CreatePoisonFoodOrPlaceTrapJob(Character p_actor, Character p_targetCharacter, int p_poisonFoodChance, int p_placeTrapChance)
	{
		if (GameUtilities.RollChance(ModifyChanceBasedOnMood(p_actor.moodComponent.moodState, p_poisonFoodChance)) && p_actor.jobComponent.CreatePoisonFoodJob(p_targetCharacter))
		{
			return true;
		}
		if (GameUtilities.RollChance(ModifyChanceBasedOnMood(p_actor.moodComponent.moodState, p_placeTrapChance)))
		{
			if (p_targetCharacter.HasOwnedItemInHomeStructure() && p_actor.jobComponent.CreatePlaceTrapOnOwnedHomeItemJob(p_targetCharacter))
			{
				return true;
			}
			if (p_actor.jobComponent.CreatePlaceTrapOnAnyHomeItemJob(p_targetCharacter))
			{
				return true;
			}
		}
		return false;
	}

	private bool CreateArsonJob(Character p_actor, Character p_targetCharacter, int p_chance)
	{
		if (p_actor.traitContainer.HasTrait("Pyromaniac") && p_targetCharacter.homeStructure != null && p_targetCharacter.homeStructure != p_actor.homeStructure && GameUtilities.RollChance(ModifyChanceBasedOnMood(p_actor.moodComponent.moodState, p_chance)))
		{
			TileObject randomArsonTarget = GetRandomArsonTarget(p_actor, p_targetCharacter);
			if (randomArsonTarget != null && p_actor.jobComponent.TriggerArson(randomArsonTarget) != null)
			{
				return true;
			}
		}
		return false;
	}

	public void DisconnectFromCharacter(Character p_character)
	{
		charactersWithOpinion?.Remove(p_character);
	}

	public void CleanUp()
	{
		charactersWithOpinion?.Clear();
		charactersWithOpinion = null;
		relationships?.Clear();
		relationships = null;
		Messenger.RemoveListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnNewVillagerArrived);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		charactersWithOpinion.Contains(p_character);
	}
}
