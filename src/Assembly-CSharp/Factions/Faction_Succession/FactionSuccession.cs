using System.Collections.Generic;
using System.Linq;
using Factions.Faction_Types;
using UtilityScripts;

namespace Factions.Faction_Succession;

public class FactionSuccession
{
	public FACTION_SUCCESSION_TYPE type { get; protected set; }

	public string name { get; protected set; }

	public FactionSuccession(FACTION_SUCCESSION_TYPE type)
	{
		this.type = type;
		name = Utilities.NormalizeStringUpperCaseFirstLetters(type.ToString());
	}

	public virtual void PopulateSuccessorListWeightsInOrder(List<Character> successorList, WeightedDictionary<Character> weightedDictionary, Faction faction)
	{
	}

	public virtual Character PickSuccessor(Character[] successors, int[] weights)
	{
		int p_max = weights.Sum();
		int num = GameUtilities.RandomBetweenTwoNumbers(0, p_max);
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < weights.Length; i++)
		{
			int num4 = weights[i];
			num2 += num4;
			if (num >= num3 && num < num2)
			{
				return successors[i];
			}
			num3 = num2;
		}
		foreach (Character character in successors)
		{
			if (character != null)
			{
				return character;
			}
		}
		return null;
	}

	protected bool CanBeCandidateForSuccession(Character character, Faction faction)
	{
		if (character.isDead || character.isBeingSeized || character.isInLimbo || character.isFactionLeader || character.petComponent.petOwner != null)
		{
			return false;
		}
		if (faction.factionType is CultFaction && !character.isNormalCharacter)
		{
			return false;
		}
		return true;
	}
}
