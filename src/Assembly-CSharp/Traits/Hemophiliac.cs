using System;
using System.Collections.Generic;

namespace Traits;

public class Hemophiliac : Trait
{
	private List<Character> _knownVampires;

	private Character _owner;

	public List<Character> knownVampires => _knownVampires;

	public override Type serializedData => typeof(SaveDataHemophiliac);

	public Hemophiliac()
	{
		name = "Hemophiliac";
		description = "Obsessed with Vampires.";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		mutuallyExclusive = new string[1] { "Hemophobic" };
		_knownVampires = new List<Character>();
	}

	public void OnBecomeAwareOfVampire(Character vampire)
	{
		if (!_knownVampires.Contains(vampire))
		{
			_knownVampires.Add(vampire);
			_owner.relationshipContainer.AdjustOpinion(_owner, vampire, "Hemophiliac", 30);
		}
	}

	public bool IsVampireKnown(Character character)
	{
		return _knownVampires.Contains(character);
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character owner)
		{
			_owner = owner;
		}
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character owner)
		{
			_owner = owner;
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		_owner = null;
	}

	public override void DisconnectFromCharacter(IPointOfInterest p_owner, Character p_character)
	{
		base.DisconnectFromCharacter(p_owner, p_character);
		_knownVampires.Remove(p_character);
	}

	public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
		SaveDataHemophiliac saveDataHemophiliac = p_saveDataTrait as SaveDataHemophiliac;
		knownVampires.AddRange(SaveUtilities.ConvertIDListToCharacters(saveDataHemophiliac.knownVampires));
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = _owner;
		_knownVampires.Contains(p_character);
	}
}
