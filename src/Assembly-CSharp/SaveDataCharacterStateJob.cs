public class SaveDataCharacterStateJob : SaveDataJobQueueItem
{
	public CHARACTER_STATE targetState;

	public override void Save(JobQueueItem job)
	{
		base.Save(job);
		CharacterStateJob characterStateJob = job as CharacterStateJob;
		targetState = characterStateJob.targetState;
	}

	public override JobQueueItem Load()
	{
		return JobManager.Instance.CreateNewCharacterStateJob(this);
	}
}
