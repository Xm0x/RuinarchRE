using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class CharacterStructureComponent : CharacterComponent
{
	public ManMadeStructure workPlaceStructure { get; private set; }

	public CharacterStructureComponent()
	{
	}

	public CharacterStructureComponent(SaveDataCharacterStructureComponent data)
	{
	}

	public void SetWorkPlaceStructure(ManMadeStructure p_structure)
	{
		if (workPlaceStructure != p_structure)
		{
			workPlaceStructure = p_structure;
			if (HasWorkPlaceStructure() && base.owner.partyComponent.hasParty)
			{
				base.owner.interruptComponent.TriggerInterrupt(INTERRUPT.Left_Party, base.owner, "", null, "Left_Party_Became_Civilian");
			}
		}
	}

	public bool HasWorkPlaceStructure()
	{
		return workPlaceStructure != null;
	}

	public void ProcessUnclaimWorkStructure()
	{
		if (!HasWorkPlaceStructure())
		{
			return;
		}
		CharacterClass characterClass = base.owner.characterClass;
		if (characterClass.isVillagerType)
		{
			if (workPlaceStructure.structureType != characterClass.workStructureType)
			{
				workPlaceStructure.RemoveAssignedWorker(base.owner);
			}
		}
		else if (workPlaceStructure.structureType == STRUCTURE_TYPE.HOSPICE && characterClass.workStructureType != STRUCTURE_TYPE.HOSPICE)
		{
			workPlaceStructure.RemoveAssignedWorker(base.owner);
		}
	}

	public LocationStructure GetPreferredBasicResourceStructure(Character p_character)
	{
		if (p_character.homeSettlement == null)
		{
			return null;
		}
		if (p_character.faction == null)
		{
			return null;
		}
		if (!p_character.homeSettlement.HasBasicResourceProducingStructure())
		{
			return null;
		}
		if (HasWorkPlaceStructure() && workPlaceStructure.structureType.IsBasicResourceProducingStructureForFaction(p_character.faction.factionType.type))
		{
			bool flag = false;
			if (p_character.faction.factionType.type == FACTION_TYPE.Human_Empire)
			{
				flag = p_character.structureComponent.workPlaceStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.STONE_PILE);
			}
			else if (p_character.faction.factionType.type == FACTION_TYPE.Elven_Kingdom)
			{
				flag = p_character.structureComponent.workPlaceStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.WOOD_PILE);
			}
			else if (p_character.faction.factionType.type == FACTION_TYPE.Demon_Cult || p_character.faction.factionType.type == FACTION_TYPE.Lycan_Clan || p_character.faction.factionType.type == FACTION_TYPE.Vampire_Clan)
			{
				flag = p_character.structureComponent.workPlaceStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.STONE_PILE) || p_character.structureComponent.workPlaceStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.WOOD_PILE);
			}
			if (flag)
			{
				return p_character.structureComponent.workPlaceStructure;
			}
		}
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		if (p_character.faction.factionType.type == FACTION_TYPE.Human_Empire || p_character.faction.factionType.type == FACTION_TYPE.Demon_Cult || p_character.faction.factionType.type == FACTION_TYPE.Lycan_Clan || p_character.faction.factionType.type == FACTION_TYPE.Vampire_Clan)
		{
			List<LocationStructure> structuresOfType = p_character.homeSettlement.GetStructuresOfType(STRUCTURE_TYPE.MINE);
			if (structuresOfType != null)
			{
				list.AddRange(structuresOfType);
			}
		}
		if (p_character.faction.factionType.type == FACTION_TYPE.Elven_Kingdom || p_character.faction.factionType.type == FACTION_TYPE.Demon_Cult || p_character.faction.factionType.type == FACTION_TYPE.Lycan_Clan || p_character.faction.factionType.type == FACTION_TYPE.Vampire_Clan)
		{
			List<LocationStructure> structuresOfType2 = p_character.homeSettlement.GetStructuresOfType(STRUCTURE_TYPE.LUMBERYARD);
			if (structuresOfType2 != null)
			{
				list.AddRange(structuresOfType2);
			}
		}
		if (list.Count > 0)
		{
			list.Shuffle();
			for (int i = 0; i < list.Count; i++)
			{
				ManMadeStructure manMadeStructure = list[i] as ManMadeStructure;
				bool flag2 = false;
				if (p_character.faction.factionType.type == FACTION_TYPE.Human_Empire)
				{
					flag2 = manMadeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.STONE_PILE);
				}
				else if (p_character.faction.factionType.type == FACTION_TYPE.Elven_Kingdom)
				{
					flag2 = manMadeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.WOOD_PILE);
				}
				else if (p_character.faction.factionType.type == FACTION_TYPE.Demon_Cult || p_character.faction.factionType.type == FACTION_TYPE.Lycan_Clan || p_character.faction.factionType.type == FACTION_TYPE.Vampire_Clan)
				{
					flag2 = manMadeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.STONE_PILE) || manMadeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.WOOD_PILE);
				}
				if (flag2)
				{
					RuinarchListPool<LocationStructure>.Release(list);
					return manMadeStructure;
				}
			}
		}
		RuinarchListPool<LocationStructure>.Release(list);
		return null;
	}

	public void LoadReferences(SaveDataCharacterStructureComponent data)
	{
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		_ = workPlaceStructure;
	}
}
