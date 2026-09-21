using UnityEngine;

namespace Inner_Maps.Location_Structures;

public class TortureChamberStructureObject : LocationStructureObject
{
	[Header("Torture Chamber")]
	[SerializeField]
	private Vector3Int entranceCoordinates;

	public LocationGridTile entrance { get; private set; }

	public void SetEntrance(InnerTileMap innerTileMap)
	{
		entrance = ConvertLocalPointInStructureToTile(entranceCoordinates, innerTileMap);
	}

	protected override void PreplacedObjectProcessing(StructureTemplateObjectData preplacedObj, LocationGridTile tile, LocationStructure structure, TileObject newTileObject)
	{
		base.PreplacedObjectProcessing(preplacedObj, tile, structure, newTileObject);
		if (newTileObject.tileObjectType == TILE_OBJECT_TYPE.TORTURE_CHAMBERS_TILE_OBJECT || newTileObject.tileObjectType == TILE_OBJECT_TYPE.STRUCTURE_BLOCKER_TILE_OBJECT)
		{
			structure.AddObjectAsDamageContributor(newTileObject);
		}
	}
}
