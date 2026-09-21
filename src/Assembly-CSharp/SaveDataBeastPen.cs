using Inner_Maps.Location_Structures;

public class SaveDataBeastPen : SaveDataManMadeStructure
{
	public GameDate spawnDate;

	public SUMMON_TYPE monsterType;

	public string monsterClassName;

	public int minLimit;

	public int maxLimit;

	public string pluralizedMonsterTypeString;

	public override void Save(LocationStructure locationStructure)
	{
		base.Save(locationStructure);
		BeastPen beastPen = locationStructure as BeastPen;
		spawnDate = beastPen.spawnDate;
		monsterType = beastPen.monsterType;
		monsterClassName = beastPen.monsterClassName;
		minLimit = beastPen.minLimit;
		maxLimit = beastPen.maxLimit;
		pluralizedMonsterTypeString = beastPen.pluralizedMonsterTypeString;
	}
}
