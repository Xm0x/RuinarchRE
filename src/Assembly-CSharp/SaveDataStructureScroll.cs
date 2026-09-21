public class SaveDataStructureScroll : SaveDataTileObject
{
	public STRUCTURE_TYPE structureType;

	public override void Save(TileObject data)
	{
		base.Save(data);
		StructureScroll structureScroll = data as StructureScroll;
		structureType = structureScroll.structureType;
	}
}
