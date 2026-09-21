public class DamageChaosOrbThresholdReduction : PassiveSkill
{
	public override string name => "Chaos Orb Damage Threshold Reduction";

	public override string description => "Reduces needed amount of accumulated spell/raid damage to produce a chaos orb.";

	public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Damage_Chaos_Orb_Threshold_Reduction;

	public override void ActivateSkill()
	{
		PlayerManager.Instance.player.playerSkillComponent.OverrideDefaultChaosOrbExpulsionThreshold(500);
		PlayerManager.Instance.player.playerSkillComponent.OverrideDefaultChaosOrbExpulsionThresholdFromRaid(300);
	}
}
