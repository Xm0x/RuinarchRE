namespace Traits;

public class UndeadSlayer : Slayer
{
	public UndeadSlayer()
	{
		name = "Undead Slayer";
		description = "Grants additional damage to undeads.";
		type = TRAIT_TYPE.BUFF;
		effect = TRAIT_EFFECT.POSITIVE;
		ticksDuration = 0;
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		_ = addedTo is Character;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		_ = addTo is Character;
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
	}
}
