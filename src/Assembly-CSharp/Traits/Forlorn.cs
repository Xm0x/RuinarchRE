namespace Traits;

public class Forlorn : Trait
{
	public ForlornSpirit owner { get; private set; }

	public Forlorn()
	{
		name = "Forlorn";
		description = "This is forlorn.";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		isHidden = true;
		AddTraitOverrideFunctionIdentifier("Collision_Trait");
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is ForlornSpirit forlornSpirit)
		{
			owner = forlornSpirit;
		}
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is ForlornSpirit forlornSpirit)
		{
			owner = forlornSpirit;
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
