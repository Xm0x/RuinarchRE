using Inner_Maps;
using UtilityScripts;

public class Unicorn : Summon
{
	public const string ClassName = "Unicorn";

	public Unicorn()
		: base(SUMMON_TYPE.Unicorn, "Unicorn", RACE.UNICORN, Utilities.GetRandomGender())
	{
	}

	public Unicorn(string className)
		: base(SUMMON_TYPE.Unicorn, className, RACE.UNICORN, Utilities.GetRandomGender())
	{
	}

	public Unicorn(SaveDataSummon data)
		: base(data)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		base.piercingAndResistancesComponent.AdjustSecondaryResistances(40f);
		base.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Physical, 40f);
	}

	protected override void AfterDeath(LocationGridTile deathTileLocation)
	{
		base.AfterDeath(deathTileLocation);
		if (base.faction != null && !base.faction.IsFriendlyWith(PlayerManager.Instance.player.playerFaction))
		{
			Area area = deathTileLocation?.area;
			if (area != null)
			{
				SummonVengefulGhosts(area);
			}
		}
	}

	public override bool Agitate(ref JobQueueItem p_agitateJob)
	{
		if (base.limiterComponent.IsIncapacitated())
		{
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Incapacitated);
			return false;
		}
		CreateAgitateLog(AGITATE_MESSAGE_TYPE.Agitate_Success);
		base.interruptComponent.TriggerInterrupt(INTERRUPT.Wide_Heal, this);
		return true;
	}

	private void SummonVengefulGhosts(Area p_area)
	{
		if (p_area == null)
		{
			return;
		}
		for (int i = 0; i < 4; i++)
		{
			LocationGridTile locationGridTile = p_area.GetRandomPassableTile();
			if (locationGridTile == null)
			{
				locationGridTile = p_area.gridTileComponent.GetRandomTile();
			}
			if (locationGridTile != null)
			{
				Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Vengeful_Ghost, base.faction, homeStructure: base.homeStructure, homeLocation: base.homeSettlement, homeRegion: GridMap.Instance.mainRegion, className: "", bypassIdeologyChecking: true);
				CharacterManager.Instance.PlaceSummonInitially(summon, locationGridTile);
				if (summon.homeStructure == null)
				{
					summon.SetTerritory(p_area);
				}
			}
		}
	}
}
