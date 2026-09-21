public abstract class SaveDataJobNode : SaveData<JobNode>
{
	public string persistentID;

	public override void Save(JobNode data)
	{
		base.Save(data);
		persistentID = data.persistentID;
	}
}
