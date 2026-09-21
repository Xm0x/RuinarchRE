using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;

namespace Inner_Maps.Grid_Tile_Features;

public class GridTileFeatureComponent
{
	private List<GridTileFeature> _features;

	public List<GridTileFeature> features => _features;

	public GridTileFeatureComponent()
	{
		_features = new List<GridTileFeature>();
	}

	public void LoadReferences(SaveDataGridTileFeatureComponent data)
	{
		for (int i = 0; i < data.features.Length; i++)
		{
			SaveDataGridTileFeature saveDataGridTileFeature = data.features[i];
			GridTileFeature gridTileFeature = saveDataGridTileFeature.Load();
			_features.Add(gridTileFeature);
			gridTileFeature.Initialize();
			gridTileFeature.LoadReferences(saveDataGridTileFeature);
		}
	}

	public void Initialize()
	{
		List<GridTileFeature> list = ReflectiveEnumerator.GetEnumerableOfType<GridTileFeature>(Array.Empty<object>()).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			GridTileFeature gridTileFeature = list[i];
			_features.Add(gridTileFeature);
			gridTileFeature.Initialize();
		}
	}

	public void AddFeatureToTile<T>(LocationGridTile p_tile) where T : GridTileFeature
	{
		GetFeature<T>().AddTile(p_tile);
	}

	public void RemoveFeatureFromTile<T>(LocationGridTile p_tile) where T : GridTileFeature
	{
		GetFeature<T>().RemoveTile(p_tile);
	}

	public T GetFeature<T>()
	{
		for (int i = 0; i < _features.Count; i++)
		{
			GridTileFeature gridTileFeature = _features[i];
			if (gridTileFeature is T)
			{
				return (T)(object)((gridTileFeature is T) ? gridTileFeature : null);
			}
		}
		return default(T);
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
