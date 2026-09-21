using System.Collections.Generic;

public class BaseRelationshipData : IRelationshipData
{
	public string targetName { get; private set; }

	public GENDER targetGender { get; private set; }

	public List<RELATIONSHIP_TYPE> relationships { get; private set; }

	public OpinionData opinions { get; private set; }

	public bool hasGrudge { get; private set; }

	public BaseRelationshipData()
	{
		relationships = new List<RELATIONSHIP_TYPE>();
		opinions = ObjectPoolManager.Instance.CreateNewOpinionData();
	}

	public void SetTargetName(string name)
	{
		targetName = name;
	}

	public void SetTargetGender(GENDER gender)
	{
		targetGender = gender;
	}

	public void SetHasGrudge(bool p_state)
	{
		hasGrudge = p_state;
	}

	public void AddRelationship(RELATIONSHIP_TYPE relType)
	{
		relationships.Add(relType);
	}

	public void RemoveRelationship(RELATIONSHIP_TYPE relType)
	{
		relationships.Remove(relType);
	}

	public bool HasRelationship(RELATIONSHIP_TYPE rel)
	{
		if (relationships.Contains(rel))
		{
			return true;
		}
		return false;
	}

	public bool HasRelationship(RELATIONSHIP_TYPE rel1, RELATIONSHIP_TYPE rel2)
	{
		for (int i = 0; i < relationships.Count; i++)
		{
			RELATIONSHIP_TYPE rELATIONSHIP_TYPE = relationships[i];
			if (rELATIONSHIP_TYPE == rel1 || rELATIONSHIP_TYPE == rel2)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasRelationship(RELATIONSHIP_TYPE rel1, RELATIONSHIP_TYPE rel2, RELATIONSHIP_TYPE rel3)
	{
		for (int i = 0; i < relationships.Count; i++)
		{
			RELATIONSHIP_TYPE rELATIONSHIP_TYPE = relationships[i];
			if (rELATIONSHIP_TYPE == rel1 || rELATIONSHIP_TYPE == rel2 || rELATIONSHIP_TYPE == rel3)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasRelationship(RELATIONSHIP_TYPE rel1, RELATIONSHIP_TYPE rel2, RELATIONSHIP_TYPE rel3, RELATIONSHIP_TYPE rel4)
	{
		for (int i = 0; i < relationships.Count; i++)
		{
			RELATIONSHIP_TYPE rELATIONSHIP_TYPE = relationships[i];
			if (rELATIONSHIP_TYPE == rel1 || rELATIONSHIP_TYPE == rel2 || rELATIONSHIP_TYPE == rel3 || rELATIONSHIP_TYPE == rel4)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasRelationship(RELATIONSHIP_TYPE rel1, RELATIONSHIP_TYPE rel2, RELATIONSHIP_TYPE rel3, RELATIONSHIP_TYPE rel4, RELATIONSHIP_TYPE rel5)
	{
		for (int i = 0; i < relationships.Count; i++)
		{
			RELATIONSHIP_TYPE rELATIONSHIP_TYPE = relationships[i];
			if (rELATIONSHIP_TYPE == rel1 || rELATIONSHIP_TYPE == rel2 || rELATIONSHIP_TYPE == rel3 || rELATIONSHIP_TYPE == rel4 || rELATIONSHIP_TYPE == rel5)
			{
				return true;
			}
		}
		return false;
	}

	public RELATIONSHIP_TYPE GetFirstMajorRelationship()
	{
		if (relationships.Count > 0)
		{
			return relationships[0];
		}
		return RELATIONSHIP_TYPE.NONE;
	}

	public bool IsFamilyMember()
	{
		return HasRelationship(RELATIONSHIP_TYPE.CHILD, RELATIONSHIP_TYPE.PARENT, RELATIONSHIP_TYPE.SIBLING, RELATIONSHIP_TYPE.RELATIVE);
	}

	public bool IsLoverOrAffair()
	{
		return HasRelationship(RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR);
	}
}
