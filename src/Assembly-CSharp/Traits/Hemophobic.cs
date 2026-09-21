using System;
using System.Collections.Generic;

namespace Traits;

public class Hemophobic : Trait
{
	private List<Character> _knownVampires;

	private Character _owner;

	public List<Character> knownVampires => _knownVampires;

	public override Type serializedData => typeof(SaveDataHemophobic);

	public Hemophobic()
	{
		name = "Hemophobic";
		description = "Deathly afraid of Vampires.";
		type = TRAIT_TYPE.FLAW;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = 0;
		canBeTriggered = false;
		mutuallyExclusive = new string[1] { "Hemophiliac" };
		_knownVampires = new List<Character>();
	}

	public void OnBecomeAwareOfVampire(Character vampire)
	{
		if (!_knownVampires.Contains(vampire))
		{
			_knownVampires.Add(vampire);
			_owner.relationshipContainer.AdjustOpinion(_owner, vampire, "Hemophobic", -30);
			_owner.relationshipContainer.TryBreakUp(_owner, vampire, "Break_Up_Hemophobic");
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

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom == _owner)
		{
			_owner = null;
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

	public override void DisconnectFromCharacter(IPointOfInterest p_owner, Character p_character)
	{
		base.DisconnectFromCharacter(p_owner, p_character);
		_knownVampires.Remove(p_character);
	}

	public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
		SaveDataHemophobic saveDataHemophobic = p_saveDataTrait as SaveDataHemophobic;
		knownVampires.AddRange(SaveUtilities.ConvertIDListToCharacters(saveDataHemophobic.knownVampires));
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = _owner;
		_knownVampires.Contains(p_character);
	}
}
