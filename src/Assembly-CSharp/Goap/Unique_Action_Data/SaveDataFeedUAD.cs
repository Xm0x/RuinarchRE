namespace Goap.Unique_Action_Data;

public class SaveDataFeedUAD : SaveDataUniqueActionData
{
	public bool usedPoisonedFood;

	public override void Save(UniqueActionData data)
	{
		base.Save(data);
		FeedUAD feedUAD = data as FeedUAD;
		usedPoisonedFood = feedUAD.usedPoisonedFood;
	}

	public override UniqueActionData Load()
	{
		return new FeedUAD(this);
	}
}
