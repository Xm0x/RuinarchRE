public class DarkRitualChaosOrb : PassiveSkill
{
	public override string name => "Mana Orbs from Dark Ritual Action Cultists";

	public override string description => "Mana Orbs on Dark Ritual Action";

	public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Dark_Ritual_Chaos_Orb;

	public override void ActivateSkill()
	{
		Messenger.AddListener<ActualGoapNode>(JobSignals.ON_FINISH_PRAYING, OnSuccessPraying);
	}

	private void OnSuccessPraying(ActualGoapNode p_goapNode)
	{
		if (p_goapNode.goapType == INTERACTION_TYPE.DARK_RITUAL)
		{
			int p_amount = 3;
			if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.BRAINWASH).TryDecreaseRemainingChaosOrbs(ref p_amount))
			{
				Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_goapNode.target.worldPosition, p_amount, p_goapNode.target.gridTileLocation.parentMap);
			}
		}
	}
}
