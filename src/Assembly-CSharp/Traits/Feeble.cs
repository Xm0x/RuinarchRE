namespace Traits;

public class Feeble : Trait
{
	public FeebleSpirit owner { get; private set; }

	public Feeble()
	{
		name = "Feeble";
		description = "This is feeble.";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		isHidden = true;
		AddTraitOverrideFunctionIdentifier("Collision_Trait");
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is FeebleSpirit feebleSpirit)
		{
			owner = feebleSpirit;
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom == owner)
		{
			owner = null;
		}
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is FeebleSpirit feebleSpirit)
		{
			owner = feebleSpirit;
		}
	}

	public override bool OnCollideWith(IPointOfInterest collidedWith, IPointOfInterest owner)
	{
		if (collidedWith is Character)
		{
			Character character = collidedWith as Character;
			if (character.needsComponent.HasNeeds() && !character.isDead)
			{
				this.owner.StartSpiritPossession(character);
			}
		}
		return true;
	}
}
