using Inner_Maps;
using UtilityScripts;

public class BanditsEvent : PrismEvent
{
	public int numberOfAliveVagrantsWithSeriousOrHeinousCrime { get; private set; }

	public BanditsEvent(PrismEventData p_data)
		: base(p_data)
	{
		base.requirements = new PrismEventRequirement[2]
		{
			new PrismEventRequirement("Bandits_Event_Requirement_1", IsVagrantsRequirementSatisfied),
			new PrismEventRequirement("Bandits_Event_Requirement_2", IsBanditFactionRequirementSatisfied)
		};
	}

	public void AdjustNumberOfAliveVagrantsWithSeriousOrHeinousCrime(int p_amount)
	{
		numberOfAliveVagrantsWithSeriousOrHeinousCrime += p_amount;
		if (numberOfAliveVagrantsWithSeriousOrHeinousCrime < 0)
		{
			numberOfAliveVagrantsWithSeriousOrHeinousCrime = 0;
		}
	}

	public void SetNumberOfAliveVagrantsWithSeriousOrHeinousCrime(int p_amount)
	{
		numberOfAliveVagrantsWithSeriousOrHeinousCrime = p_amount;
	}

	private void GenerateBanditsAndBanditFaction()
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Prism", "PrismEvents_Table", "Bandits_Migration_Effect", LOG_TAG.Major);
		log.AddLogToDatabase();
		PlayerManager.Instance?.player?.ShowNotificationFromPlayer(log, releaseLogAfter: true);
		LocationGridTile randomPassableEdgeTile = GridMap.Instance.mainRegion.innerMap.GetRandomPassableEdgeTile();
		RACE race = RACE.HUMANS;
		if (GameUtilities.RollChance(50))
		{
			race = RACE.ELVES;
		}
		for (int i = 0; i < 5; i++)
		{
			CharacterClass randomLowTierCombatant = CharacterManager.Instance.GetRandomLowTierCombatant();
			GENDER randomGender = Utilities.GetRandomGender();
			Character character = CharacterManager.Instance.CreateNewCharacter(randomLowTierCombatant.className, race, randomGender);
			character.CreateMarker();
			character.InitialCharacterPlacement(randomPassableEdgeTile);
			FactionManager.Instance.JoinOrCreateBanditFaction(character);
		}
	}

	private bool IsVagrantsRequirementSatisfied()
	{
		return numberOfAliveVagrantsWithSeriousOrHeinousCrime >= 4;
	}

	private bool IsBanditFactionRequirementSatisfied()
	{
		if (FactionManager.Instance.banditFaction == null)
		{
			return true;
		}
		if (!FactionManager.Instance.banditFaction.HasAliveMember())
		{
			return true;
		}
		return false;
	}

	protected override void TriggerEventBase()
	{
		base.TriggerEventBase();
		GenerateBanditsAndBanditFaction();
	}
}
