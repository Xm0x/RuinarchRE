public static class JobSignals
{
	public static string STARTED_PERFORMING_ACTION = "OnActionPerformed";

	public static string SCREAM_FOR_HELP = "OnScreamForHelp";

	public static string CHARACTER_WILL_DO_JOB = "OnCharacterRecievedPlan";

	public static string CHARACTER_DID_ACTION_SUCCESSFULLY = "OnCharacterDidActionSuccessfully";

	public static string CHARACTER_FINISHED_ACTION = "OnCharacterFinishedAction";

	public static string CHARACTER_DOING_ACTION = "OnCharacterDoingAction";

	public static string AFTER_ACTION_STATE_SET = "OnAfterActionStateSet";

	public static string CHECK_JOB_APPLICABILITY = "OnCheckJobApplicability";

	public static string CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING = "OnCheckAllJobsTargetingApplicability";

	public static string CHECK_JOB_APPLICABILITY_OF_ALL_JOBS_OF_TYPE = "OnCheckJobApplicabilityOfAllJobsOfType";

	public static string JOB_REMOVED_FROM_QUEUE = "OnJobRemovedFromQueue";

	public static string JOB_ADDED_TO_QUEUE = "OnJobAddedToQueue";

	public static string DEMONIC_STRUCTURE_DISCOVERED = "DemonicStructureDiscovered";

	public static string CHARACTER_ASSUMED = "OnCharacterAssumed";

	public static string JOB_REMOVED_FROM_JOB_BOARD = "OnJobRemovedFromJobBoard";

	public static string ON_FINISH_PRAYING = "OnFinishPraying";
}
