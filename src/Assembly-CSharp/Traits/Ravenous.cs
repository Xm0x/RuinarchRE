namespace Traits;

public class Ravenous : Trait
{
	public RavenousSpirit owner { get; private set; }

	public Ravenous()
	{
		name = "Ravenous";
		description = "This is ravenous.";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		isHidden = true;
		AddTraitOverrideFunctionIdentifier("Collision_Trait");
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is RavenousSpirit)
		{
			owner = addedTo as RavenousSpirit;
		}
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is RavenousSpirit)
		{
			owner = addTo as RavenousSpirit;
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
