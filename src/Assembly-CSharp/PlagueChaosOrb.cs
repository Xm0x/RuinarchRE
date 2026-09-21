using Interrupts;
using UtilityScripts;

public class PlagueChaosOrb : PassiveSkill
{
	public override string name => "Chaos Orbs from Plague";

	public override string description => "Chaos Orbs upon Acquiring plague symptom";

	public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Plague_Chaos_Orb;

	public override void ActivateSkill()
	{
		Messenger.AddListener<InterruptHolder>(InterruptSignals.INTERRUPT_STARTED, OnInterruptAdded);
	}

	private void OnInterruptAdded(InterruptHolder interrupt)
	{
		if (GameUtilities.RollChance(50))
		{
			Character actor = interrupt.actor;
			if (actor.faction != null && actor.faction.factionType.type != FACTION_TYPE.Demon_Cult && (interrupt.interrupt.type == INTERRUPT.Seizure || interrupt.interrupt.type == INTERRUPT.Sneeze || interrupt.interrupt.type == INTERRUPT.Puke) && PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.PLAGUE).TryDecreaseRemainingChaosOrbs(1))
			{
				Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, actor.worldPosition, 1, actor.gridTileLocation.parentMap);
			}
		}
	}
}
