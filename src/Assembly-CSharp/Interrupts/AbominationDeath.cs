using Inner_Maps;

namespace Interrupts;

public class AbominationDeath : Interrupt
{
	public AbominationDeath()
		: base(INTERRUPT.Abomination_Death)
	{
		base.duration = 3;
		base.doesStopCurrentAction = true;
		base.interruptIconString = GoapActionStateDB.Death_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder)
	{
		LocationGridTile gridTileLocation = interruptHolder.actor.gridTileLocation;
		Summon arg = CharacterManager.Instance.SpawnNewMonsterInstanceFrom(SUMMON_TYPE.Abomination, interruptHolder.actor, null, null, gridTileLocation, FactionManager.Instance.wildMonsterFaction);
		interruptHolder.actor.SetDestroyMarkerOnDeath(state: true);
		interruptHolder.actor.Death("Abomination Germ", null, null, null, null, null, this);
		Messenger.Broadcast(MonsterSignals.ABOMINATION_SPAWNED_FROM_GERM, arg);
		if (UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter == interruptHolder.actor)
		{
			UIManager.Instance.characterInfoUI.CloseMenu();
		}
		return true;
	}
}
