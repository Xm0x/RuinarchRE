namespace Traits;

public class Suspicious : Trait
{
	public override bool isSingleton => true;

	public Suspicious()
	{
		name = "Suspicious";
		description = "Thinks danger is lurking at every corner. Might destroy things the player touches.";
		type = TRAIT_TYPE.BUFF;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		AddTraitOverrideFunctionIdentifier("See_Poi_Trait");
	}

	public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob)
	{
		if (characterThatWillDoJob.limiterComponent.canPerform && characterThatWillDoJob.limiterComponent.canMove && !characterThatWillDoJob.isDead && !characterThatWillDoJob.isAlliedWithPlayer && targetPOI is TileObject { tileObjectType: not TILE_OBJECT_TYPE.STRUCTURE_TILE_OBJECT, tileObjectType: not TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT } tileObject && tileObject.lastManipulatedBy is Player && (!tileObject.traitContainer.HasTrait("Edible") || !characterThatWillDoJob.needsComponent.isStarving) && !(tileObject is Heirloom))
		{
			if (targetPOI.IsOwnedBy(characterThatWillDoJob))
			{
				characterThatWillDoJob.jobComponent.RetrieveStolenItem(targetPOI as TileObject);
			}
			else
			{
				characterThatWillDoJob.jobComponent.TriggerDestroy(tileObject, "Destroy_Suspicious");
			}
		}
		return base.OnSeePOI(targetPOI, characterThatWillDoJob);
	}
}
