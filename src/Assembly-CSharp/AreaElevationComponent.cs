using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class AreaElevationComponent : AreaComponent
{
	public Dictionary<ELEVATION, int> elevationDictionary { get; }

	public ELEVATION elevationType { get; private set; }

	public AreaElevationComponent()
	{
		elevationDictionary = new Dictionary<ELEVATION, int>();
	}

	public void OnTileAddedToArea(LocationGridTile p_tile)
	{
		AddElevationVoteToDictionary(p_tile.elevationType);
	}

	public void OnTileInAreaChangedElevation(LocationGridTile p_tile, ELEVATION p_oldElevation)
	{
		RemoveElevationVoteFromDictionary(p_oldElevation);
		AddElevationVoteToDictionary(p_tile.elevationType);
	}

	private void AddElevationVoteToDictionary(ELEVATION p_elevation)
	{
		if (!elevationDictionary.ContainsKey(p_elevation))
		{
			elevationDictionary.Add(p_elevation, 0);
		}
		elevationDictionary[p_elevation]++;
		UpdateElevationBasedOnVotes();
	}

	private void RemoveElevationVoteFromDictionary(ELEVATION p_elevation)
	{
		if (!elevationDictionary.ContainsKey(p_elevation))
		{
			elevationDictionary.Add(p_elevation, 0);
		}
		elevationDictionary[p_elevation]--;
		UpdateElevationBasedOnVotes();
	}

	public bool IsFully(ELEVATION p_elevation)
	{
		foreach (KeyValuePair<ELEVATION, int> item in elevationDictionary)
		{
			if (item.Key != p_elevation && item.Value > 0)
			{
				return false;
			}
		}
		return true;
	}

	public bool HasElevation(ELEVATION p_elevation)
	{
		if (elevationDictionary.ContainsKey(p_elevation))
		{
			return elevationDictionary[p_elevation] > 0;
		}
		return false;
	}

	private void UpdateElevationBasedOnVotes()
	{
		int num = int.MinValue;
		ELEVATION eLEVATION = ELEVATION.PLAIN;
		foreach (KeyValuePair<ELEVATION, int> item in elevationDictionary)
		{
			int value = item.Value;
			if (value > num)
			{
				eLEVATION = item.Key;
				num = value;
			}
			else if (value == num)
			{
				if (eLEVATION == ELEVATION.WATER && item.Key == ELEVATION.MOUNTAIN)
				{
					eLEVATION = item.Key;
				}
				else if (eLEVATION == ELEVATION.PLAIN)
				{
					eLEVATION = ELEVATION.PLAIN;
				}
			}
		}
		elevationType = eLEVATION;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
