using System;
using UnityEngine;
using UtilityScripts;

namespace Traits;

public class FiniteZombie : Trait
{
	protected Character owner;

	protected bool _hasTurnedAtLeastOnce;

	public override bool isPersistent => true;

	public override Type serializedData => typeof(SaveDataFiniteZombie);

	public override bool shouldBeLoadedInMainThread => true;

	public bool hasTurnedAtLeastOnce => _hasTurnedAtLeastOnce;

	public FiniteZombie()
	{
		name = "Finite Zombie";
		description = "Finite Zombie";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		isHidden = true;
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		if (saveDataTrait is SaveDataFiniteZombie saveDataFiniteZombie)
		{
			_hasTurnedAtLeastOnce = saveDataFiniteZombie.hasTurnedAtLeastOnce;
		}
		else
		{
			_hasTurnedAtLeastOnce = true;
		}
	}

	public override void LoadTraitSecondWaveInMainThread(SaveDataTrait p_saveDataTrait)
	{
		base.LoadTraitSecondWaveInMainThread(p_saveDataTrait);
		if (owner is Summon || owner.minion != null)
		{
			owner.visuals.UsePreviousClassAsset(p_state: true);
			owner.visuals.UpdateAllVisuals(owner);
		}
		else if (!_hasTurnedAtLeastOnce)
		{
			owner.visuals.UsePreviousClassAsset(p_state: true);
			owner.visuals.UpdateAllVisuals(owner);
		}
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character character)
		{
			owner = character;
			if (!_hasTurnedAtLeastOnce)
			{
				Messenger.AddListener(Signals.HOUR_STARTED, HourlyCheck);
			}
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			owner = character;
			Messenger.AddListener(Signals.HOUR_STARTED, HourlyCheck);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (owner != null)
		{
			Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyCheck);
			owner = null;
		}
	}

	private void HourlyCheck()
	{
		if (owner.isDead && GameUtilities.RollChance(50) && !owner.isBeingSeized && (owner.grave == null || !owner.grave.isBeingSeized))
		{
			if (!owner.hasMarker)
			{
				Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyCheck);
				return;
			}
			if (!owner.hasMarker && owner.grave == null)
			{
				Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyCheck);
				return;
			}
			DropTransformingInfected();
			Reanimate();
			Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyCheck);
		}
	}

	private void DropTransformingInfected()
	{
		if (owner.isBeingCarriedBy != null && owner.isBeingCarriedBy.carryComponent.IsPOICarried(owner))
		{
			owner.isBeingCarriedBy.StopCurrentActionNode();
		}
		else if (owner.grave != null && owner.grave.isBeingCarriedBy != null && owner.grave.isBeingCarriedBy.carryComponent.IsPOICarried(owner.grave))
		{
			owner.grave.isBeingCarriedBy.StopCurrentActionNode();
		}
		if (owner.isBeingCarriedBy != null)
		{
			owner.isBeingCarriedBy.UncarryPOI(owner);
		}
		else if (owner.grave != null && owner.grave.isBeingCarriedBy != null)
		{
			owner.grave.isBeingCarriedBy.UncarryPOI(owner.grave);
		}
	}

	private void Reanimate()
	{
		_hasTurnedAtLeastOnce = true;
		if (owner is Summon || owner.minion != null)
		{
			owner.visuals.UsePreviousClassAsset(p_state: true);
			owner.visuals.SetBaseMarkerTint(Color.grey);
		}
		else
		{
			owner.visuals.UsePreviousClassAsset(p_state: false);
		}
		CharacterManager.Instance.RaiseFromDeadRetainCharacterInstance(owner, FactionManager.Instance.undeadFaction, RACE.ZOMBIE, owner.characterClass.className);
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
	}
}
