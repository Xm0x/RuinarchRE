using System;

[Serializable]
public class SaveDataSummon : SaveDataCharacter
{
	public SUMMON_TYPE summonType;

	public override void Save(Character data)
	{
		base.Save(data);
		if (data is Summon summon)
		{
			summonType = summon.summonType;
		}
	}

	public override Character Load()
	{
		return CharacterManager.Instance.CreateNewSummon(this);
	}
}
