using Traits;

public class HellspawnData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.HELLSPAWN;

	public override string name => "Hellspawn";

	public override string description => "This Ability will plant a Hellspawn inside the Target. When the Target dies, multiple Ephemeral monsters will erupt from its dead body.";

	public HellspawnData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Character character)
		{
			character.traitContainer.AddTrait(character, "Hellspawned");
			Hellspawned traitOrStatus = character.traitContainer.GetTraitOrStatus<Hellspawned>("Hellspawned");
			if (traitOrStatus != null && character.moodComponent.moodState == MOOD_STATE.Critical)
			{
				traitOrStatus.TryHellspawnDeath(character);
			}
			base.ActivateAbility(targetPOI);
		}
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		bool flag = base.IsValid(target);
		if (flag && target is Character character)
		{
			if (character.isDead)
			{
				return false;
			}
			if (character.traitContainer.HasTrait("Hellspawned"))
			{
				return false;
			}
		}
		return flag;
	}
}
