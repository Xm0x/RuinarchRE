using System.Collections.Generic;
using Traits;
using UtilityScripts;

namespace Factions.Faction_Succession;

public class Popularity : FactionSuccession
{
	public Popularity()
		: base(FACTION_SUCCESSION_TYPE.Popularity)
	{
	}

	public override void PopulateSuccessorListWeightsInOrder(List<Character> successorList, WeightedDictionary<Character> weightedDictionary, Faction faction)
	{
		base.PopulateSuccessorListWeightsInOrder(successorList, weightedDictionary, faction);
		for (int i = 0; i < faction.characters.Count; i++)
		{
			Character character = faction.characters[i];
			if (!CanBeCandidateForSuccession(character, faction))
			{
				continue;
			}
			int num = 0;
			num = GameUtilities.RandomBetweenTwoNumbers(40, 60);
			if (faction.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Vampires))
			{
				Vampire traitOrStatus = character.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
				if (traitOrStatus != null && traitOrStatus.DoesFactionKnowThisVampire(faction, includeDeadMembersInChecking: false))
				{
					num += 100;
				}
			}
			if (faction.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Werewolves) && character.isLycanthrope && character.lycanData.DoesFactionKnowThisLycan(faction))
			{
				num += 100;
			}
			if (character.characterClass.className == "Noble")
			{
				num += 40;
			}
			for (int j = 0; j < faction.characters.Count; j++)
			{
				Character character2 = faction.characters[j];
				if (character2 != character && !character2.isDead)
				{
					if (character2.relationshipContainer.IsFriendsWith(character))
					{
						num += GameUtilities.RandomBetweenTwoNumbers(40, 50);
					}
					else if (character2.relationshipContainer.IsEnemiesWith(character))
					{
						num += -20;
					}
				}
			}
			if (character.traitContainer.HasTrait("Inspiring"))
			{
				num += 25;
			}
			if (character.traitContainer.HasTrait("Authoritative"))
			{
				num += 50;
			}
			if (character.traitContainer.HasTrait("Unattractive"))
			{
				num += -20;
			}
			if (character.hasUnresolvedCrime)
			{
				num += -50;
			}
			if (faction.factionType.IsCivilian(character.characterClass.className))
			{
				num += -20;
			}
			num += AdditionalWeightBasedOnSocialTalent(character);
			RaceData raceData = RaceManager.Instance.GetRaceData(character.race);
			if ((raceData.category == CHARACTER_CATEGORY.Beast || raceData.category == CHARACTER_CATEGORY.Undead || character.characterClass.IsZombie()) && faction.HasMemberThatIsSapientAndIsAtHomeOrHasJoinedQuest())
			{
				num += -30;
			}
			if (character.traitContainer.HasTrait("Enslaved"))
			{
				num += -40;
			}
			if (character.crimeComponent.IsWantedBy(faction))
			{
				num = 0;
			}
			if (faction.leader is Character character3 && character3 != character && character3.relationshipContainer.IsCharacterConsideredMissingOrPresumedDead(character3, character))
			{
				num = 0;
			}
			if (num < 0)
			{
				num = 0;
			}
			if (num <= 0)
			{
				continue;
			}
			weightedDictionary.AddElement(character, num);
			if (successorList.Count > 0)
			{
				bool flag = false;
				for (int k = 0; k < successorList.Count; k++)
				{
					int elementWeight = weightedDictionary.GetElementWeight(successorList[k]);
					if (num > elementWeight)
					{
						flag = true;
						successorList.Insert(k, character);
						break;
					}
				}
				if (!flag)
				{
					successorList.Add(character);
				}
			}
			else
			{
				successorList.Add(character);
			}
		}
	}

	private int AdditionalWeightBasedOnSocialTalent(Character p_character)
	{
		if (p_character.HasTalents())
		{
			return p_character.TryGetTalentLevel(CHARACTER_TALENT.Social) switch
			{
				1 => 0, 
				2 => 50, 
				3 => 100, 
				4 => 250, 
				5 => 250, 
				_ => 0, 
			};
		}
		return 0;
	}
}
