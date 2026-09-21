namespace Traits;

public class Ephemeral : Status
{
	public Ephemeral()
	{
		name = "Ephemeral";
		description = "Will not live long.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.POSITIVE;
		isStacking = false;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(24);
		AddTraitOverrideFunctionIdentifier("Death_Trait");
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character { isDead: false } character)
		{
			character.SetDestroyMarkerOnDeath(state: true);
			character.Death("disappear");
		}
	}

	public override bool OnDeath(Character character)
	{
		return character.traitContainer.RemoveTrait(character, this);
	}
}
