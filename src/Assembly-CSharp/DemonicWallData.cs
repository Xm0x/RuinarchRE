public class DemonicWallData : BuildPlayerSkill
{
	public override string name => "Demonic Wall";

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DEMONIC_WALL;

	public override void ActivateAbility(int p_numberOfTimesToBeExecuted)
	{
		base.ActivateAbility(p_numberOfTimesToBeExecuted);
		if ((base.hasCharges && base.charges <= 0 && base.bonusCharges <= 0) || (base.hasSpiritEnergyCost && PlayerManager.Instance.player.currenciesComponent.spiritEnergy < base.spiritEnergyCost))
		{
			Messenger.Broadcast(UISignals.UPDATE_BUILD_LIST);
		}
	}
}
