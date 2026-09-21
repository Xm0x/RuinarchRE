namespace Interrupts;

public class NecromanticTransformation : Interrupt
{
	public NecromanticTransformation()
		: base(INTERRUPT.Necromantic_Transformation)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.No_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Major };
		base.shouldShowNotif = true;
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Character actor = interruptHolder.actor;
		actor.classComponent.AssignClass("Necromancer");
		actor.ResetUIString();
		actor.traitContainer.RemoveTrait(actor, "Enslaved");
		actor.ChangeFactionTo(FactionManager.Instance.undeadFaction, bypassIdeologyChecking: true);
		CharacterManager.Instance.SetNecromancerInTheWorld(actor);
		actor.MigrateHomeStructureTo(null);
		actor.ClearTerritory();
		return true;
	}

	public override Log CreateEffectLog(Character actor, IPointOfInterest target)
	{
		if (LocalizationManager.Instance.HasLocalizedValue("Interrupts_Table", base.name + " effect"))
		{
			string localizedNameOfTrait = TraitManager.Instance.GetLocalizedNameOfTrait("Treacherous");
			if (actor.traitContainer.HasTrait("Evil"))
			{
				localizedNameOfTrait = TraitManager.Instance.GetLocalizedNameOfTrait("Evil");
			}
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " effect", base.logTags);
			log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(null, localizedNameOfTrait, LOG_IDENTIFIER.STRING_1);
			return log;
		}
		return null;
	}
}
