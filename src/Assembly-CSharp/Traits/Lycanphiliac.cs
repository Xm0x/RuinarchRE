using System;
using System.Collections.Generic;

namespace Traits;

public class Lycanphiliac : Trait
{
	private Character _owner;

	private List<Character> _knownLycans;

	public List<Character> knownLycans => _knownLycans;

	public override Type serializedData => typeof(SaveDataLycanphiliac);

	public Lycanphiliac()
	{
		name = "Lycanphiliac";
		description = "Obsessed with Lycanthropes.";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		canBeTriggered = false;
		mutuallyExclusive = new string[1] { "Lycanphobic" };
		_knownLycans = new List<Character>();
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
		_knownLycans.Remove(p_character);
	}

	public void OnBecomeAwareOfLycan(Character lycan)
	{
		if (!_knownLycans.Contains(lycan))
		{
			_knownLycans.Add(lycan);
			_owner.relationshipContainer.AdjustOpinion(_owner, lycan, "Lycanphiliac", 30);
		}
	}

	public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
		SaveDataLycanphiliac saveDataLycanphiliac = p_saveDataTrait as SaveDataLycanphiliac;
		_knownLycans.AddRange(SaveUtilities.ConvertIDListToCharacters(saveDataLycanphiliac.knownLycans));
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = _owner;
		_knownLycans.Contains(p_character);
	}
}
