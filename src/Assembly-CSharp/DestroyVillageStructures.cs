using System;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Object_Pools;

public class DestroyVillageStructures : GoalTask
{
	public int neededDestroyStructureCount { get; private set; }

	public int currentDestroyStructureCount { get; private set; }

	public override Type serializedData => typeof(SaveDataDestroyVillageStructures);

	public DestroyVillageStructures()
	{
		switch (WorldSettings.Instance.worldSettingsData.mapSettings.mapSize)
		{
		case MAP_SIZE.Small:
			neededDestroyStructureCount = 10;
			break;
		case MAP_SIZE.Medium:
			neededDestroyStructureCount = 15;
			break;
		case MAP_SIZE.Large:
			neededDestroyStructureCount = 15;
			break;
		case MAP_SIZE.Extra_Large:
			neededDestroyStructureCount = 15;
			break;
		}
		UpdateTaskName();
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Destroy_Village_Structures_Task_Tooltip");
	}

	public DestroyVillageStructures(SaveDataDestroyVillageStructures p_data)
		: base(p_data)
	{
		neededDestroyStructureCount = p_data.neededDestroyStructureCount;
		currentDestroyStructureCount = p_data.currentDestroyStructureCount;
	}

	public override void StartTask()
	{
		Messenger.AddListener<LocationStructure, BaseSettlement>(StructureSignals.STRUCTURE_DESTROYED_BY_PLAYER, OnStructureDestroyedByPlayer);
	}

	protected override void EndTask()
	{
		Messenger.RemoveListener<LocationStructure, BaseSettlement>(StructureSignals.STRUCTURE_DESTROYED_BY_PLAYER, OnStructureDestroyedByPlayer);
	}

	protected override void ReevaluateLocalizedTexts()
	{
		UpdateTaskName();
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Destroy_Village_Structures_Task_Tooltip");
	}

	private void UpdateTaskName()
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Goals", "Goals_Table", "Destroy_Village_Structures_Task");
		log.AddToFillers(null, currentDestroyStructureCount + "/" + neededDestroyStructureCount, LOG_IDENTIFIER.STRING_1);
		log.FinalizeText();
		SetTaskName(log.rawText);
		LogPool.Release(log);
	}

	private void OnStructureDestroyedByPlayer(LocationStructure p_structure, BaseSettlement p_settlementLocation)
	{
		if (p_structure.structureType.IsVillageStructure() && p_settlementLocation != null && p_settlementLocation.owner != null && (p_settlementLocation.owner.factionType.type == FACTION_TYPE.Human_Empire || p_settlementLocation.owner.factionType.type == FACTION_TYPE.Elven_Kingdom))
		{
			currentDestroyStructureCount++;
			UpdateTaskName();
			if (currentDestroyStructureCount >= neededDestroyStructureCount)
			{
				CompleteTask();
			}
		}
	}
}
