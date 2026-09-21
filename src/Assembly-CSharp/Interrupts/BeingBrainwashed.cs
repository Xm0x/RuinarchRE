using Inner_Maps.Location_Structures;

namespace Interrupts;

public class BeingBrainwashed : Interrupt
{
	public BeingBrainwashed()
		: base(INTERRUPT.Being_Brainwashed)
	{
		base.duration = 24;
		base.doesStopCurrentAction = true;
		base.doesDropCurrentJob = true;
		base.interruptIconString = GoapActionStateDB.Sad_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Player,
			LOG_TAG.Life_Changes
		};
	}

	public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder)
	{
		Character actor = interruptHolder.actor;
		if (actor.gridTileLocation.structure.IsTilePartOfARoom(actor.gridTileLocation, out var room) && room is PrisonCell prisonCell)
		{
			Log log;
			if (prisonCell.WasBrainwashSuccessful(actor))
			{
				LocationStructure currentStructure = actor.currentStructure;
				if (currentStructure != null && currentStructure is TortureChambers)
				{
					actor.movementComponent.LetGo();
				}
				actor.religionComponent.ChangeReligion(RELIGION.Demon_Worship);
				int p_amount = ReligionComponent.Religious_Cultist_Belief_Threshold - actor.religionComponent.GetBeliefPoints(RELIGION.Demon_Worship);
				actor.religionComponent.IncreaseBeliefPoints(RELIGION.Demon_Worship, p_amount);
				actor.traitContainer.AddTrait(actor, "Demon Cultist");
				actor.needsComponent.ResetNeeds();
				log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " converted", LOG_TAG.Major);
			}
			else
			{
				actor.traitContainer.AddTrait(actor, "Unconscious");
				log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " not_converted", base.logTags);
			}
			log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
		}
		return true;
	}
}
