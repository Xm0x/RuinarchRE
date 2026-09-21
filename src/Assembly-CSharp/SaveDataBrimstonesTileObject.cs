using System;

[Serializable]
public class SaveDataBrimstonesTileObject : SaveDataAOEPlayerSpellTileObject
{
	public bool isBrimstoneCastedByPlayer;

	public int remainingBrimstoneDuration;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		BrimstonesTileObject brimstonesTileObject = tileObject as BrimstonesTileObject;
		remainingBrimstoneDuration = brimstonesTileObject.currentBrimstonesDuration;
		isBrimstoneCastedByPlayer = brimstonesTileObject.isBrimstoneCastedByPlayer;
	}
}
