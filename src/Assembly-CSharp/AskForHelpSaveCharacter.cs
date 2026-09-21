public class AskForHelpSaveCharacter : GoapAction
{
	private Character troubledCharacter;

	public AskForHelpSaveCharacter()
		: base(INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER)
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
