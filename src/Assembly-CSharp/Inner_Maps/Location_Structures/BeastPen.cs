using System;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public class BeastPen : ManMadeStructure
{
	public GameDate spawnDate { get; private set; }

	public SUMMON_TYPE monsterType { get; private set; }

	public string monsterClassName { get; private set; }

	public int minLimit { get; private set; }

	public int maxLimit { get; private set; }

	public string pluralizedMonsterTypeString { get; private set; }

	public override Type serializedData => typeof(SaveDataBeastPen);

	public BeastPen(Region location)
		: base(STRUCTURE_TYPE.BEAST_PEN, location)
	{
		SetMaxHPAndReset(3000);
	}

	public BeastPen(Region location, SaveDataBeastPen data)
		: base(location, data)
	{
		SetMaxHP(3000);
		spawnDate = data.spawnDate;
		monsterType = data.monsterType;
		monsterClassName = data.monsterClassName;
		minLimit = data.minLimit;
		maxLimit = data.maxLimit;
		pluralizedMonsterTypeString = data.pluralizedMonsterTypeString;
	}

	public override void LoadStructureSecondWaveInMainThread(SaveDataLocationStructure saveDataLocationStructure)
	{
		base.LoadStructureSecondWaveInMainThread(saveDataLocationStructure);
		SchedulingManager.Instance.AddEntry(spawnDate, SpawnMonsters, this);
	}

	public override void OnBuiltNewStructure()
	{
		base.OnBuiltNewStructure();
		OnBuiltBeastPen();
	}

	private void OnBuiltBeastPen()
	{
		if (monsterType != SUMMON_TYPE.None || !string.IsNullOrEmpty(monsterClassName))
		{
			return;
		}
		MonsterMigrationBiomeAtomizedData monsterMigrationBiomeAtomizedData = LandmarkManager.Instance.GetStructureData(base.structureType)?.GetRandomMonsterToSpawn();
		if (monsterMigrationBiomeAtomizedData != null)
		{
			monsterType = monsterMigrationBiomeAtomizedData.monsterType;
			monsterClassName = monsterMigrationBiomeAtomizedData.monsterClassName;
			minLimit = monsterMigrationBiomeAtomizedData.minRange;
			maxLimit = monsterMigrationBiomeAtomizedData.maxRange;
			if (monsterType != SUMMON_TYPE.None)
			{
				string s = monsterType.ToStringEnumWithSpace();
				pluralizedMonsterTypeString = Utilities.PluralizeString(s);
			}
			else
			{
				pluralizedMonsterTypeString = Utilities.PluralizeString(monsterClassName);
			}
		}
		ScheduleNextSpawn();
	}

	private void ScheduleNextSpawn()
	{
		spawnDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(24));
		SchedulingManager.Instance.AddEntry(spawnDate, SpawnMonsters, this);
	}

	private void SpawnMonsters()
	{
		if (base.hasBeenDestroyed)
		{
			return;
		}
		if (base.settlementLocation != null && base.settlementLocation.owner != null && CanSpawnNewMonsters())
		{
			int num = GameUtilities.RandomBetweenTwoNumbers(minLimit, maxLimit);
			for (int i = 0; i < num; i++)
			{
				LocationGridTile locationGridTile = GetRandomPassableTile();
				if (locationGridTile == null)
				{
					locationGridTile = GetRandomTile();
				}
				if (monsterType != SUMMON_TYPE.None)
				{
					Summon summon = CharacterManager.Instance.CreateNewSummon(monsterType, base.settlementLocation.owner, base.settlementLocation, base.region, this, "", bypassIdeologyChecking: true);
					CharacterManager.Instance.PlaceSummonInitially(summon, locationGridTile);
				}
				else if (monsterClassName == "Ratman")
				{
					CharacterManager.Instance.GenerateRatman(locationGridTile, locationGridTile.structure);
				}
			}
		}
		ScheduleNextSpawn();
	}

	private bool CanSpawnNewMonsters()
	{
		if (GetNumberOfResidentsThatIsAlive() < maxLimit)
		{
			return true;
		}
		return false;
	}
}
