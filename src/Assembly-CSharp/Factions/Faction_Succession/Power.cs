using System.Collections.Generic;
using Traits;
using UtilityScripts;

namespace Factions.Faction_Succession;

public class Power : FactionSuccession
{
	public Power()
		: base(FACTION_SUCCESSION_TYPE.Power)
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
			for (int j = 0; j < character.combatComponent.numOfKilledCharacters; j++)
			{
				num += GameUtilities.RandomBetweenTwoNumbers(20, 30);
			}
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
			if (character.characterClass.IsCombatant())
			{
				num += GameUtilities.RandomBetweenTwoNumbers(50, 70);
			}
			if (character.traitContainer.HasTrait("Mighty"))
			{
				num += 50;
			}
			if (character.traitContainer.HasTrait("Ruthless"))
			{
				num += 50;
			}
			if (character.traitContainer.HasTrait("Authoritative"))
			{
				num += 50;
			}
			if (faction.factionType.IsCivilian(character.characterClass.className))
			{
				num += -20;
			}
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
			if (faction.leader is Character character2 && character2 != character && character2.relationshipContainer.IsCharacterConsideredMissingOrPresumedDead(character2, character))
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
}
