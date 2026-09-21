using System;

public class SpawnLycanClan : GoalTask
{
	public override Type serializedData => typeof(SaveDataSpawnLycanClan);

	public SpawnLycanClan()
	{
		base.taskName = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Spawn_Lycan_Clan_Task");
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Spawn_Lycan_Clan_Task_Tooltip");
	}

	public SpawnLycanClan(SaveDataSpawnLycanClan p_data)
		: base(p_data)
	{
	}

	public override void StartTask()
	{
		Messenger.AddListener<Faction>(FactionSignals.FACTION_CREATED, OnFactionCreated);
		Messenger.AddListener<Faction, Character>(FactionSignals.CREATE_FACTION_INTERRUPT, OnFactionCreated);
	}

	protected override void EndTask()
	{
		Messenger.RemoveListener<Faction>(FactionSignals.FACTION_CREATED, OnFactionCreated);
		Messenger.RemoveListener<Faction, Character>(FactionSignals.CREATE_FACTION_INTERRUPT, OnFactionCreated);
	}

	protected override void ReevaluateLocalizedTexts()
	{
		base.taskName = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Spawn_Lycan_Clan_Task");
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Spawn_Lycan_Clan_Task_Tooltip");
	}

	private void OnFactionCreated(Faction p_faction)
	{
		if (p_faction.factionType.type == FACTION_TYPE.Lycan_Clan)
		{
			CompleteTask();
		}
	}

	private void OnFactionCreated(Faction p_faction, Character p_character)
	{
		if (p_faction.factionType.type == FACTION_TYPE.Lycan_Clan)
		{
			CompleteTask();
		}
	}
}
