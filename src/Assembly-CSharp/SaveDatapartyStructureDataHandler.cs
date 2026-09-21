public class SaveDatapartyStructureDataHandler : SaveData<PartyStructureDataHandler>
{
	public int prismAvailableSummonCount;

	public int maraudAvailableSummonCount;

	public int kennelAvailableSummonCount;

	public int prisonerAvailableSummonCount;

	public int snatchObjectSummonCount;

	public override void Save(PartyStructureDataHandler data)
	{
		base.Save(data);
		prismAvailableSummonCount = data.prismAvailableSummonCount;
		maraudAvailableSummonCount = data.maraudAvailableSummonCount;
		kennelAvailableSummonCount = data.kennelAvailableSummonCount;
		prisonerAvailableSummonCount = data.prisonerAvailableSummonCount;
		snatchObjectSummonCount = data.snatchObjectSummonCount;
	}

	public override PartyStructureDataHandler Load()
	{
		return new PartyStructureDataHandler(this);
	}
}
