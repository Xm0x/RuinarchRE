using Inner_Maps;

public class SpellDamageChaosOrb : PassiveSkill
{
	public override string name => "Chaos Orbs from Spell Damage";

	public override string description => "Chaos Orbs upon spell damage";

	public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Spell_Damage_Chaos_Orb;

	public override void ActivateSkill()
	{
		PlayerManager.Instance.player.damageAccumulator.SetActivatedSpellDamageChaosOrbPassiveSkill(p_state: true);
	}

	private void OnSpellDamageDone(Character character, int p_damageDone)
	{
		if (character == null)
		{
			return;
		}
		if (p_damageDone < 0)
		{
			p_damageDone *= -1;
		}
		int num = p_damageDone / 300;
		if (num > 0)
		{
			LocationGridTile locationGridTile = character.gridTileLocation;
			if (character.isDead)
			{
				locationGridTile = character.deathTilePosition;
			}
			if (locationGridTile != null)
			{
				Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, locationGridTile.centeredWorldLocation, num, locationGridTile.parentMap);
			}
		}
	}
}
