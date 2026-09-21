public class SeducerSummon : Summon
{
	public SeducerSummon(SUMMON_TYPE type, GENDER gender, string className)
		: base(type, className, RACE.LESSER_DEMON, gender)
	{
	}

	public SeducerSummon(SaveDataSummon data)
		: base(data)
	{
	}
}
