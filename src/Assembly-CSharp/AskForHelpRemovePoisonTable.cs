using Traits;

public class AskForHelpRemovePoisonTable : GoapAction
{
	private Character troubledCharacter;

	private IPointOfInterest targetTable;

	private Poisoned poison;

	public AskForHelpRemovePoisonTable()
		: base(INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE)
	{
		base.actionIconString = GoapActionStateDB.Work_Icon;
		validTimeOfDays = new TIME_IN_WORDS[5]
		{
			TIME_IN_WORDS.MORNING,
			TIME_IN_WORDS.LUNCH_TIME,
			TIME_IN_WORDS.AFTERNOON,
			TIME_IN_WORDS.EARLY_NIGHT,
			TIME_IN_WORDS.LATE_NIGHT
		};
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}
}
