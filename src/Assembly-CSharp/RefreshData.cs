public class RefreshData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.REFRESH;

	public override string name => "Refresh";

	public override string description => "This Ability replenishes all Needs of the target villager.";

	public RefreshData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Character character)
		{
			if (HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Replenish_All_Needs_Small_Amount))
			{
				character.needsComponent.AdjustNeeds(25);
			}
			else if (HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Replenish_All_Needs_By_Half))
			{
				character.needsComponent.AdjustNeeds(50);
			}
			else if (HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Replenish_All_Needs_Large_Amount))
			{
				character.needsComponent.AdjustNeeds(75);
			}
			else if (HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Fully_Replenish_All_Needs))
			{
				character.needsComponent.ResetNeeds();
			}
			GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Heal, allowRotation: false);
			base.ActivateAbility(targetPOI);
		}
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (target is Character character && (character.isDead || !character.race.IsSapient()))
		{
			return false;
		}
		return base.IsValid(target);
	}
}
