namespace Inner_Maps.Location_Structures;

public class PrimordialPoolStructureObject : LocationStructureObject
{
	protected override void PreplacedObjectProcessing(StructureTemplateObjectData preplacedObj, LocationGridTile tile, LocationStructure structure, TileObject newTileObject)
	{
		base.PreplacedObjectProcessing(preplacedObj, tile, structure, newTileObject);
		if (newTileObject.tileObjectType == TILE_OBJECT_TYPE.PRIMORDIAL_POOL_TILE_OBJECT)
		{
			structure.AddObjectAsDamageContributor(newTileObject);
		}
	}
}
