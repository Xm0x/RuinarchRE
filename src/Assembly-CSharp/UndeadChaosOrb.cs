using UtilityScripts;

public class UndeadChaosOrb : PassiveSkill
{
	public override string name => "Mana Orbs from Undead Death";

	public override string description => "Mana Orbs on Undead Death";

	public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Undead_Chaos_Orb;

	public override void ActivateSkill()
	{
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
	}

	private void OnCharacterDied(Character character)
	{
		Faction faction = character.faction;
		if (faction != null && faction.factionType.type == FACTION_TYPE.Undead && character.hasMarker)
		{
			bool flag = true;
			if (character.characterClass.IsZombie())
			{
				flag = GameUtilities.RollChance(35);
			}
			if (flag)
			{
				Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.worldPosition, 1, character.currentRegion.innerMap);
			}
		}
	}
}
