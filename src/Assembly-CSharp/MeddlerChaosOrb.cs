public class MeddlerChaosOrb : PassiveSkill
{
	public override string name => "Chaos Orbs from Successful Meddler Scheme";

	public override string description => "Chaos Orbs from Successful Meddler Scheme";

	public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Meddler_Chaos_Orb;

	public override void ActivateSkill()
	{
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_MEDDLER_SCHEME_SUCCESSFUL, OnSuccessMeddlerScheme);
	}

	private void OnSuccessMeddlerScheme(Character p_character)
	{
		int p_amount = 3;
		PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.SCHEME).TryDecreaseRemainingChaosOrbs(p_amount);
		if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.MEDDLER).TryDecreaseRemainingChaosOrbs(ref p_amount))
		{
			Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.gridTileLocation.centeredWorldLocation, p_amount, p_character.gridTileLocation.parentMap);
		}
	}
}
