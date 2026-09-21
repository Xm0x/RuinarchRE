using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public abstract class AOESpellTileObject : TileObject, GridTileEventDispatcher.ICharacterTileListener, GridTileEventDispatcher.ITileStructureListener
{
	protected List<LocationGridTile> affectedTiles;

	protected List<Character> allCharactersInsideAOE;

	protected Dictionary<LocationStructure, int> affectedStructures;

	protected abstract int effectRadius { get; }

	protected AOESpellTileObject(TILE_OBJECT_TYPE p_tileObjectType)
	{
		Initialize(p_tileObjectType, shouldAddCommonAdvertisements: false);
		base.traitContainer.AddTrait(this, "Immovable");
		base.traitContainer.RemoveTrait(this, "Flammable");
		base.hiddenComponent.SetIsHidden(this, state: true, affectAlpha: false);
		affectedTiles = new List<LocationGridTile>();
		allCharactersInsideAOE = new List<Character>();
		affectedStructures = new Dictionary<LocationStructure, int>();
	}

	protected AOESpellTileObject(SaveDataTileObject data)
		: base(data)
	{
		affectedTiles = new List<LocationGridTile>();
		allCharactersInsideAOE = new List<Character>();
		affectedStructures = new Dictionary<LocationStructure, int>();
	}

	public override bool OccupiesTile()
	{
		return false;
	}

	public override bool CanBeSelected()
	{
		return false;
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		gridTileLocation.parentMap.region.regionSpellsComponent.AddActiveSpellInRegion(gridTileLocation, this);
		gridTileLocation.PopulateTilesInRadius(affectedTiles, effectRadius, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
		for (int i = 0; i < affectedTiles.Count; i++)
		{
			LocationGridTile p_tile = affectedTiles[i];
			ProcessTileAffectedByAOESpell(p_tile);
		}
		PopulateInitialCharactersInsideAOE();
		PopulateInitialStructuresInsideAOE();
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		base.previousTile.parentMap.region.regionSpellsComponent.RemoveActiveSpellInRegion(base.previousTile, this);
		for (int i = 0; i < affectedTiles.Count; i++)
		{
			LocationGridTile p_tile = affectedTiles[i];
			ProcessTileNoLongerAffectedByAOESpell(p_tile);
		}
		allCharactersInsideAOE = null;
		affectedTiles = null;
		affectedStructures = null;
	}

	protected override void DisconnectFromCharacter(Character p_character)
	{
		base.DisconnectFromCharacter(p_character);
		allCharactersInsideAOE?.Remove(p_character);
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		if (allCharactersInsideAOE != null)
		{
			allCharactersInsideAOE.Contains(p_character);
		}
	}

	protected virtual void ProcessTileAffectedByAOESpell(LocationGridTile p_tile)
	{
		p_tile.tileObjectComponent.AddAffectedByAOETileObject(this);
		p_tile.eventDispatcher.SubscribeToCharacterTileEvents(this);
	}

	protected virtual void ProcessTileNoLongerAffectedByAOESpell(LocationGridTile p_tile)
	{
		p_tile.tileObjectComponent.RemoveAffectedByAOETileObject(this);
		p_tile.eventDispatcher.UnsubscribeToCharacterTileEvents(this);
	}

	public virtual void OnCharacterEnteredTile(Character p_character, LocationGridTile p_tile)
	{
		AddCharacterInsideAOE(p_character);
	}

	public virtual void OnCharacterLeftTile(Character p_character, LocationGridTile p_tile)
	{
		RemoveCharacterInsideAOE(p_character);
	}

	private void PopulateInitialCharactersInsideAOE()
	{
		for (int i = 0; i < affectedTiles.Count; i++)
		{
			LocationGridTile locationGridTile = affectedTiles[i];
			for (int j = 0; j < locationGridTile.charactersHere.Count; j++)
			{
				Character p_character = locationGridTile.charactersHere[j];
				AddCharacterInsideAOE(p_character);
			}
		}
	}

	private void AddCharacterInsideAOE(Character p_character)
	{
		if (!allCharactersInsideAOE.Contains(p_character))
		{
			allCharactersInsideAOE.Add(p_character);
		}
	}

	private void RemoveCharacterInsideAOE(Character p_character)
	{
		allCharactersInsideAOE.Remove(p_character);
	}

	private void PopulateInitialStructuresInsideAOE()
	{
		for (int i = 0; i < affectedTiles.Count; i++)
		{
			LocationGridTile locationGridTile = affectedTiles[i];
			if (ShouldSpellAffectStructureOfType(locationGridTile.structure))
			{
				AddAffectedStructure(locationGridTile.structure);
			}
		}
	}

	private void AddAffectedStructure(LocationStructure p_structure)
	{
		if (!affectedStructures.ContainsKey(p_structure))
		{
			affectedStructures.Add(p_structure, 0);
		}
		affectedStructures[p_structure]++;
	}

	private void RemoveAffectedStructure(LocationStructure p_structure)
	{
		if (affectedStructures.ContainsKey(p_structure))
		{
			affectedStructures[p_structure]--;
			if (affectedStructures[p_structure] <= 0)
			{
				affectedStructures.Remove(p_structure);
			}
		}
	}

	protected virtual bool ShouldSpellAffectStructureOfType(LocationStructure p_structure)
	{
		return p_structure.structureType != STRUCTURE_TYPE.WILDERNESS;
	}

	public void OnTileChangedStructure(LocationStructure p_newStructure, LocationStructure p_oldStructure, LocationGridTile p_tile)
	{
		if (p_oldStructure != null && ShouldSpellAffectStructureOfType(p_oldStructure))
		{
			RemoveAffectedStructure(p_oldStructure);
		}
		if (p_newStructure != null && ShouldSpellAffectStructureOfType(p_newStructure))
		{
			AddAffectedStructure(p_newStructure);
		}
	}

	public virtual string GetAOESpellTestingData()
	{
		string text = "\n\tAffected Tile Count: " + affectedTiles.Count;
		text += "\n\tAffected Structures:";
		foreach (KeyValuePair<LocationStructure, int> affectedStructure in affectedStructures)
		{
			text = text + "\n\t" + affectedStructure.Key.name + " - " + affectedStructure.Value;
		}
		return text;
	}
}
