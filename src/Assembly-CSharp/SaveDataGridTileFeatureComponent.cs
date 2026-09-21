using System;
using Inner_Maps.Grid_Tile_Features;

public class SaveDataGridTileFeatureComponent : SaveData<GridTileFeatureComponent>
{
	public SaveDataGridTileFeature[] features;

	public override void Save(GridTileFeatureComponent data)
	{
		base.Save(data);
		features = new SaveDataGridTileFeature[data.features.Count];
		for (int i = 0; i < data.features.Count; i++)
		{
			GridTileFeature gridTileFeature = data.features[i];
			SaveDataGridTileFeature saveDataGridTileFeature = Activator.CreateInstance(gridTileFeature.serializedData) as SaveDataGridTileFeature;
			saveDataGridTileFeature.Save(gridTileFeature);
			features[i] = saveDataGridTileFeature;
		}
	}

	public override GridTileFeatureComponent Load()
	{
		return new GridTileFeatureComponent();
	}

	public override void CleanUp()
	{
		for (int i = 0; i < features.Length; i++)
		{
			features[i].CleanUp();
		}
		features = null;
	}
}
