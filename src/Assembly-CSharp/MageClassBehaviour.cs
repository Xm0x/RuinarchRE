using Inner_Maps.Location_Structures;
using UtilityScripts;

public class MageClassBehaviour : CharacterClassBehaviour
{
	public override bool TryDoBehaviour(Character p_character, ref JobQueueItem p_producedJob, ref string log)
	{
		if (MagicUserBehaviour(p_character, ref log, ref p_producedJob))
		{
			return true;
		}
		if (p_character.traitContainer.HasTrait("Demon Cultist") || (p_character.faction != null && p_character.faction.isPlayerFaction))
		{
			return false;
		}
		if (GameUtilities.RollChance(p_character.classComponent.devastationCounter, ref log) || GameUtilities.RollChance(ChanceData.GetChance(CHANCE_TYPE.Devastation_Ritual)))
		{
			LocationStructure locationStructure = GridMap.Instance.mainRegion.GetFirstNearbyMageTowerFromCharacterWithNoResidentThatIsNotTargetOfDevastationRitual(p_character);
			if (locationStructure == null)
			{
				locationStructure = GridMap.Instance.mainRegion.GetFirstNearbyMageTowerFromCharacterThatIsNotTargetOfDevastationRitual(p_character);
			}
			if (locationStructure != null)
			{
				if (locationStructure.HasAliveResident(null) && locationStructure.residents[0].IsHostileWith(p_character))
				{
					if (!p_character.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Extermination, locationStructure))
					{
						p_character.faction.partyQuestBoard.CreateExterminatePartyQuest(p_character, p_character.homeSettlement, locationStructure);
					}
					return false;
				}
				if (!p_character.jobQueue.HasJob(JOB_TYPE.DEVASTATION_RITUAL))
				{
					MagicCircle randomTileObjectOfTypeThatHasTileLocationAndIsBuilt = locationStructure.GetRandomTileObjectOfTypeThatHasTileLocationAndIsBuilt<MagicCircle>();
					if (randomTileObjectOfTypeThatHasTileLocationAndIsBuilt != null)
					{
						p_character.jobComponent.CreateDevastationRitualJob(randomTileObjectOfTypeThatHasTileLocationAndIsBuilt, locationStructure, ref p_producedJob);
						if (p_producedJob != null)
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}
}
