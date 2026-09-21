using Characters.Components;
using Inner_Maps.Location_Structures;

namespace Traits;

public class StockedUp : Status, CharacterEventDispatcher.IHomeStructureListener
{
	public StockedUp()
	{
		name = "Stocked Up";
		description = "Well prepared food-wise.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.POSITIVE;
		ticksDuration = 0;
		moodEffect = 10;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character character)
		{
			character.eventDispatcher.SubscribeToCharacterSetHomeStructure(this);
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			character.eventDispatcher.SubscribeToCharacterSetHomeStructure(this);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character.eventDispatcher.UnsubscribeToCharacterSetHomeStructure(this);
		}
	}

	public void OnCharacterSetHomeStructure(Character p_character, LocationStructure p_homeStructure)
	{
		if (p_character.homeStructure == null)
		{
			p_character.traitContainer.RemoveTrait(p_character, this);
		}
	}

	public void OnObjectPlacedInHomeDwelling(Character p_character, LocationStructure p_homeStructure, TileObject p_placedObject)
	{
	}

	public void OnObjectRemovedFromHomeDwelling(Character p_character, LocationStructure p_homeStructure, TileObject p_removedObject)
	{
	}
}
