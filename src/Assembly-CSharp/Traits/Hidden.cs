namespace Traits;

public class Hidden : Status
{
	public Hidden()
	{
		name = "Hidden";
		description = "Cannot be seen.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		isHidden = true;
		hindersWitness = true;
		AddTraitOverrideFunctionIdentifier("Initiate_Map_Visual_Trait");
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			if (character.hasMarker)
			{
				character.marker.SetVisionColliderState(p_state: false);
				character.marker.SetVisionTriggerState(p_state: false);
			}
			Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, (IPointOfInterest)character, "");
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character { hasMarker: not false } character)
		{
			character.marker.SetVisionColliderState(p_state: true);
			character.marker.SetVisionTriggerState(p_state: true);
		}
	}

	public override void OnInitiateMapObjectVisual(ITraitable traitable)
	{
		if (traitable is Character character)
		{
			character.marker.SetVisionColliderState(p_state: false);
			character.marker.SetVisionTriggerState(p_state: false);
		}
	}
}
