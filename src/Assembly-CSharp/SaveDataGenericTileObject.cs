using Inner_Maps;

public class SaveDataGenericTileObject : SaveDataTileObject
{
	public string blueprintOnTileName;

	public GameDate blueprintExpiryDate;

	public GameDate blueprintAutoBuildDate;

	public bool isCurrentlyBuilding;

	public string selfBuildingStructureSettlement;

	public bool hasStructureConnector;

	public bool hasStartedBuildingBlueprintOnTile;

	public override void Save(TileObject data)
	{
		base.Save(data);
		GenericTileObject genericTileObject = data as GenericTileObject;
		if (!string.IsNullOrEmpty(genericTileObject.blueprintTemplateName))
		{
			blueprintOnTileName = genericTileObject.blueprintTemplateName.Replace("(Clone)", "");
			blueprintExpiryDate = genericTileObject.blueprintExpiryDate;
			isCurrentlyBuilding = genericTileObject.isCurrentlyBuilding;
			blueprintAutoBuildDate = genericTileObject.selfBuildingStructureDueDate;
			if (genericTileObject.selfBuildingStructureSettlement != null)
			{
				selfBuildingStructureSettlement = genericTileObject.selfBuildingStructureSettlement.persistentID;
			}
		}
		hasStructureConnector = genericTileObject.structureConnector != null;
		hasStartedBuildingBlueprintOnTile = genericTileObject.hasStartedBuildingBlueprintOnTile;
	}

	public override TileObject Load()
	{
		return InnerMapManager.Instance.LoadTileObject<GenericTileObject>(this);
	}
}
