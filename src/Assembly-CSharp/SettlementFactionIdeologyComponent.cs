using System.Collections.Generic;
using UtilityScripts;

public class SettlementFactionIdeologyComponent : NPCSettlementComponent
{
	public GameDate scheduleDateForProcessingOfEvents { get; private set; }

	public Dictionary<FactionIdeology, int> daysPassedPerIdeology { get; private set; }

	public SettlementFactionIdeologyComponent()
	{
		daysPassedPerIdeology = new Dictionary<FactionIdeology, int>(10);
	}

	public SettlementFactionIdeologyComponent(SaveDataSettlementFactionIdeologyComponent data)
	{
		daysPassedPerIdeology = new Dictionary<FactionIdeology, int>(10);
		scheduleDateForProcessingOfEvents = data.scheduleDateForProcessingOfEvents;
	}

	public void OnChangeFactionOwner(Faction p_previousOwner, Faction p_newOwner)
	{
		daysPassedPerIdeology.Clear();
	}

	public void OnRemoveFactionIdeology(FactionIdeology p_ideology, Faction p_faction)
	{
		if (base.owner.owner == p_faction && daysPassedPerIdeology.ContainsKey(p_ideology))
		{
			daysPassedPerIdeology.Remove(p_ideology);
		}
	}

	public void InitialScheduleProcessingOfEvents()
	{
		if (base.owner.locationType == LOCATION_TYPE.VILLAGE || base.owner.locationType == LOCATION_TYPE.PSEUDO_VILLAGE)
		{
			int ticksBasedOnHour = GameManager.Instance.GetTicksBasedOnHour(8);
			int ticksBasedOnHour2 = GameManager.Instance.GetTicksBasedOnHour(10);
			int ticks = GameUtilities.RandomBetweenTwoNumbers(ticksBasedOnHour, ticksBasedOnHour2);
			GameDate gameDate = GameManager.Instance.Today().AddDays(1);
			gameDate.SetTicks(ticks);
			scheduleDateForProcessingOfEvents = gameDate;
			SchedulingManager.Instance.AddEntry(scheduleDateForProcessingOfEvents, ProcessingOfEvents, base.owner);
		}
	}

	private void RescheduleProcessingOfEvents()
	{
		scheduleDateForProcessingOfEvents = GameManager.Instance.Today().AddDays(1);
		SchedulingManager.Instance.AddEntry(scheduleDateForProcessingOfEvents, ProcessingOfEvents, base.owner);
	}

	private void ProcessingOfEvents()
	{
		if (base.owner.owner != null)
		{
			List<FactionIdeology> currentIdeologies = base.owner.owner.ideologyComponent.currentIdeologies;
			for (int i = 0; i < currentIdeologies.Count; i++)
			{
				FactionIdeology factionIdeology = currentIdeologies[i];
				if (CanTriggerSettlementEvent(factionIdeology))
				{
					factionIdeology.TriggerIdeologySettlementEvent(base.owner);
				}
				IncreaseDaysPassed(factionIdeology);
			}
		}
		RescheduleProcessingOfEvents();
	}

	private bool CanTriggerSettlementEvent(FactionIdeology p_ideology)
	{
		if (p_ideology.daysIntervalSettlementEvent <= 0)
		{
			return true;
		}
		if (!daysPassedPerIdeology.ContainsKey(p_ideology))
		{
			daysPassedPerIdeology.Add(p_ideology, 0);
		}
		if (daysPassedPerIdeology[p_ideology] % p_ideology.daysIntervalSettlementEvent == 0)
		{
			return true;
		}
		return false;
	}

	private void IncreaseDaysPassed(FactionIdeology p_ideology)
	{
		if (daysPassedPerIdeology.ContainsKey(p_ideology))
		{
			daysPassedPerIdeology[p_ideology]++;
		}
	}

	public void LoadReferences(SaveDataSettlementFactionIdeologyComponent data)
	{
		if (base.owner.locationType == LOCATION_TYPE.VILLAGE || base.owner.locationType == LOCATION_TYPE.PSEUDO_VILLAGE)
		{
			SchedulingManager.Instance.AddEntry(scheduleDateForProcessingOfEvents, ProcessingOfEvents, base.owner);
		}
		if (data.ideologies == null || data.ideologies.Length == 0)
		{
			return;
		}
		for (int i = 0; i < data.ideologies.Length; i++)
		{
			FACTION_IDEOLOGY p_ideologyType = data.ideologies[i];
			int value = data.daysPassed[i];
			FactionIdeology factionIdeology = base.owner.owner.ideologyComponent.GetFactionIdeology(p_ideologyType);
			if (factionIdeology != null)
			{
				daysPassedPerIdeology.Add(factionIdeology, value);
			}
		}
	}
}
