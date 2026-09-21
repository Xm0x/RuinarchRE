namespace Traits;

public class TileObjectTraitProcessor : TraitProcessor
{
	public override void OnTraitAdded(ITraitable traitable, Trait trait, Character characterResponsible, int overrideDuration)
	{
		TileObject tileObject = traitable as TileObject;
		tileObject.OnTileObjectGainedTrait(trait);
		DefaultProcessOnAddTrait(traitable, trait, characterResponsible, overrideDuration);
		Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_TRAIT_ADDED, tileObject, trait);
	}

	public override void OnTraitRemoved(ITraitable traitable, Trait trait, Character removedBy)
	{
		TileObject tileObject = traitable as TileObject;
		DefaultProcessOnRemoveTrait(traitable, trait, removedBy);
		tileObject.OnTileObjectLostTrait(trait);
		Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_TRAIT_REMOVED, tileObject, trait);
	}

	public override void OnStatusStacked(ITraitable traitable, Status status, Character characterResponsible, int overrideDuration)
	{
		if (DefaultProcessOnStackStatus(traitable, status, characterResponsible, overrideDuration))
		{
			Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_TRAIT_STACKED, traitable as TileObject, status.GetBase());
		}
	}

	public override void OnStatusUnstack(ITraitable traitable, Status status, Character removedBy = null, bool bySchedule = false)
	{
		DefaultProcessOnUnstackStatus(traitable, status, removedBy, bySchedule);
		Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_TRAIT_UNSTACKED, traitable as TileObject, status.GetBase());
	}
}
