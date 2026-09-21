using System.Collections.Generic;
using Traits;

public class CharacterRelationshipValidator : IRelationshipValidator
{
	public bool CanHaveRelationship(Relatable character, Relatable target, RELATIONSHIP_TYPE type)
	{
		Character character2 = target as Character;
		Character character3 = character as Character;
		List<RELATIONSHIP_TYPE> list = character3.relationshipContainer.GetRelationshipDataWith(character2)?.relationships ?? null;
		switch (type)
		{
		case RELATIONSHIP_TYPE.LOVER:
			if (character3.traitContainer.HasTrait("Griefstricken") || character2.traitContainer.HasTrait("Griefstricken"))
			{
				return false;
			}
			if (character3.traitContainer.HasTrait("Heartbroken") || character2.traitContainer.HasTrait("Heartbroken"))
			{
				return false;
			}
			if (character3.relationshipContainer.HasAliveOrUnspawnedRelationship(RELATIONSHIP_TYPE.LOVER))
			{
				return false;
			}
			if ((list != null && list.Contains(RELATIONSHIP_TYPE.AFFAIR)) || character3.relationshipContainer.IsEnemiesWith(character2))
			{
				return false;
			}
			return true;
		case RELATIONSHIP_TYPE.AFFAIR:
			if (character3.traitContainer.HasTrait("Griefstricken") || character2.traitContainer.HasTrait("Griefstricken"))
			{
				return false;
			}
			if (character3.traitContainer.HasTrait("Heartbroken") || character2.traitContainer.HasTrait("Heartbroken"))
			{
				return false;
			}
			if (!UnfaithfulCanHaveAffairChecking(character3, character2))
			{
				return false;
			}
			if (list != null && list.Contains(RELATIONSHIP_TYPE.LOVER))
			{
				return false;
			}
			if (!character2.relationshipContainer.HasAliveOrUnspawnedRelationship(RELATIONSHIP_TYPE.LOVER) && !character3.relationshipContainer.HasAliveOrUnspawnedRelationship(RELATIONSHIP_TYPE.LOVER))
			{
				return false;
			}
			return true;
		default:
			return true;
		}
	}

	private bool UnfaithfulCanHaveAffairChecking(Character p_character1, Character p_character2)
	{
		Unfaithful traitOrStatus = p_character1.traitContainer.GetTraitOrStatus<Unfaithful>("Unfaithful");
		if (traitOrStatus != null)
		{
			return traitOrStatus.CanBeLoverOrAffairBasedOnPersonalConstraints(p_character1, p_character2);
		}
		if (!p_character1.relationshipContainer.HasAliveOrUnspawnedRelationship(RELATIONSHIP_TYPE.LOVER))
		{
			return true;
		}
		return false;
	}
}
