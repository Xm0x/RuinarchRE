using System;

[Serializable]
public class SaveDataTriton : SaveDataSummon
{
	public TileLocationSave tileLocationSave;

	public override void Save(Character data)
	{
		base.Save(data);
		if (data is Triton { spawnLocationTile: not null } triton)
		{
			tileLocationSave = new TileLocationSave(triton.spawnLocationTile);
		}
	}
}
