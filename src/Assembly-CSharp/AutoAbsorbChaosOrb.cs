public class AutoAbsorbChaosOrb : PassiveSkill
{
	public override string name => "Automatically absorb Chaos Orbs";

	public override string description => "Expired Mana Orbs are auto absorbed";

	public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Auto_Absorb_Chaos_Orb;

	public override void ActivateSkill()
	{
		Messenger.AddListener<ChaosOrb>(PlayerSignals.CHAOS_ORB_EXPIRED, OnChaosOrbExpired);
	}

	private void OnChaosOrbExpired(ChaosOrb chaosOrb)
	{
		PlayerManager.Instance.player.currenciesComponent.AdjustChaoticEnergy(2);
	}
}
