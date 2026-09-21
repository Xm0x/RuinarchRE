namespace Inner_Maps.Location_Structures;

public class MagicAcademyStructureObject : LocationStructureObject
{
	protected override void PreplacedObjectProcessing(StructureTemplateObjectData preplacedObj, LocationGridTile tile, LocationStructure structure, TileObject newTileObject)
	{
		base.PreplacedObjectProcessing(preplacedObj, tile, structure, newTileObject);
		if (newTileObject is TreeObject)
		{
			newTileObject.mapVisual.UpdateTileObjectVisual(newTileObject);
		}
	}
}
