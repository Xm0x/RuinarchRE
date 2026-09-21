public interface IRelationshipProcessor
{
	void OnRelationshipAdded(Relatable rel1, Relatable rel2, RELATIONSHIP_TYPE relType);

	void OnRelationshipRemoved(Relatable rel1, Relatable rel2, RELATIONSHIP_TYPE relType);
}
