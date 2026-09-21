namespace Interrupts;

public class FeelingAngry : Interrupt
{
	public FeelingAngry()
		: base(INTERRUPT.Feeling_Angry)
	{
		base.duration = 3;
		base.doesStopCurrentAction = true;
		base.interruptIconString = GoapActionStateDB.Anger_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Life_Changes,
			LOG_TAG.Combat
		};
	}
}
