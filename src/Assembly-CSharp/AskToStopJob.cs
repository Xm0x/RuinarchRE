public class AskToStopJob : GoapAction
{
	public GoapPlanJob jobToStop { get; private set; }

	public AskToStopJob()
		: base(INTERACTION_TYPE.ASK_TO_STOP_JOB)
	{
		base.actionIconString = GoapActionStateDB.Work_Icon;
		base.doesNotStopTargetCharacter = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}
}
