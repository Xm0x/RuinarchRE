public class CharacterRelationshipProcessor : IRelationshipProcessor
{
	public void OnRelationshipAdded(Relatable rel1, Relatable rel2, RELATIONSHIP_TYPE relType)
	{
		Character character = rel1 as Character;
		Character character2 = rel2 as Character;
		character.relationshipContainer.AdjustOpinion(character, character2, "Base", 0);
		if (relType == RELATIONSHIP_TYPE.LOVER && character.homeSettlement != null && character2.homeSettlement != null && character.homeRegion == character2.homeRegion && character.homeStructure != character2.homeStructure)
		{
			if (character.homeStructure == null && character2.homeStructure != null)
			{
				character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, character2);
			}
			else if (character.homeStructure != null && character2.homeStructure == null)
			{
				character2.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, character);
			}
			else
			{
				character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, character2);
			}
		}
	}

	public void OnRelationshipRemoved(Relatable rel1, Relatable rel2, RELATIONSHIP_TYPE relType)
	{
		Character obj = rel1 as Character;
		Character target = rel2 as Character;
		string opinionText = relType.ToStringEnumWithSpaceNormalized();
		obj.relationshipContainer.RemoveOpinion(target, opinionText);
	}
}
