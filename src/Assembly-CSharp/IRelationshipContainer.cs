using System.Collections.Generic;
using Locations.Settlements;

public interface IRelationshipContainer
{
	Dictionary<int, IRelationshipData> relationships { get; }

	List<Character> charactersWithOpinion { get; }

	void AddRelationship(Relatable owner, Relatable relatable, RELATIONSHIP_TYPE rel);

	IRelationshipData CreateNewRelationship(Relatable owner, Relatable target);

	IRelationshipData CreateNewRelationship(Relatable owner, int id, string name, GENDER gender);

	void RemoveRelationship(Relatable relatable, RELATIONSHIP_TYPE rel);

	bool HasRelationshipWith(int id);

	bool HasRelationshipWith(Relatable relatable);

	bool HasRelationshipWith(Relatable relatable, RELATIONSHIP_TYPE relType);

	bool HasRelationshipWith(Relatable relatable, RELATIONSHIP_TYPE relType1, RELATIONSHIP_TYPE relType2);

	bool HasRelationshipWith(Relatable relatable, RELATIONSHIP_TYPE relType1, RELATIONSHIP_TYPE relType2, RELATIONSHIP_TYPE relType3);

	bool HasRelationshipWith(Relatable relatable, RELATIONSHIP_TYPE relType1, RELATIONSHIP_TYPE relType2, RELATIONSHIP_TYPE relType3, RELATIONSHIP_TYPE relType4, RELATIONSHIP_TYPE relType5);

	bool HasSpecialRelationshipWith(Relatable relatable);

	bool HasRelationship(RELATIONSHIP_TYPE type);

	bool HasAliveOrUnspawnedRelationship(RELATIONSHIP_TYPE type);

	bool HasRelationshipWithAliveCharacter(RELATIONSHIP_TYPE type);

	bool HasRelationshipWithSpawnedCharacter(RELATIONSHIP_TYPE type1, RELATIONSHIP_TYPE type2);

	bool IsKnown(Character p_character);

	void PopulateAllRelatableIDWithRelationship(List<int> ids, RELATIONSHIP_TYPE type);

	void PopulateAliveFamilyMembers(List<Character> p_characters);

	void PopulateAllCharactersWithRelationship(List<Character> p_characters, RELATIONSHIP_TYPE type);

	void PopulateAliveCharactersWithRelationship(List<Character> p_characters, RELATIONSHIP_TYPE type);

	int GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE type);

	int GetRelatablesWithRelationshipCount(RELATIONSHIP_TYPE type);

	int GetRelatablesWithRelationshipCount(RELATIONSHIP_TYPE type1, RELATIONSHIP_TYPE type2);

	int GetFirstAliveOrUnspawnedRelationshipID(RELATIONSHIP_TYPE type);

	int GetAliveOrUnspawnedRelatablesWithRelationshipCount(RELATIONSHIP_TYPE type1, RELATIONSHIP_TYPE type2);

	Character GetFirstAliveCharacterWithRelationship(RELATIONSHIP_TYPE type);

	IRelationshipData GetRelationshipDataWith(Relatable relatable);

	RELATIONSHIP_TYPE GetRelationshipFromParametersWith(Relatable relatable, RELATIONSHIP_TYPE relType1, RELATIONSHIP_TYPE relType2);

	bool IsFamilyMember(Character target);

	bool IsLoverOrAffair(Character target);

	bool IsFamilyMemberOrLoverOrAffairAndNotRival(Character character);

	Character GetRandomMissingCharacterWithOpinion(Character p_source, string opinionLabel);

	Character GetRandomMissingCharacterThatIsFamilyMemberOrLoverAffairOf(Character p_source, Character p_character);

	Character GetRandomAliveCharacterWithOpinion();

	Character GetRandomAliveNonLeaderCharacterWithOpinion(Character p_source);

	Character GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE type1, RELATIONSHIP_TYPE type2);

	Character GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE type);

	void AdjustOpinion(Character owner, Character target, string opinionText, int opinionValue, string lastStrawReason = "", bool createJobsOnReduce = true);

	void SetOpinion(Character owner, Character target, string opinionText, int opinionValue, string lastStrawReason = "", bool isInitial = false);

	void SetOpinion(Character owner, int targetID, string targetName, GENDER gender, string opinionText, int opinionValue, bool isInitial, string lastStrawReason = "");

	void RemoveOpinion(Character target, string opinionText);

	void AddSharedOpinionModifier(Character owner, Character target, SharedOpinionModifier p_modifier);

	void CreateJobsOnOpinionReduced(Character owner, Character targetCharacter, string reason, int amountReduced);

	void PopulateAliveEnemyCharacters(List<Character> characters);

	void PopulateAliveFriendCharacters(List<Character> characters);

	bool HasOpinion(Character target, string opinionText);

	bool HasOpinion(int id, string opinionText);

	int GetTotalOpinion(Character target);

	OpinionData GetOpinionData(Character target);

	string GetOpinionLabel(Character target);

	bool IsFriendsWith(Character character);

	bool IsFriendsOrAcquaintancesWith(Character character);

	bool IsEnemiesWith(Character character);

	Character GetFirstAliveEnemyCharacter();

	Character GetRandomAliveEnemyCharacter();

	Character GetRandomAliveEnemyCharacterThatIsNot(Character p_exclusion);

	Character GetFirstAliveCharacterWithLowestOpinionInsideSettlement(Faction p_faction, BaseSettlement p_homeSettlement);

	bool HasOpinionLabelWithCharacter(Character character, string opinion);

	bool HasOpinionLabelWithCharacter(Character character, string opinion1, string opinion2);

	bool HasOpinionLabelWithCharacter(Character character, string opinion1, string opinion2, string opinion3);

	bool HasOpinionLabelWithCharacter(Character character, List<OPINIONS> opinions);

	bool HasAliveEnemyCharacterThatIsNot(Character p_exclusion);

	int GetNumberOfFriendCharacters();

	RELATIONSHIP_EFFECT GetRelationshipEffectWith(Character character);

	int GetCompatibility(Character target);

	string GetLocalizedRelationshipNameWith(Character target);

	string GetLocalizedRelationshipNameWith(int target);

	AWARENESS_STATE GetAwarenessState(Character p_source, Character p_target);

	bool IsCharacterConsideredMissingOrPresumedDead(Character p_source, Character p_target);

	OpinionData GetOpinionData(int id);

	IRelationshipData GetRelationshipDataWith(int id);

	int GetTotalOpinion(int id);

	IRelationshipData GetOrCreateRelationshipDataWith(Relatable owner, int id, string name, GENDER gender);

	IRelationshipData GetOrCreateRelationshipDataWith(Relatable owner, Relatable relatable);

	int GetCompatibility(int targetID);

	bool HasSpecialPositiveRelationshipWith(Character characterThatDied);

	bool TryBreakUp(Character owner, Character targetCharacter, string reason);

	bool HasGrudgeAgainst(Relatable p_target);

	bool HasGrudgeAgainstAliveCharactersWithPath(Character p_source);

	bool SetHasGrudgeAgainst(Character p_actor, Character p_target, bool p_state);

	void OnOwnerDied(Character p_owner);

	void CheckIfCharacterIsStillReferenced(Character p_character);

	void DisconnectFromCharacter(Character p_character);

	void CleanUp();
}
