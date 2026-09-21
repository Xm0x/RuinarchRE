public class SaveDataDoorTileObject : SaveDataTileObject
{
	public bool isOpen;

	public override void Save(TileObject data)
	{
		base.Save(data);
		DoorTileObject doorTileObject = data as DoorTileObject;
		isOpen = doorTileObject.isOpen;
	}
}
