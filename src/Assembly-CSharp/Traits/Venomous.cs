using UnityEngine;

namespace Traits;

public class Venomous : Trait
{
	public override bool isSingleton => true;

	public Venomous()
	{
		name = "Venomous";
		description = "Deals Poison damage. Also sometimes leaks out Poison.";
		type = TRAIT_TYPE.BUFF;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		elementalType = ELEMENTAL_TYPE.Poison;
		AddTraitOverrideFunctionIdentifier("Tick_Started_Trait");
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		addedTo.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned")?.SetIsVenomous();
	}

	public override void OnTickStarted(ITraitable traitable)
	{
		base.OnTickStarted(traitable);
		ApplyPoisonToTile(traitable);
	}

	private void ApplyPoisonToTile(ITraitable traitable)
	{
		if (traitable.gridTileLocation != null && Random.Range(0, 100) < 10)
		{
			traitable.gridTileLocation.tileObjectComponent.genericTileObject.traitContainer.AddTrait(traitable.gridTileLocation.tileObjectComponent.genericTileObject, "Poisoned", null, bypassElementalChance: true, -1, 0f, ELEMENTAL_TYPE.Poison);
		}
	}
}
