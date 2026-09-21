namespace Inner_Maps.Location_Structures;

public class MushroomHavenStructureObject : LocationStructureObject
{
	protected override void PreplacedObjectProcessing(StructureTemplateObjectData preplacedObj, LocationGridTile tile, LocationStructure structure, TileObject newTileObject)
	{
		base.PreplacedObjectProcessing(preplacedObj, tile, structure, newTileObject);
		if (newTileObject is TreeObject || newTileObject is Flower)
		{
			newTileObject.mapVisual.UpdateTileObjectVisual(newTileObject);
		}
	}
}
