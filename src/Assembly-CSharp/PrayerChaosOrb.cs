using UtilityScripts;

public class PrayerChaosOrb : PassiveSkill
{
	public override string name => "Mana Orbs from Praying Cultists";

	public override string description => "Mana Orbs on Praying Cultist";

	public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Prayer_Chaos_Orb;

	public override void ActivateSkill()
	{
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_PRAY_SUCCESS, OnSuccessPraying);
	}

	private void OnSuccessPraying(Character character)
	{
		int p_amount = GameUtilities.RandomBetweenTwoNumbers(1, 2);
		if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.BRAINWASH).TryDecreaseRemainingChaosOrbs(ref p_amount))
		{
			Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.worldPosition, p_amount, character.gridTileLocation.parentMap);
		}
	}
}
