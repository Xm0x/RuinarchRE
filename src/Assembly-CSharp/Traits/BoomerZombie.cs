namespace Traits;

public class BoomerZombie : FiniteZombie
{
	public BoomerZombie()
	{
		name = "Boomer Zombie";
		description = "Boomer Zombie";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		isHidden = true;
		AddTraitOverrideFunctionIdentifier("Death_Trait");
	}

	public override bool OnDeath(Character character)
	{
		if (character.traitContainer.HasTrait("Poisoned"))
		{
			Poisoned traitOrStatus = character.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned");
			int stacks = character.traitContainer.GetStacks("Poisoned");
			character.traitContainer.RemoveStatusAndStacks(character, "Poisoned");
			if (character.gridTileLocation != null)
			{
				CombatManager.Instance.PoisonExplosion(character, character.gridTileLocation, stacks, character, 2, traitOrStatus.isPlayerSource);
			}
			character.SetDestroyMarkerOnDeath(state: true);
		}
		return base.OnDeath(character);
	}
}
