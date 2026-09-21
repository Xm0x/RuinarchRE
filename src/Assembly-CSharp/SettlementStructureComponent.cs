using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class SettlementStructureComponent : NPCSettlementComponent
{
	public List<LocationStructure> linkedStructures { get; private set; }

	public SettlementStructureComponent()
	{
		linkedStructures = new List<LocationStructure>();
	}

	public SettlementStructureComponent(SaveDataSettlementStructureComponent data)
	{
		linkedStructures = new List<LocationStructure>();
	}

	public void PerHour()
	{
		TryCreateTendToWyvernCoopJob();
	}

	public void DisconnectFromStructure(LocationStructure p_structure)
	{
		RemoveLinkedStructure(p_structure);
	}

	private void TryCreateTendToWyvernCoopJob()
	{
		if (!ChanceData.RollChance(CHANCE_TYPE.Tend_Wyvern_Coop) || base.owner.HasJob(JOB_TYPE.TEND_WYVERN_COOP))
		{
			return;
		}
		LocationStructure wyvernCoopToTend = GetWyvernCoopToTend();
		if (wyvernCoopToTend != null)
		{
			int numberOfResidentsThatIsAliveMonsterAndMonsterTypeIs = base.owner.GetNumberOfResidentsThatIsAliveMonsterAndMonsterTypeIs(SUMMON_TYPE.Wyvern, SUMMON_TYPE.Wyvernling);
			int numberOfTileObjectsInAllAreas = base.owner.GetNumberOfTileObjectsInAllAreas(TILE_OBJECT_TYPE.WYVERN_EGG);
			int numberOfResidentsThatIsAliveVillager = base.owner.GetNumberOfResidentsThatIsAliveVillager();
			if (numberOfResidentsThatIsAliveMonsterAndMonsterTypeIs + numberOfTileObjectsInAllAreas < numberOfResidentsThatIsAliveVillager)
			{
				base.owner.settlementJobTriggerComponent.CreateTendWyvernCoopJob(wyvernCoopToTend as WyvernCoop);
			}
		}
	}

	private LocationStructure GetWyvernCoopToTend()
	{
		LocationStructure result = null;
		List<LocationStructure> structuresOfType = base.owner.GetStructuresOfType(STRUCTURE_TYPE.WYVERN_COOP);
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		if (structuresOfType != null)
		{
			for (int i = 0; i < structuresOfType.Count; i++)
			{
				LocationStructure locationStructure = structuresOfType[i];
				if (!locationStructure.hasBeenDestroyed && locationStructure.HasUnoccupiedTile() && locationStructure.GetNumberOfBuiltTileObjects(TILE_OBJECT_TYPE.WYVERN_EGG) < 3)
				{
					list.Add(locationStructure);
				}
			}
		}
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<LocationStructure>.Release(list);
		return result;
	}

	public bool HasLinkedStructures()
	{
		return linkedStructures.Count > 0;
	}

	public bool HasLinkedStructureWithAliveResident()
	{
		for (int i = 0; i < linkedStructures.Count; i++)
		{
			if (linkedStructures[i].HasAliveResident(null))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasLinkedStructureWithMonsterSpawner()
	{
		for (int i = 0; i < linkedStructures.Count; i++)
		{
			if (linkedStructures[i].HasTileObjectOfType(TILE_OBJECT_TYPE.MONSTER_SPAWNER))
			{
				return true;
			}
		}
		return false;
	}

	public void AddLinkedStructure(LocationStructure p_structure)
	{
		if (!linkedStructures.Contains(p_structure))
		{
			linkedStructures.Add(p_structure);
			p_structure.SetLinkedSettlement(base.owner);
		}
	}

	public void RemoveLinkedStructure(LocationStructure p_structure)
	{
		if (linkedStructures.Remove(p_structure))
		{
			p_structure.SetLinkedSettlement(null);
		}
	}

	public void RelinkAllLinkedStructures()
	{
		for (int i = 0; i < linkedStructures.Count; i++)
		{
			LocationStructure locationStructure = linkedStructures[i];
			locationStructure.SetLinkedSettlement(null);
			locationStructure.LinkThisStructureToAVillage(base.owner);
		}
		linkedStructures.Clear();
	}

	public LocationStructure GetRandomLinkedStructureForExtermination()
	{
		LocationStructure result = null;
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		if (HasLinkedStructureWithMonsterSpawner() && GameUtilities.RollChance(70))
		{
			for (int i = 0; i < linkedStructures.Count; i++)
			{
				LocationStructure locationStructure = linkedStructures[i];
				if (!locationStructure.hasBeenDestroyed && locationStructure.HasResidentForExtermination() && locationStructure.HasTileObjectOfType(TILE_OBJECT_TYPE.MONSTER_SPAWNER))
				{
					list.Add(locationStructure);
				}
			}
		}
		if (list.Count <= 0)
		{
			for (int j = 0; j < linkedStructures.Count; j++)
			{
				LocationStructure locationStructure2 = linkedStructures[j];
				if (!locationStructure2.hasBeenDestroyed && locationStructure2.HasResidentForExtermination())
				{
					list.Add(locationStructure2);
				}
			}
		}
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<LocationStructure>.Release(list);
		return result;
	}

	public string GetLinkedStructuresSummary()
	{
		string text = string.Empty;
		for (int i = 0; i < linkedStructures.Count; i++)
		{
			if (i > 0)
			{
				text += ",";
			}
			text += linkedStructures[i].name;
		}
		return text;
	}

	public void LoadReferences(SaveDataSettlementStructureComponent data)
	{
		if (data.linkedStructures == null)
		{
			return;
		}
		for (int i = 0; i < data.linkedStructures.Count; i++)
		{
			LocationStructure structureByPersistentIDSafe = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(data.linkedStructures[i]);
			if (structureByPersistentIDSafe != null)
			{
				linkedStructures.Add(structureByPersistentIDSafe);
				structureByPersistentIDSafe.SetLinkedSettlement(base.owner);
			}
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		linkedStructures.Contains(p_structure);
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
