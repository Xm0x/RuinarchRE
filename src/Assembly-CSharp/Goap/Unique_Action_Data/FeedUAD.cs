namespace Goap.Unique_Action_Data;

public class FeedUAD : UniqueActionData
{
	public bool usedPoisonedFood { get; private set; }

	public FeedUAD()
	{
		usedPoisonedFood = false;
	}

	public FeedUAD(SaveDataFeedUAD saveData)
	{
		usedPoisonedFood = saveData.usedPoisonedFood;
	}

	public void SetUsedPoisonedFood(bool state)
	{
		usedPoisonedFood = state;
	}

	public override SaveDataUniqueActionData Save()
	{
		SaveDataFeedUAD saveDataFeedUAD = new SaveDataFeedUAD();
		saveDataFeedUAD.Save(this);
		return saveDataFeedUAD;
	}
}
