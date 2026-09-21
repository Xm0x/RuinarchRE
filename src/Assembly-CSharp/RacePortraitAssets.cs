using System;

[Serializable]
public class RacePortraitAssets
{
	public string raceName;

	public RACE race;

	public PortraitAssetCollection neutralAssets;

	public PortraitAssetCollection maleAssets;

	public PortraitAssetCollection femaleAssets;

	public RacePortraitAssets(RACE race)
	{
		this.race = race;
		raceName = race.ToStringEnum();
		neutralAssets = new PortraitAssetCollection();
		maleAssets = new PortraitAssetCollection();
		femaleAssets = new PortraitAssetCollection();
	}

	public PortraitAssetCollection GetPortraitAssetCollection(GENDER gender)
	{
		return gender switch
		{
			GENDER.MALE => maleAssets, 
			GENDER.FEMALE => femaleAssets, 
			_ => neutralAssets, 
		};
	}
}
