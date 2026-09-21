using System.Collections.Generic;
using System.Linq;
using Locations.Settlements;
using Traits;
using UtilityScripts;

public class DefaultFactionRelated : CharacterBehaviour
{
	public DefaultFactionRelated()
	{
		base.priority = 26;
		attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[1] { BEHAVIOUR_COMPONENT_ATTRIBUTE.DO_NOT_SKIP_PROCESSING };
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (character.faction != null && character.crimeComponent.IsWantedBy(character.faction, CRIME_TYPE.Theft, CRIME_TYPE.Murder, CRIME_TYPE.Assault, CRIME_TYPE.Arson) && character.crimeComponent.HasWantedCrimeType(CRIME_TYPE.Theft, CRIME_TYPE.Murder, CRIME_TYPE.Assault, CRIME_TYPE.Arson) && (character.faction == null || character.faction.factionType.type != FACTION_TYPE.Bandits) && ChanceData.RollChance(CHANCE_TYPE.Join_Bandit, ref log) && character.JoinFactionProcessing() != null)
		{
			return true;
		}
		if (character.traitContainer.HasTrait("Vampire") && character.faction != null && character.faction.factionType.type != FACTION_TYPE.Vampire_Clan && !character.traitContainer.GetTraitOrStatus<Vampire>("Vampire").dislikedBeingVampire && ChanceData.RollChance(CHANCE_TYPE.Join_Vampire_Clan, ref log))
		{
			Faction faction = null;
			List<Faction> list = RuinarchListPool<Faction>.Claim();
			for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++)
			{
				Faction faction2 = FactionManager.Instance.allFactions[i];
				if (faction2.factionType.type == FACTION_TYPE.Vampire_Clan && faction2 != character.prevFaction && !faction2.isDisbanded && !faction2.IsCharacterBannedFromJoining(character) && faction2.ideologyComponent.DoesCharacterFitCurrentIdeologies(character))
				{
					list.Add(faction2);
				}
			}
			faction = CollectionUtilities.GetRandomElement(list);
			RuinarchListPool<Faction>.Release(list);
			if (faction != null)
			{
				character.interruptComponent.TriggerInterrupt(INTERRUPT.Join_Faction, faction.characters[0], "join_faction_normal");
				return true;
			}
		}
		if (character.characterClass.IsReligiousCultLeaderClass(out var p_religion) && FactionManager.Instance.GetReligiousCultFactionForReligion(p_religion) == null && ChanceData.RollChance(CHANCE_TYPE.Cult_Leader_Create_Faction, ref log))
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Faction, character, "create_religious_cult");
			return true;
		}
		if (character.isVagrantOrFactionless)
		{
			if (ChanceData.RollChance(CHANCE_TYPE.Vagrant_Join_Or_Create_Faction, ref log))
			{
				character.JoinFactionProcessing();
				return true;
			}
			if (!WorldSettings.Instance.worldSettingsData.factionSettings.disableNewFactions && FactionManager.Instance.GetActiveVillagerFactionCount() < FactionManager.Instance.maxActiveVillagerFactions)
			{
				int chance = ChanceData.GetChance(CHANCE_TYPE.Base_Create_Faction_Chance);
				if (character.traitContainer.HasTrait("Inspiring", "Ambitious") || character.characterClass.className == "Vampire Lord")
				{
					chance = 15;
				}
				if (character.traitContainer.IsReligiousCultist(out var p_religion2) && FactionManager.Instance.GetReligiousCultFactionForReligion(p_religion2) != null)
				{
					chance = 0;
				}
				if (GameUtilities.RollChance(chance))
				{
					character.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Faction, character);
					return true;
				}
			}
		}
		if (!character.isFactionLeader && !character.isSettlementRuler)
		{
			if (character.faction != null && character.faction.isMajorNonPlayer && character.homeSettlement != null && character.homeSettlement.residents.Count((Character c) => c.isNormalCharacter) > 8)
			{
				int num = 0;
				if (character.moodComponent.moodState == MOOD_STATE.Bad)
				{
					num++;
				}
				else if (character.moodComponent.moodState == MOOD_STATE.Critical)
				{
					num += 4;
				}
				if (character.traitContainer.HasTrait("Betrayed") && character.faction.leader != null)
				{
					Betrayed traitOrStatus = character.traitContainer.GetTraitOrStatus<Betrayed>("Betrayed");
					Character character2 = character.faction.leader as Character;
					if (traitOrStatus.IsResponsibleForTrait(character2))
					{
						num += 30;
					}
				}
				if (GameUtilities.RollChance(num))
				{
					character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, character, "left_faction_normal");
					if (character.characterClass.className == "Vampire Lord")
					{
						character.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Faction, character);
					}
					return true;
				}
			}
			if (character.traitContainer.IsReligiousCultist(out var p_religion3))
			{
				FACTION_TYPE factionTypeForReligion = p_religion3.GetFactionTypeForReligion();
				if (character.faction == null || character.faction.factionType.type != factionTypeForReligion)
				{
					int num2 = 0;
					if (HasFactionWithMemberWithUnoccupiedDwelling(factionTypeForReligion))
					{
						num2 += 3;
					}
					if (HasFactionWith2Members(factionTypeForReligion))
					{
						num2 += 3;
					}
					if (GameUtilities.RollChance(num2))
					{
						character.JoinFactionProcessing();
					}
				}
			}
		}
		if (character.petComponent.HasWyvernPet() && character.mountComponent.TryToMountWyvernPet())
		{
			return true;
		}
		return false;
	}

	private bool HasFactionWithMemberWithUnoccupiedDwelling(FACTION_TYPE factionType)
	{
		for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++)
		{
			Faction faction = FactionManager.Instance.allFactions[i];
			if (faction.factionType.type == factionType && faction.HasMemberThatIsNotDeadHasHomeSettlementUnoccupiedDwelling())
			{
				return true;
			}
		}
		return false;
	}

	private bool HasFactionWith2Members(FACTION_TYPE factionType)
	{
		for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++)
		{
			Faction faction = FactionManager.Instance.allFactions[i];
			if (faction.factionType.type == factionType)
			{
				int aliveMembersCount = faction.GetAliveMembersCount();
				if (aliveMembersCount == 1 || aliveMembersCount == 2)
				{
					return true;
				}
			}
		}
		return false;
	}

	private int GetFactionsInRegion(Region region)
	{
		List<Faction> list = new List<Faction>();
		for (int i = 0; i < region.settlementsInRegion.Count; i++)
		{
			BaseSettlement baseSettlement = region.settlementsInRegion[i];
			if (baseSettlement is NPCSettlement && baseSettlement.owner != null && baseSettlement.owner.isMajorNonPlayer && !list.Contains(baseSettlement.owner))
			{
				list.Add(baseSettlement.owner);
			}
		}
		return list.Count;
	}
}
