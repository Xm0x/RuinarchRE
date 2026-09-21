using System;
using UnityEngine;

namespace Traits;

public class NightZombie : Trait
{
	private Character owner;

	private bool _hasTurnedAtLeastOnce;

	private const int DieTick = 120;

	private const int ReanimateTick = 360;

	public override bool isPersistent => true;

	public override Type serializedData => typeof(SaveDataNightZombie);

	public override bool shouldBeLoadedInMainThread => true;

	public bool hasTurnedAtLeastOnce => _hasTurnedAtLeastOnce;

	public NightZombie()
	{
		name = "Night Zombie";
		description = "Night Zombie";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		isHidden = true;
	}

	public override void LoadTraitSecondWaveInMainThread(SaveDataTrait p_saveDataTrait)
	{
		base.LoadTraitSecondWaveInMainThread(p_saveDataTrait);
		if (p_saveDataTrait is SaveDataNightZombie saveDataNightZombie)
		{
			_hasTurnedAtLeastOnce = saveDataNightZombie.hasTurnedAtLeastOnce;
		}
		else
		{
			_hasTurnedAtLeastOnce = true;
		}
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
			Messenger.AddListener(Signals.HOUR_STARTED, HourlyCheck);
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			owner = character;
			SetMovementSpeed();
			Messenger.AddListener(Signals.HOUR_STARTED, HourlyCheck);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (owner != null)
		{
			UnsetMovementSpeed();
			Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyCheck);
			owner = null;
		}
	}

	private void HourlyCheck()
	{
		int currentTick = GameManager.Instance.currentTick;
		if (currentTick != 120 && currentTick != 360)
		{
			return;
		}
		if (owner.isBeingSeized || (owner.grave != null && owner.grave.isBeingSeized))
		{
			Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_UNSEIZE_POI, OnUnseizePOI);
		}
		else if (!owner.hasMarker)
		{
			Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyCheck);
		}
		else if (owner.marker == null && owner.grave == null)
		{
			Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyCheck);
		}
		else if (currentTick == 120)
		{
			if (!owner.isDead)
			{
				DropTransformingInfected();
				owner.Death();
			}
		}
		else if (owner.isDead)
		{
			DropTransformingInfected();
			Reanimate();
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
		if (owner.hasMarker)
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
	}

	private void SetMovementSpeed()
	{
		owner.movementComponent.AdjustRunSpeedModifier(1f);
		owner.movementComponent.AdjustWalkSpeedModifier(-0.5f);
	}

	private void UnsetMovementSpeed()
	{
		owner.movementComponent.AdjustRunSpeedModifier(-1f);
		owner.movementComponent.AdjustWalkSpeedModifier(0.5f);
	}

	private void OnUnseizePOI(IPointOfInterest poi)
	{
		if (poi != owner)
		{
			return;
		}
		Messenger.RemoveListener<IPointOfInterest>(CharacterSignals.ON_UNSEIZE_POI, OnUnseizePOI);
		if (GameManager.Instance.currentTick >= 120 && GameManager.Instance.currentTick < 360)
		{
			if (!owner.isDead)
			{
				owner.Death();
			}
		}
		else if (owner.isDead)
		{
			Reanimate();
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
	}
}
