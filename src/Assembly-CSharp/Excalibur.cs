using System;
using System.Collections.Generic;
using Inner_Maps;

public class Excalibur : TileObject
{
	private HashSet<int> _finishedCharacters;

	public HashSet<int> finishedCharacters => _finishedCharacters;

	public override Type serializedData => typeof(SaveDataExcalibur);

	public Excalibur()
	{
		Initialize(TILE_OBJECT_TYPE.EXCALIBUR);
		base.traitContainer.AddTrait(this, "Indestructible");
		AddAdvertisedAction(INTERACTION_TYPE.INSPECT);
		_finishedCharacters = new HashSet<int>();
	}

	public Excalibur(SaveDataExcalibur data)
		: base(data)
	{
		_finishedCharacters = new HashSet<int>(data.finishedCharacters);
	}

	public override void OnInspect(Character inspector)
	{
		base.OnInspect(inspector);
		AddFinishedCharacter(inspector);
		if (inspector.traitContainer.IsBlessed() && !inspector.traitContainer.HasTrait("Evil", "Treacherous", "Demon Cultist"))
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Tile Object", "TileObjectAlerts_Table", "Excalibur on_inspect_success", LOG_TAG.Life_Changes);
			log.AddToFillers(inspector, inspector.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddLogToDatabase(releaseLogAfter: true);
			UnlockSword(inspector);
		}
		else
		{
			Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Tile Object", "TileObjectAlerts_Table", "Excalibur on_inspect_fail", LOG_TAG.Work, null);
			log2.AddToFillers(inspector, inspector.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log2.AddLogToDatabase(releaseLogAfter: true);
		}
	}

	private void AddFinishedCharacter(Character character)
	{
		if (!_finishedCharacters.Contains(character.id))
		{
			_finishedCharacters.Add(character.id);
		}
	}

	public bool HasInspectedThis(Character character)
	{
		return _finishedCharacters.Contains(character.id);
	}

	protected override void DisconnectFromCharacter(Character p_character)
	{
		base.DisconnectFromCharacter(p_character);
		_finishedCharacters?.Remove(p_character.id);
	}

	private void UnlockSword(Character character)
	{
		LocationGridTile locationGridTile = gridTileLocation;
		locationGridTile.structure.RemovePOI(this);
		TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.EXCALIBUR_SWORD);
		locationGridTile.structure.AddPOI(tileObject, locationGridTile);
		character.PickUpItem(tileObject, changeCharacterOwnership: true);
		locationGridTile.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.ROCK), locationGridTile);
	}
}
