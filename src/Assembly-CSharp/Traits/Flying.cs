using System;

namespace Traits;

public class Flying : Trait
{
	public int stackCount;

	public override Type serializedData => typeof(SaveDataFlying);

	public Flying()
	{
		name = "Flying";
		description = "Flying Creature";
		type = TRAIT_TYPE.BUFF;
		effect = TRAIT_EFFECT.POSITIVE;
		ticksDuration = 0;
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			character?.movementComponent.SetTagAsTraversable(2);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character?.movementComponent.SetTagAsUnTraversable(2);
		}
	}
}
