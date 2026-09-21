using System;
using UnityEngine;

namespace Traits;

public class Newcomer : Status
{
	private Character _traitOwner;

	public const int MaxStacks = 4;

	public SharedOpinionModifier opinionModifier { get; private set; }

	public override Type serializedData => typeof(SaveDataNewcomer);

	public Newcomer()
	{
		name = "Newcomer";
		description = "Fresh off the boat!";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		isStacking = true;
		stackLimit = 4;
		ticksDuration = 0;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character traitOwner)
		{
			_traitOwner = traitOwner;
		}
	}

	public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
		SaveDataNewcomer saveDataNewcomer = p_saveDataTrait as SaveDataNewcomer;
		opinionModifier = DatabaseManager.Instance.sharedOpinionDatabase.GetOpinionModifierByPersistentID(saveDataNewcomer.sharedOpinionID);
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character traitOwner)
		{
			_traitOwner = traitOwner;
			opinionModifier = RelationshipManager.Instance.CreateNewComerOpinionModifier(_traitOwner);
			_traitOwner.faction.opinionComponent.AddOpinionModifier(_traitOwner, opinionModifier);
			opinionModifier.DecreaseModifierValue(10);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		opinionModifier.IncreaseModifierValue(Mathf.Abs(opinionModifier.modifierValue));
		_traitOwner = null;
		opinionModifier = null;
	}

	public override void OnStackStatus(ITraitable addedTo)
	{
		base.OnStackStatus(addedTo);
		opinionModifier.DecreaseModifierValue(10);
	}

	public override void OnUnstackStatus(ITraitable addedTo, bool bySchedule)
	{
		base.OnUnstackStatus(addedTo, bySchedule);
		opinionModifier.IncreaseModifierValue(10);
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = _traitOwner;
	}
}
