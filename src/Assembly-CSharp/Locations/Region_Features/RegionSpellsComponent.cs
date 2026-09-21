using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;

namespace Locations.Region_Features;

public class RegionSpellsComponent
{
	public Dictionary<LocationGridTile, List<AOESpellTileObject>> activeSpellsInRegion { get; }

	public RegionSpellsComponent()
	{
		activeSpellsInRegion = new Dictionary<LocationGridTile, List<AOESpellTileObject>>();
	}

	public void AddActiveSpellInRegion(LocationGridTile p_tile, AOESpellTileObject p_spellObject)
	{
		if (!activeSpellsInRegion.ContainsKey(p_tile))
		{
			activeSpellsInRegion.Add(p_tile, new List<AOESpellTileObject>());
		}
		activeSpellsInRegion[p_tile].Add(p_spellObject);
	}

	public void RemoveActiveSpellInRegion(LocationGridTile p_tile, AOESpellTileObject p_spellObject)
	{
		if (activeSpellsInRegion.ContainsKey(p_tile) && activeSpellsInRegion[p_tile].Remove(p_spellObject) && activeSpellsInRegion[p_tile].Count <= 0)
		{
			activeSpellsInRegion.Remove(p_tile);
		}
	}

	public List<AOESpellTileObject> GetActiveSpellsOnTile(LocationGridTile p_tile)
	{
		if (activeSpellsInRegion.ContainsKey(p_tile))
		{
			return activeSpellsInRegion[p_tile];
		}
		return null;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
