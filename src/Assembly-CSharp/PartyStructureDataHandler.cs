using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class PartyStructureDataHandler
{
	public int prismAvailableSummonCount { get; private set; }

	public int maraudAvailableSummonCount { get; private set; }

	public int kennelAvailableSummonCount { get; private set; }

	public int prisonerAvailableSummonCount { get; private set; }

	public int snatchObjectSummonCount { get; private set; }

	public PartyStructureDataHandler()
	{
		Initialize();
	}

	public void Initialize()
	{
		prismAvailableSummonCount = 3;
		maraudAvailableSummonCount = 2;
		kennelAvailableSummonCount = 2;
		prisonerAvailableSummonCount = 2;
		snatchObjectSummonCount = 2;
	}

	public PartyStructureDataHandler(SaveDatapartyStructureDataHandler data)
	{
		prismAvailableSummonCount = data.prismAvailableSummonCount;
		maraudAvailableSummonCount = data.maraudAvailableSummonCount;
		kennelAvailableSummonCount = data.kennelAvailableSummonCount;
		prisonerAvailableSummonCount = data.prisonerAvailableSummonCount;
		snatchObjectSummonCount = data.snatchObjectSummonCount;
	}

	public int GetSummonCountForStructureType(LocationStructure p_structure)
	{
		return GetSummonCountForStructureType(p_structure.structureType);
	}

	public int GetSummonCountForSnatchObject()
	{
		return snatchObjectSummonCount;
	}

	public int GetSummonCountForStructureType(STRUCTURE_TYPE p_type)
	{
		return p_type switch
		{
			STRUCTURE_TYPE.MARAUD => maraudAvailableSummonCount, 
			STRUCTURE_TYPE.PRISM => prismAvailableSummonCount, 
			STRUCTURE_TYPE.KENNEL => kennelAvailableSummonCount, 
			STRUCTURE_TYPE.TORTURE_CHAMBERS => prisonerAvailableSummonCount, 
			_ => -1, 
		};
	}

	public void UpdateSummonCountForStructureType(LocationStructure p_structure)
	{
		UpdateSummonCountForStructureType(p_structure.structureType);
	}

	public void UpdateSummonCountForStructureType(STRUCTURE_TYPE p_type)
	{
		switch (p_type)
		{
		case STRUCTURE_TYPE.MARAUD:
			maraudAvailableSummonCount++;
			break;
		case STRUCTURE_TYPE.PRISM:
			prismAvailableSummonCount += 2;
			break;
		case STRUCTURE_TYPE.KENNEL:
			kennelAvailableSummonCount++;
			break;
		case STRUCTURE_TYPE.TORTURE_CHAMBERS:
			prisonerAvailableSummonCount++;
			break;
		}
	}

	public void AddSummonCountForSnatchObject()
	{
		snatchObjectSummonCount++;
	}

	public bool HasStructureCapableOfPartyBehaviour(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR p_behaviour)
	{
		for (int i = 0; i < PlayerManager.Instance.player.playerSettlement.allStructures.Count; i++)
		{
			LocationStructure locationStructure = PlayerManager.Instance.player.playerSettlement.allStructures[i];
			if (locationStructure.partyStructureComponent != null && locationStructure.partyStructureComponent.availableBehaviours.Contains(p_behaviour))
			{
				return true;
			}
		}
		for (int j = 0; j < InnerMapManager.Instance.currentMonsterSpawners.Count; j++)
		{
			MonsterSpawner monsterSpawner = InnerMapManager.Instance.currentMonsterSpawners[j];
			if (monsterSpawner.gridTileLocation != null && monsterSpawner.gridTileLocation.structure.partyStructureComponent != null && monsterSpawner.gridTileLocation.structure.partyStructureComponent.availableBehaviours.Contains(p_behaviour))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAvailableStructureForBehaviour(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR p_behaviour)
	{
		return GetFirstAvailableStructureForBehaviour(p_behaviour) != null;
	}

	public LocationStructure GetFirstAvailableStructureForBehaviour(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR p_behaviour)
	{
		for (int i = 0; i < PlayerManager.Instance.player.playerSettlement.allStructures.Count; i++)
		{
			LocationStructure locationStructure = PlayerManager.Instance.player.playerSettlement.allStructures[i];
			if (locationStructure.partyStructureComponent != null && locationStructure.partyStructureComponent.IsBehaviourAvailable(p_behaviour) && locationStructure.partyStructureComponent.party == null && locationStructure.partyStructureComponent.IsAvailable() && (!locationStructure.structureType.IsSpecialStructure() || locationStructure.partyStructureComponent.HasValidResidents()))
			{
				return locationStructure;
			}
		}
		for (int j = 0; j < InnerMapManager.Instance.currentMonsterSpawners.Count; j++)
		{
			MonsterSpawner monsterSpawner = InnerMapManager.Instance.currentMonsterSpawners[j];
			if (monsterSpawner.gridTileLocation != null)
			{
				LocationStructure structure = monsterSpawner.gridTileLocation.structure;
				if (structure.partyStructureComponent != null && structure.partyStructureComponent.IsBehaviourAvailable(p_behaviour) && structure.partyStructureComponent.party == null && structure.partyStructureComponent.IsAvailable() && (!structure.structureType.IsSpecialStructure() || structure.partyStructureComponent.HasValidResidents()))
				{
					return structure;
				}
			}
		}
		return null;
	}

	public bool HasPartyStructureAlreadyTargeting(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR p_behaviour, IPartyQuestTarget target)
	{
		for (int i = 0; i < PlayerManager.Instance.player.playerSettlement.allStructures.Count; i++)
		{
			LocationStructure locationStructure = PlayerManager.Instance.player.playerSettlement.allStructures[i];
			if (locationStructure.partyStructureComponent != null && locationStructure.partyStructureComponent.availableBehaviours.Contains(p_behaviour) && locationStructure.partyStructureComponent.party != null && locationStructure.partyStructureComponent.party.isActive && locationStructure.partyStructureComponent.party.currentQuest.target == target)
			{
				return true;
			}
		}
		for (int j = 0; j < InnerMapManager.Instance.currentMonsterSpawners.Count; j++)
		{
			MonsterSpawner monsterSpawner = InnerMapManager.Instance.currentMonsterSpawners[j];
			if (monsterSpawner.gridTileLocation != null)
			{
				LocationStructure structure = monsterSpawner.gridTileLocation.structure;
				if (structure.partyStructureComponent != null && structure.partyStructureComponent.availableBehaviours.Contains(p_behaviour) && structure.partyStructureComponent.party != null && structure.partyStructureComponent.party.isActive && structure.partyStructureComponent.party.currentQuest.target == target)
				{
					return true;
				}
			}
		}
		return false;
	}
}
