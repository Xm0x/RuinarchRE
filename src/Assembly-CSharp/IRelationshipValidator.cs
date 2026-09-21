public interface IRelationshipValidator
{
	bool CanHaveRelationship(Relatable rel1, Relatable rel2, RELATIONSHIP_TYPE relType);
}
