namespace Traits;

public class Webbed : Status
{
	public override bool isSingleton => true;

	public Webbed()
	{
		name = "Webbed";
		description = "This is Webbed.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = 0;
		isHidden = true;
		AddTraitOverrideFunctionIdentifier("Initiate_Map_Visual_Trait");
		AddTraitOverrideFunctionIdentifier("Destroy_Map_Visual_Trait");
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character character && (bool)character.marker)
		{
			character.marker.ShowAdditionalEffect(CharacterManager.Instance.webbedEffect);
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character { hasMarker: not false } character)
		{
			character.marker.ShowAdditionalEffect(CharacterManager.Instance.webbedEffect);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character { hasMarker: not false } character)
		{
			character.marker.HideAdditionalEffect();
		}
	}

	public override void OnInitiateMapObjectVisual(ITraitable traitable)
	{
		if (traitable is Character { hasMarker: not false } character)
		{
			character.marker.ShowAdditionalEffect(CharacterManager.Instance.webbedEffect);
		}
	}

	public override void OnDestroyMapObjectVisual(ITraitable traitable)
	{
		if (traitable is Character { hasMarker: not false } character)
		{
			character.marker.HideAdditionalEffect();
		}
	}
}
