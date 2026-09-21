using System.Collections.Generic;
using Inner_Maps.Location_Structures;

namespace Locations.Region_Components;

public class BiomeDivisionComponent
{
	public List<BiomeDivision> divisions { get; private set; }

	public BiomeDivisionComponent()
	{
		divisions = new List<BiomeDivision>();
	}

	public BiomeDivisionComponent(SaveDataRegionDivisionComponent data)
	{
		divisions = new List<BiomeDivision>();
		for (int i = 0; i < data.divisions.Count; i++)
		{
			divisions.Add(data.divisions[i].Load());
		}
	}

	public void AddBiomeDivision(BiomeDivision p_division)
	{
		divisions.Add(p_division);
	}

	public BiomeDivision GetBiomeDivision(BIOMES p_biome)
	{
		for (int i = 0; i < divisions.Count; i++)
		{
			BiomeDivision biomeDivision = divisions[i];
			if (biomeDivision.biome == p_biome)
			{
				return biomeDivision;
			}
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
