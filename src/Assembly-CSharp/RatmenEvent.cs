using Inner_Maps;
using Locations.Settlements;

public class RatmenEvent : PrismEvent
{
	public int numberOfAbandonedVillages { get; private set; }

	public int numberOfAlivePlaguedVillagers { get; private set; }

	public RatmenEvent(PrismEventData p_data)
		: base(p_data)
	{
		base.requirements = new PrismEventRequirement[3]
		{
			new PrismEventRequirement("Ratmen_Event_Requirement_1", IsVillageRequirementSatisfied),
			new PrismEventRequirement("Ratmen_Event_Requirement_2", IsPlaguedVillagersRequirementSatisfied),
			new PrismEventRequirement("Ratmen_Event_Requirement_3", IsRatmenFactionRequirementSatisfied)
		};
	}

	public void AdjustNumberOfAbandonedVillages(int p_amount)
	{
		numberOfAbandonedVillages += p_amount;
	}

	public void SetNumberOfAbandonedVillages(int p_amount)
	{
		numberOfAbandonedVillages = p_amount;
	}

	public void AdjustNumberOfPlaguedVillagers(int p_amount)
	{
		numberOfAlivePlaguedVillagers += p_amount;
	}

	public void SetNumberOfPlaguedVillagers(int p_amount)
	{
		numberOfAlivePlaguedVillagers = p_amount;
	}

	private void Invade()
	{
		if (FactionManager.Instance.ratmenFaction == null)
		{
			FactionManager.Instance.CreateRatmenFaction();
		}
		Faction ratmenFaction = FactionManager.Instance.ratmenFaction;
		Region mainRegion = GridMap.Instance.mainRegion;
		BaseSettlement baseSettlement = null;
		for (int i = 0; i < mainRegion.settlementsInRegion.Count; i++)
		{
			BaseSettlement baseSettlement2 = mainRegion.settlementsInRegion[i];
			if (baseSettlement2.locationType == LOCATION_TYPE.VILLAGE && !baseSettlement2.HasResidents())
			{
				baseSettlement = baseSettlement2;
				break;
			}
		}
		for (int j = 0; j < 5; j++)
		{
			Character character = CharacterManager.Instance.CreateNewCharacter("Ratman", RACE.RATMAN, GENDER.MALE, ratmenFaction ?? FactionManager.Instance.wildMonsterFaction, baseSettlement, mainRegion);
			character.CreateMarker();
			LocationGridTile randomPassableTile = baseSettlement.GetRandomPassableTile();
			character.InitialCharacterPlacement(randomPassableTile);
		}
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Prism", "PrismEvents_Table", "Ratmen_Migration_Effect", LOG_TAG.Major);
		log.AddLogToDatabase();
		PlayerManager.Instance?.player?.ShowNotificationFromPlayer(log, releaseLogAfter: true);
	}

	private bool IsVillageRequirementSatisfied()
	{
		return numberOfAbandonedVillages > 0;
	}

	private bool IsPlaguedVillagersRequirementSatisfied()
	{
		return numberOfAlivePlaguedVillagers >= 3;
	}

	private bool IsRatmenFactionRequirementSatisfied()
	{
		if (FactionManager.Instance.ratmenFaction == null)
		{
			return true;
		}
		if (!FactionManager.Instance.ratmenFaction.HasAliveMember())
		{
			return true;
		}
		return false;
	}

	protected override void TriggerEventBase()
	{
		base.TriggerEventBase();
		Invade();
	}
}
