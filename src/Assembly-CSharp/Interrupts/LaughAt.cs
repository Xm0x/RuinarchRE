namespace Interrupts;

public class LaughAt : Interrupt
{
	public LaughAt()
		: base(INTERRUPT.Laugh_At)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.Mock_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Character character = interruptHolder.target as Character;
		if (character.limiterComponent.canWitness && character.hasMarker && character.marker.IsPOIInVision(interruptHolder.actor))
		{
			character.traitContainer.AddTrait(character, "Ashamed");
			return true;
		}
		if (interruptHolder.actor.gender == GENDER.MALE)
		{
			AkSoundEngine.PostEvent("Play_Male_Laugh", interruptHolder.actor.marker.gameObject);
		}
		else if (interruptHolder.actor.gender == GENDER.FEMALE)
		{
			AkSoundEngine.PostEvent("Play_Female_Laugh", interruptHolder.actor.marker.gameObject);
		}
		return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
	}
}
