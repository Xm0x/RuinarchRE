using System;
using UnityEngine;

namespace Traits;

public class Zapped : Status, IElementalTrait
{
	private GameObject electricEffectGO;

	private IPointOfInterest _owner;

	public bool isPlayerSource { get; private set; }

	public override Type serializedData => typeof(SaveDataZapped);

	public override bool shouldBeLoadedInMainThread => true;

	public Zapped()
	{
		name = "Zapped";
		description = "Jolted and temporarily paralyzed.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = GameManager.Instance.GetTicksBasedOnMinutes(15);
		hindersMovement = true;
		hindersWitness = true;
		hindersPerform = true;
		AddTraitOverrideFunctionIdentifier("Enter_Grid_Tile_Trait");
		AddTraitOverrideFunctionIdentifier("Initiate_Map_Visual_Trait");
		AddTraitOverrideFunctionIdentifier("Destroy_Map_Visual_Trait");
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataZapped saveDataZapped = saveDataTrait as SaveDataZapped;
		isPlayerSource = saveDataZapped.isPlayerSource;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is IPointOfInterest owner)
		{
			_owner = owner;
		}
	}

	public override void LoadTraitSecondWaveInMainThread(SaveDataTrait p_saveDataTrait)
	{
		base.LoadTraitSecondWaveInMainThread(p_saveDataTrait);
		if (electricEffectGO == null)
		{
			electricEffectGO = GameManager.Instance.CreateParticleEffectAt(_owner, PARTICLE_EFFECT.Electric);
		}
		if (electricEffectGO != null)
		{
			AkSoundEngine.PostEvent("Play_Zapped", electricEffectGO);
		}
	}

	public override void OnAddTrait(ITraitable sourcePOI)
	{
		if (sourcePOI is IPointOfInterest pointOfInterest)
		{
			_owner = pointOfInterest;
			electricEffectGO = GameManager.Instance.CreateParticleEffectAt(pointOfInterest, PARTICLE_EFFECT.Electric);
		}
		if (electricEffectGO != null)
		{
			AkSoundEngine.PostEvent("Play_Zapped", electricEffectGO);
		}
		if (sourcePOI is Character)
		{
			Character character = sourcePOI as Character;
			if ((bool)character.marker)
			{
				character.marker.pathfindingAI.ClearAllCurrentPathData();
			}
			if (character.stateComponent.currentState != null)
			{
				character.stateComponent.ExitCurrentState();
			}
			character.combatComponent.ClearHostilesInRange(processCombatBehavior: false);
			character.combatComponent.ClearAvoidInRange(processCombatBehavior: false);
		}
		base.OnAddTrait(sourcePOI);
	}

	public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy)
	{
		if (electricEffectGO != null)
		{
			ObjectPoolManager.Instance.DestroyObject(electricEffectGO);
			electricEffectGO = null;
		}
		if (sourcePOI is Character)
		{
			Character character = sourcePOI as Character;
			if ((bool)character.marker)
			{
				character.combatComponent.ClearHostilesInRange(processCombatBehavior: false);
				character.combatComponent.ClearAvoidInRange(processCombatBehavior: false);
			}
			Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)character);
		}
		base.OnRemoveTrait(sourcePOI, removedBy);
		_owner = null;
	}

	public override void OnEnterGridTile(IPointOfInterest poiWhoEntered, IPointOfInterest owner)
	{
		if (!poiWhoEntered.traitContainer.HasTrait("Zapped"))
		{
			poiWhoEntered.traitContainer.AddTrait(poiWhoEntered, "Zapped", null, bypassElementalChance: false, -1, 0f, ELEMENTAL_TYPE.Electric);
		}
	}

	public override void OnInitiateMapObjectVisual(ITraitable traitable)
	{
		if (traitable is IPointOfInterest poi)
		{
			if ((bool)electricEffectGO)
			{
				ObjectPoolManager.Instance.DestroyObject(electricEffectGO);
				electricEffectGO = null;
			}
			electricEffectGO = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Electric);
		}
	}

	public override void OnDestroyMapObjectVisual(ITraitable traitable)
	{
		if ((bool)electricEffectGO)
		{
			ObjectPoolManager.Instance.DestroyObject(electricEffectGO);
			electricEffectGO = null;
		}
	}

	protected override string GetDescriptionInUI()
	{
		return base.GetDescriptionInUI();
	}

	public void SetIsPlayerSource(bool p_state)
	{
		isPlayerSource = p_state;
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = _owner;
	}
}
