public class TrapChaosOrb : PassiveSkill
{
	public override string name => "Mana Orbs from Trap";

	public override string description => "Mana Orbs from Trap";

	public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Trap_Chaos_Orb;

	public override void ActivateSkill()
	{
		Messenger.AddListener<Character, PLAYER_SKILL_TYPE, bool>(PlayerSkillSignals.ON_TRAP_ACTIVATED_ON_VILLAGER, OnTrapActivated);
	}

	private void OnTrapActivated(Character character, PLAYER_SKILL_TYPE p_trapType, bool p_isPlayerSource)
	{
		int p_amount = 5;
		if (!p_isPlayerSource || PlayerSkillManager.Instance.GetSkillData(p_trapType).TryDecreaseRemainingChaosOrbs(ref p_amount))
		{
			Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.worldPosition, p_amount, character.gridTileLocation.parentMap);
		}
	}
}
