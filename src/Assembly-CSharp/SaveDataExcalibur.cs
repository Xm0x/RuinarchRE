using System.Collections.Generic;

public class SaveDataExcalibur : SaveDataTileObject
{
	public List<int> finishedCharacters;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		Excalibur excalibur = tileObject as Excalibur;
		finishedCharacters = new List<int>(excalibur.finishedCharacters);
	}
}
