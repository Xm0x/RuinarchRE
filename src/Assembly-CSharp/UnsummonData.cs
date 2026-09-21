public class UnsummonData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.UNSUMMON;

	public override string name => "Unsummon";

	public override string description => "Unsummon this character.";

	public UnsummonData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Character character)
		{
			character.Death();
			if (UIManager.Instance.monsterInfoUI.isShowing && UIManager.Instance.monsterInfoUI.activeMonster == character)
			{
				UIManager.Instance.monsterInfoUI.CloseMenu();
			}
			else if (UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter == character)
			{
				UIManager.Instance.characterInfoUI.CloseMenu();
			}
		}
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (base.CanPerformAbilityTowards(targetCharacter))
		{
			return !targetCharacter.isDead;
		}
		return false;
	}
}
