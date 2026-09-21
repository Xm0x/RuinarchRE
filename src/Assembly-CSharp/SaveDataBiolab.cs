using Inner_Maps.Location_Structures;

public class SaveDataBiolab : SaveDataDemonicStructure
{
	public GameDate replenishDate;

	public override void Save(LocationStructure structure)
	{
		base.Save(structure);
		Biolab biolab = structure as Biolab;
		replenishDate = biolab.replenishDate;
	}
}
