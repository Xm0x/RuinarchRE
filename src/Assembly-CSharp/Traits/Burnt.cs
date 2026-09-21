using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

namespace Traits;

public class Burnt : Status
{
	private GameObject _burntEffect;

	private ITraitable _owner;

	private Color burntColor => Color.gray;

	public override bool shouldBeLoadedInMainThread => true;

	public Burnt()
	{
		name = "Burnt";
		description = "Was ravaged by fire.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(48);
		isTangible = true;
		moodEffect = -7;
		advertisedInteractions = new List<INTERACTION_TYPE>
		{
			INTERACTION_TYPE.CLEAN_UP,
			INTERACTION_TYPE.HEALER_CURE
		};
		AddTraitOverrideFunctionIdentifier("Initiate_Map_Visual_Trait");
		AddTraitOverrideFunctionIdentifier("Destroy_Map_Visual_Trait");
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		_owner = addTo;
	}

	public override void LoadTraitSecondWaveInMainThread(SaveDataTrait p_saveDataTrait)
	{
		base.LoadTraitSecondWaveInMainThread(p_saveDataTrait);
		UpdateVisualsOnAdd(_owner);
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		_owner = addedTo;
		UpdateVisualsOnAdd(_owner);
		if (addedTo is BaseBed baseBed)
		{
			baseBed.SetPOIState(POI_STATE.INACTIVE);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		UpdateVisualsOnRemove(removedFrom);
		if (removedFrom is BaseBed baseBed)
		{
			baseBed.SetPOIState(POI_STATE.ACTIVE);
		}
		_owner = null;
	}

	public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob)
	{
		if (traitOwner is TileObject { tileObjectType: not TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT } tileObject && tileObject.Advertises(INTERACTION_TYPE.REPAIR) && tileObject.GetJobTargetingThisCharacter(JOB_TYPE.REPAIR) == null && InteractionManager.Instance.CanCharacterTakeRepairJob(characterThatWillDoJob, tileObject))
		{
			GoapEffect goapEffectData = InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Burnt", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPAIR, goapEffectData, tileObject, characterThatWillDoJob);
			JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(characterThatWillDoJob, goapPlanJob, INTERACTION_TYPE.TAKE_RESOURCE);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[1] { TileObjectDB.GetTileObjectData(tileObject.tileObjectType).mainRecipe });
			characterThatWillDoJob.jobQueue.AddJobInQueue(goapPlanJob);
			return true;
		}
		return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
	}

	public override void OnInitiateMapObjectVisual(ITraitable traitable)
	{
		UpdateVisualsOnAdd(traitable);
	}

	public override void OnDestroyMapObjectVisual(ITraitable traitable)
	{
		UpdateVisualsOnRemove(traitable);
	}

	private void UpdateVisualsOnAdd(ITraitable traitable)
	{
		if (traitable is BaseMapObject baseMapObject && baseMapObject.baseMapObjectVisual != null)
		{
			if (baseMapObject is GenericTileObject { gridTileLocation: var gridTileLocation })
			{
				Sprite sprite = gridTileLocation.parentMap.groundTilemap.GetSprite(gridTileLocation.localPlace);
				baseMapObject.baseMapObjectVisual.SetVisual(sprite);
				gridTileLocation.parentTileMap.SetColor(gridTileLocation.localPlace, burntColor);
				gridTileLocation.parentMap.detailsTilemap.SetColor(gridTileLocation.localPlace, burntColor);
				gridTileLocation.parentMap.northEdgeTilemap.SetColor(gridTileLocation.localPlace, burntColor);
				gridTileLocation.parentMap.southEdgeTilemap.SetColor(gridTileLocation.localPlace, burntColor);
				gridTileLocation.parentMap.eastEdgeTilemap.SetColor(gridTileLocation.localPlace, burntColor);
				gridTileLocation.parentMap.westEdgeTilemap.SetColor(gridTileLocation.localPlace, burntColor);
			}
			if (traitable is IPointOfInterest poi)
			{
				_burntEffect = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Burnt);
			}
			baseMapObject.baseMapObjectVisual.SetMaterial(InnerMapManager.Instance.assetManager.burntMaterial);
		}
	}

	private void UpdateVisualsOnRemove(ITraitable traitable)
	{
		if (traitable is BaseMapObject baseMapObject && baseMapObject.baseMapObjectVisual != null)
		{
			if (baseMapObject is GenericTileObject { gridTileLocation: var gridTileLocation })
			{
				gridTileLocation.parentTileMap.SetColor(gridTileLocation.localPlace, Color.white);
				gridTileLocation.parentMap.detailsTilemap.SetColor(gridTileLocation.localPlace, Color.white);
				gridTileLocation.parentMap.northEdgeTilemap.SetColor(gridTileLocation.localPlace, Color.white);
				gridTileLocation.parentMap.southEdgeTilemap.SetColor(gridTileLocation.localPlace, Color.white);
				gridTileLocation.parentMap.eastEdgeTilemap.SetColor(gridTileLocation.localPlace, Color.white);
				gridTileLocation.parentMap.westEdgeTilemap.SetColor(gridTileLocation.localPlace, Color.white);
			}
			baseMapObject.baseMapObjectVisual.SetMaterial(InnerMapManager.Instance.assetManager.defaultObjectMaterial);
		}
		if (_burntEffect != null)
		{
			ObjectPoolManager.Instance.DestroyObject(_burntEffect);
			_burntEffect = null;
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = _owner;
	}
}
