public class SaveDataTrainingDummy : SaveDataTileObject
{
	public string currentUserID;

	public override void Save(TileObject data)
	{
		base.Save(data);
		TrainingDummy trainingDummy = data as TrainingDummy;
		if (trainingDummy.currentUser != null)
		{
			currentUserID = trainingDummy.currentUser.persistentID;
		}
	}
}
