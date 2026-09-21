public class CorruptTileData : BuildPlayerSkill
{
	public override string name => "Corrupt Tile";

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.CORRUPT_TILE;

	public override void ActivateAbility(int p_numberOfTimesToBeExecuted)
	{
		base.ActivateAbility(p_numberOfTimesToBeExecuted);
		if ((base.hasCharges && base.charges <= 0 && base.bonusCharges <= 0) || (base.hasSpiritEnergyCost && PlayerManager.Instance.player.currenciesComponent.spiritEnergy < base.spiritEnergyCost))
		{
			Messenger.Broadcast(UISignals.UPDATE_BUILD_LIST);
		}
	}
}
