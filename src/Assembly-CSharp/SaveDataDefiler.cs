using Inner_Maps.Location_Structures;

public class SaveDataDefiler : SaveDataDemonicStructure
{
	public bool hasVampirismBefore;

	public bool hasSpawnNecronomiconBefore;

	public override void Save(LocationStructure structure)
	{
		base.Save(structure);
		Defiler defiler = structure as Defiler;
		hasVampirismBefore = defiler.hasVampirismBefore;
		hasSpawnNecronomiconBefore = defiler.hasSpawnNecronomiconBefore;
	}
}
