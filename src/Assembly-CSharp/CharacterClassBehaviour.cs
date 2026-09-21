using Inner_Maps.Location_Structures;

public abstract class CharacterClassBehaviour
{
	public abstract bool TryDoBehaviour(Character p_character, ref JobQueueItem p_producedJob, ref string log);

	protected bool MagicUserBehaviour(Character character, ref string log, ref JobQueueItem producedJob)
	{
		if (character.faction != null && character.faction.ideologyComponent.HasIdeology(FACTION_IDEOLOGY.Mage_Guild))
		{
			if (BuildMagicAcademyForMageGuild(character, ref log, ref producedJob))
			{
				return true;
			}
			if (MagicTraining(character, ref log, ref producedJob))
			{
				return true;
			}
		}
		else if (character.race == RACE.ELVES && character.homeSettlement != null)
		{
			if (ChanceData.RollChance(CHANCE_TYPE.Place_Magic_Academy) && character.faction != null && character.faction.factionType.type == FACTION_TYPE.Elven_Kingdom && (character.TryGetTalentLevel(CHARACTER_TALENT.Combat_Magic) >= 4 || character.TryGetTalentLevel(CHARACTER_TALENT.Healing_Magic) >= 4) && !GridMap.Instance.mainRegion.HasStructure(STRUCTURE_TYPE.MAGIC_ACADEMY) && !GridMap.Instance.mainRegion.HasStructureBlueprint(STRUCTURE_TYPE.MAGIC_ACADEMY) && character.jobComponent.TryCreatePlaceBlueprintJob(character.faction.factionType.type, STRUCTURE_TYPE.MAGIC_ACADEMY, character, character.homeSettlement, out producedJob, ref log))
			{
				return true;
			}
			if (MagicTraining(character, ref log, ref producedJob))
			{
				return true;
			}
		}
		producedJob = null;
		return false;
	}

	private bool MagicTraining(Character character, ref string log, ref JobQueueItem producedJob)
	{
		if (ChanceData.RollChance(CHANCE_TYPE.Train_Magic_Academy) && character.homeSettlement.HasStructure(STRUCTURE_TYPE.MAGIC_ACADEMY))
		{
			LocationStructure randomStructureOfType = character.homeSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.MAGIC_ACADEMY);
			if ((character.TryGetTalentLevel(CHARACTER_TALENT.Combat_Magic) < 5 || character.TryGetTalentLevel(CHARACTER_TALENT.Healing_Magic) < 5) && (randomStructureOfType.HasTileObjectOfType(TILE_OBJECT_TYPE.TRAINING_DUMMY) || randomStructureOfType.HasTileObjectOfType(TILE_OBJECT_TYPE.DESK) || randomStructureOfType.HasTileObjectOfType(TILE_OBJECT_TYPE.WATER_BASIN)) && character.jobComponent.TriggerTrainMagic(character, randomStructureOfType, out producedJob))
			{
				return true;
			}
		}
		return false;
	}

	private bool BuildMagicAcademyForMageGuild(Character character, ref string log, ref JobQueueItem producedJob)
	{
		if (ChanceData.RollChance(CHANCE_TYPE.Place_Magic_Academy) && !character.faction.HasStructure(STRUCTURE_TYPE.MAGIC_ACADEMY) && !character.faction.HasStructureBlueprint(STRUCTURE_TYPE.MAGIC_ACADEMY) && character.jobComponent.TryCreatePlaceBlueprintJob(character.faction.factionType.type, STRUCTURE_TYPE.MAGIC_ACADEMY, character, character.homeSettlement, out producedJob, ref log))
		{
			return true;
		}
		return false;
	}

	protected bool PhysicalUserBehaviour(Character character, ref string log, ref JobQueueItem producedJob)
	{
		if (character.faction != null && character.faction.ideologyComponent.HasIdeology(FACTION_IDEOLOGY.Mage_Guild))
		{
			if (BuildMagicAcademyForMageGuild(character, ref log, ref producedJob))
			{
				return true;
			}
		}
		else if (character.race == RACE.HUMANS && character.homeSettlement != null)
		{
			if (ChanceData.RollChance(CHANCE_TYPE.Place_Barracks) && character.faction != null && character.faction.factionType.type == FACTION_TYPE.Human_Empire && character.TryGetTalentLevel(CHARACTER_TALENT.Martial_Arts) >= 4 && !GridMap.Instance.mainRegion.HasStructure(STRUCTURE_TYPE.BARRACKS) && !GridMap.Instance.mainRegion.HasStructureBlueprint(STRUCTURE_TYPE.BARRACKS) && character.jobComponent.TryCreatePlaceBlueprintJob(character.faction.factionType.type, STRUCTURE_TYPE.BARRACKS, character, character.homeSettlement, out producedJob, ref log))
			{
				return true;
			}
			if (ChanceData.RollChance(CHANCE_TYPE.Train_Barracks) && character.homeSettlement.HasStructure(STRUCTURE_TYPE.BARRACKS))
			{
				LocationStructure randomStructureOfType = character.homeSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.BARRACKS);
				if (character.TryGetTalentLevel(CHARACTER_TALENT.Martial_Arts) < 5 && (randomStructureOfType.HasTileObjectOfType(TILE_OBJECT_TYPE.TRAINING_DUMMY) || randomStructureOfType.HasTileObjectOfType(TILE_OBJECT_TYPE.ARCHERY_TARGET)) && character.jobComponent.TriggerTrainPhysicalCombat(character, randomStructureOfType, out producedJob))
				{
					return true;
				}
			}
		}
		producedJob = null;
		return false;
	}
}
