using System;

namespace Locations.Area_Features;

[Serializable]
public class SaveDataPoisonBloomFeature : SaveDataAreaFeature
{
	public int expiryInTicks;

	public bool isPlayerSource;

	public override void Save(AreaFeature tileFeature)
	{
		base.Save(tileFeature);
		PoisonBloomFeature poisonBloomFeature = tileFeature as PoisonBloomFeature;
		expiryInTicks = GameManager.Instance.Today().GetTickDifference(poisonBloomFeature.expiryDate);
		isPlayerSource = poisonBloomFeature.isPlayerSource;
	}

	public override AreaFeature Load()
	{
		PoisonBloomFeature obj = base.Load() as PoisonBloomFeature;
		obj.SetExpiryInTicks(expiryInTicks);
		obj.SetIsPlayerSource(isPlayerSource);
		return obj;
	}
}
