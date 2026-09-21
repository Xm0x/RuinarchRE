public class MonsterChaosOrb : PassiveSkill
{
	public override string name => "Mana Orbs from Monster Deaths";

	public override string description => "Mana Orbs on Monster Death";

	public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Monster_Chaos_Orb;

	public override void ActivateSkill()
	{
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
	}

	private void OnCharacterDied(Character character)
	{
		if (character is Summon && character.faction != null && character.faction.isPlayerFaction && character.hasMarker && !character.destroyMarkerOnDeath)
		{
			Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.worldPosition, 1, character.currentRegion.innerMap);
		}
	}
}
