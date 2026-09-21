using System;
using Locations.Settlements;

[Serializable]
public class GolemMakers : FactionIdeology
{
	public GolemMakers()
		: base(FACTION_IDEOLOGY.Golem_Makers)
	{
		base.daysIntervalSettlementEvent = 0;
	}

	public override bool DoesCharacterFitIdeology(Character character)
	{
		return true;
	}

	public override bool DoesCharacterFitIdeology(PreCharacterData character)
	{
		return true;
	}

	protected override void SettlementEvent(BaseSettlement p_settlement)
	{
		base.SettlementEvent(p_settlement);
		if (CanSpawnNewGolem(p_settlement) && p_settlement is NPCSettlement nPCSettlement)
		{
			nPCSettlement.settlementJobTriggerComponent.TriggerCreateGolemJob();
		}
	}

	protected override bool CanCharacterDoIdeologyEventInternal(Character p_character)
	{
		RaceData raceData = RaceManager.Instance.GetRaceData(p_character.race);
		if (raceData.category != CHARACTER_CATEGORY.Humanoid && raceData.category != CHARACTER_CATEGORY.Undead)
		{
			return p_character.race.IsSapient();
		}
		return true;
	}

	private bool CanSpawnNewGolem(BaseSettlement p_settlement)
	{
		GetNumberOfAliveGolemAndSapientResidents(p_settlement, out var p_sapientCount, out var p_golemCount);
		int num = p_sapientCount / 3;
		if (p_golemCount <= num)
		{
			return true;
		}
		return false;
	}

	private void GetNumberOfAliveGolemAndSapientResidents(BaseSettlement p_settlement, out int p_sapientCount, out int p_golemCount)
	{
		p_sapientCount = 0;
		p_golemCount = 0;
		for (int i = 0; i < p_settlement.residents.Count; i++)
		{
			Character character = p_settlement.residents[i];
			if (!character.isDead)
			{
				if (character.race.IsSapient())
				{
					p_sapientCount++;
				}
				else if (character.race == RACE.GOLEM)
				{
					p_golemCount++;
				}
			}
		}
	}
}
