using System.Collections.Generic;

public interface IRelationshipData
{
	List<RELATIONSHIP_TYPE> relationships { get; }

	OpinionData opinions { get; }

	string targetName { get; }

	GENDER targetGender { get; }

	bool hasGrudge { get; }

	void AddRelationship(RELATIONSHIP_TYPE relType);

	void RemoveRelationship(RELATIONSHIP_TYPE relType);

	RELATIONSHIP_TYPE GetFirstMajorRelationship();

	bool HasRelationship(RELATIONSHIP_TYPE rel);

	bool HasRelationship(RELATIONSHIP_TYPE rel1, RELATIONSHIP_TYPE rel2);

	bool HasRelationship(RELATIONSHIP_TYPE rel1, RELATIONSHIP_TYPE rel2, RELATIONSHIP_TYPE rel3);

	bool HasRelationship(RELATIONSHIP_TYPE rel1, RELATIONSHIP_TYPE rel2, RELATIONSHIP_TYPE rel3, RELATIONSHIP_TYPE rel4);

	bool HasRelationship(RELATIONSHIP_TYPE rel1, RELATIONSHIP_TYPE rel2, RELATIONSHIP_TYPE rel3, RELATIONSHIP_TYPE rel4, RELATIONSHIP_TYPE rel5);

	void SetTargetName(string name);

	void SetTargetGender(GENDER gender);

	void SetHasGrudge(bool p_state);

	bool IsFamilyMember();

	bool IsLoverOrAffair();
}
