using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Traits;
using UnityEngine;
using UtilityScripts;

namespace Generator.Map_Generation.Components;

public class CharacterFinalization : MapGenerationComponent
{
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++)
		{
			Character character = CharacterManager.Instance.allCharacters[i];
			if (character.isNormalCharacter && !character.isDead)
			{
				character.CreateRandomInitialTraits();
				list.Add(character);
			}
		}
		AddSpecialTraitsToRandomVillager(list);
		RuinarchListPool<Character>.Release(list);
		for (int j = 0; j < FactionManager.Instance.allFactions.Count; j++)
		{
			Faction faction = FactionManager.Instance.allFactions[j];
			if (faction.isMajorNonPlayer && faction.factionType.HasIdeology(FACTION_IDEOLOGY.Exclusive, out var factionIdeology) && factionIdeology is Exclusive exclusive && (exclusive.category == EXCLUSIVE_IDEOLOGY_CATEGORIES.GENDER || exclusive.category == EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE))
			{
				List<Character> list2 = RuinarchListPool<Character>.Claim();
				list2.AddRange(faction.characters);
				for (int k = 0; k < list2.Count; k++)
				{
					Character character2 = list2[k];
					faction.CheckIfCharacterStillFitsIdeology(character2, willLog: false, rollForGrudge: false);
				}
			}
		}
		for (int l = 0; l < FactionManager.Instance.allFactions.Count; l++)
		{
			Faction faction2 = FactionManager.Instance.allFactions[l];
			if (!faction2.isMajorNonPlayer)
			{
				continue;
			}
			if (faction2.leader is Character p_member)
			{
				ApplyFactionTypeRelatedEffectToMember(faction2, p_member);
			}
			List<Character> list3 = RuinarchListPool<Character>.Claim();
			for (int m = 0; m < faction2.characters.Count; m++)
			{
				Character character3 = faction2.characters[m];
				if (character3.isSettlementRuler)
				{
					list3.Add(character3);
				}
			}
			for (int n = 0; n < list3.Count; n++)
			{
				Character p_member2 = list3[n];
				ApplyFactionTypeRelatedEffectToMember(faction2, p_member2);
			}
			List<Character> list4 = RuinarchListPool<Character>.Claim();
			for (int num = 0; num < faction2.characters.Count; num++)
			{
				Character character4 = faction2.characters[num];
				if (!character4.isFactionLeader && !character4.isSettlementRuler)
				{
					list4.Add(character4);
				}
			}
			int num2 = Mathf.FloorToInt((float)list4.Count() / 2f);
			for (int num3 = 0; num3 < list4.Count; num3++)
			{
				Character p_member3 = list4[num3];
				if (num3 < num2)
				{
					ApplyFactionTypeRelatedEffectToMember(faction2, p_member3);
				}
			}
			RuinarchListPool<Character>.Release(list3);
			RuinarchListPool<Character>.Release(list4);
		}
		yield return null;
	}

	public static void ApplyFactionTypeRelatedEffectToMember(Faction p_faction, Character p_member)
	{
		switch (p_faction.factionType.type)
		{
		case FACTION_TYPE.Vampire_Clan:
			if (!p_member.classComponent.IsStalkerCannotBeTurned())
			{
				p_member.traitContainer.AddTrait(p_member, "Vampire");
			}
			break;
		case FACTION_TYPE.Lycan_Clan:
			if (!p_member.classComponent.IsStalkerCannotBeTurned())
			{
				new LycanthropeData(p_member);
			}
			break;
		case FACTION_TYPE.Demon_Cult:
			break;
		}
	}

	private void AddSpecialTraitsToRandomVillager(List<Character> p_allAliveVillagers)
	{
		if (GameUtilities.RollChance(20))
		{
			Character randomElement = CollectionUtilities.GetRandomElement(p_allAliveVillagers);
			randomElement?.traitContainer.AddTrait(randomElement, "Nullchild");
		}
		if (GameUtilities.RollChance(30))
		{
			Character randomElement2 = CollectionUtilities.GetRandomElement(p_allAliveVillagers);
			randomElement2?.traitContainer.AddTrait(randomElement2, "Jinxed");
		}
	}

	private void ZenkoCharacterRandomInitialTraits(Character character)
	{
		List<string> list = RuinarchListPool<string>.Claim();
		list.AddRange(TraitManager.Instance.buffTraitPool);
		List<string> list2 = RuinarchListPool<string>.Claim();
		list2.AddRange(TraitManager.Instance.neutralTraitPool);
		List<string> list3 = RuinarchListPool<string>.Claim();
		list3.AddRange(TraitManager.Instance.flawTraitPool);
		List<Faction> list4 = RuinarchListPool<Faction>.Claim();
		for (int i = 0; i < DatabaseManager.Instance.factionDatabase.allFactionsList.Count; i++)
		{
			Faction faction = DatabaseManager.Instance.factionDatabase.allFactionsList[i];
			if (faction.isMajorNonPlayer)
			{
				list4.Add(faction);
			}
		}
		switch (list4.IndexOf(character.faction))
		{
		case 0:
			character.traitContainer.AddTrait(character, "Cold Blooded");
			break;
		case 1:
			character.traitContainer.AddTrait(character, "Fire Resistant");
			break;
		case 2:
			character.traitContainer.AddTrait(character, "Electric");
			break;
		default:
			character.traitContainer.AddTrait(character, "Venomous");
			break;
		}
		List<string> list5 = RuinarchListPool<string>.Claim();
		if (GameUtilities.RollChance(80))
		{
			list5.AddRange(list);
			list5.AddRange(list2);
			if (list5.Count <= 0)
			{
				throw new Exception("No more buff or neutral traits!");
			}
			string randomElement = CollectionUtilities.GetRandomElement(list5);
			list.Remove(randomElement);
			list2.Remove(randomElement);
			character.traitContainer.AddTrait(character, randomElement);
			Trait traitOrStatus = character.traitContainer.GetTraitOrStatus<Trait>(randomElement);
			if (traitOrStatus.mutuallyExclusive != null)
			{
				CollectionUtilities.RemoveElements(list, traitOrStatus.mutuallyExclusive);
				CollectionUtilities.RemoveElements(list2, traitOrStatus.mutuallyExclusive);
				CollectionUtilities.RemoveElements(list3, traitOrStatus.mutuallyExclusive);
			}
		}
		if (GameUtilities.RollChance(40))
		{
			list5.Clear();
			list5.AddRange(list);
			list5.AddRange(list2);
			list5.AddRange(list3);
			if (list5.Count <= 0)
			{
				throw new Exception("No more buff, neutral or flaw traits!");
			}
			string randomElement2 = CollectionUtilities.GetRandomElement(list5);
			character.traitContainer.AddTrait(character, randomElement2);
		}
		RuinarchListPool<string>.Release(list5);
		RuinarchListPool<string>.Release(list);
		RuinarchListPool<string>.Release(list2);
		RuinarchListPool<string>.Release(list3);
		RuinarchListPool<Faction>.Release(list4);
	}

	private void IcalawaCharacterRandomInitialTraits(int index, Character character, int totalCharacters)
	{
		if (index + 1 == totalCharacters)
		{
			character.traitContainer.AddTrait(character, "Evil");
		}
		else
		{
			character.traitContainer.AddTrait(character, "Blessed");
		}
		List<string> list = RuinarchListPool<string>.Claim();
		list.AddRange(TraitManager.Instance.buffTraitPool);
		list.Remove("Blessed");
		List<string> list2 = RuinarchListPool<string>.Claim();
		list2.AddRange(TraitManager.Instance.neutralTraitPool);
		List<string> list3 = RuinarchListPool<string>.Claim();
		list3.AddRange(TraitManager.Instance.flawTraitPool);
		list3.Remove("Evil");
		character.CreateRandomInitialTraits(list, list2, list3);
		RuinarchListPool<string>.Release(list);
		RuinarchListPool<string>.Release(list2);
		RuinarchListPool<string>.Release(list3);
	}
}
