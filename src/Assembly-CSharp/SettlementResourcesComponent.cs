using Inner_Maps;
using Inner_Maps.Location_Structures;

public class SettlementResourcesComponent : NPCSettlementComponent
{
	public SettlementResourcesComponent()
	{
	}

	public SettlementResourcesComponent(SaveDataSettlementResourcesComponent data)
	{
	}

	public int GetFoodSupplyCapacity()
	{
		int num = 0;
		for (int i = 0; i < base.owner.residents.Count; i++)
		{
			Character character = base.owner.residents[i];
			num += character.classComponent.GetFoodSupplyCapacityValue();
		}
		return num;
	}

	public int GetPotentialFoodSupplyCapacity()
	{
		int num = 0;
		for (int i = 0; i < base.owner.allStructures.Count; i++)
		{
			LocationStructure locationStructure = base.owner.allStructures[i];
			if (locationStructure.structureType.IsFoodProducingStructure() && !locationStructure.hasBeenDestroyed && locationStructure is ManMadeStructure manMadeStructure && (manMadeStructure.HasAssignedWorker() || manMadeStructure.HasSettlementOrLocalResidentThatCanWorkHere()))
			{
				num = ((!manMadeStructure.HasMaxWorkerCapacity()) ? (num + 8) : (num + manMadeStructure.maxWorkerCapacity * 8));
			}
		}
		return num;
	}

	public int GetResourceSupplyCapacity()
	{
		int num = 0;
		Faction faction = base.owner.owner;
		if (faction != null && faction.factionType.type == FACTION_TYPE.Human_Empire)
		{
			for (int i = 0; i < base.owner.residents.Count; i++)
			{
				Character character = base.owner.residents[i];
				num += character.classComponent.GetResourceSupplyCapacityValue(STRUCTURE_TYPE.MINE);
			}
		}
		else
		{
			Faction faction2 = base.owner.owner;
			if (faction2 != null && faction2.factionType.type == FACTION_TYPE.Elven_Kingdom)
			{
				for (int j = 0; j < base.owner.residents.Count; j++)
				{
					Character character2 = base.owner.residents[j];
					num += character2.classComponent.GetResourceSupplyCapacityValue(STRUCTURE_TYPE.LUMBERYARD);
				}
			}
			else
			{
				for (int k = 0; k < base.owner.residents.Count; k++)
				{
					Character character3 = base.owner.residents[k];
					num += character3.classComponent.GetResourceSupplyCapacityValue(STRUCTURE_TYPE.MINE);
					num += character3.classComponent.GetResourceSupplyCapacityValue(STRUCTURE_TYPE.LUMBERYARD);
				}
			}
		}
		return num;
	}

	public int GetPotentialResourceSupplyCapacity()
	{
		int num = 0;
		for (int i = 0; i < base.owner.allStructures.Count; i++)
		{
			LocationStructure locationStructure = base.owner.allStructures[i];
			if ((locationStructure is Lumberyard || locationStructure is Mine) && !locationStructure.hasBeenDestroyed && locationStructure is ManMadeStructure manMadeStructure && (manMadeStructure.HasAssignedWorker() || manMadeStructure.HasSettlementOrLocalResidentThatCanWorkHere()))
			{
				num = ((!manMadeStructure.HasMaxWorkerCapacity()) ? (num + 8) : (num + manMadeStructure.maxWorkerCapacity * 8));
			}
		}
		return num;
	}

	private int GetNumOfResidentsThatHasClass(string p_className)
	{
		int num = 0;
		for (int i = 0; i < base.owner.residents.Count; i++)
		{
			Character character = base.owner.residents[i];
			LocationGridTile gridTileLocation = character.gridTileLocation;
			bool flag = !character.traitContainer.HasTrait("Paralyzed", "Restrained", "Quarantined") || gridTileLocation == null || !character.hasMarker || gridTileLocation.IsPartOfSettlement(base.owner);
			if (!character.isDead && flag && character.characterClass.className == p_className)
			{
				num++;
			}
		}
		return num;
	}

	private int GetNumOfResidentsThatHasClass(string p_className1, string p_className2)
	{
		int num = 0;
		for (int i = 0; i < base.owner.residents.Count; i++)
		{
			Character character = base.owner.residents[i];
			LocationGridTile gridTileLocation = character.gridTileLocation;
			bool flag = !character.traitContainer.HasTrait("Paralyzed", "Restrained", "Quarantined") || gridTileLocation == null || !character.hasMarker || gridTileLocation.IsPartOfSettlement(base.owner);
			if (!character.isDead && flag && (character.characterClass.className == p_className1 || character.characterClass.className == p_className2))
			{
				num++;
			}
		}
		return num;
	}

	public void LoadReferences(SaveDataSettlementResourcesComponent saveDataNpcSettlement)
	{
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
