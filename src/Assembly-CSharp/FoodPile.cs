using System;
using Inner_Maps;
using UnityEngine;

public abstract class FoodPile : ResourcePile
{
	private GameObject _manifestFoodParticles;

	public bool isFromManifestFood { get; private set; }

	public FOOD_INFUSE_TYPE infusedType { get; private set; }

	public override Type serializedData => typeof(SaveDataFoodPile);

	protected FoodPile(TILE_OBJECT_TYPE tileObjectType)
		: base(RESOURCE.FOOD)
	{
		Initialize(tileObjectType, shouldAddCommonAdvertisements: false);
		base.traitContainer.AddTrait(this, "Edible");
		SetResourceInPile(20);
		AddAdvertisedAction(INTERACTION_TYPE.PACK_FOOD);
	}

	protected FoodPile(SaveDataTileObject saveDataTileObject)
		: base(saveDataTileObject, RESOURCE.FOOD)
	{
		if (saveDataTileObject is SaveDataFoodPile saveDataFoodPile)
		{
			isFromManifestFood = saveDataFoodPile.isFromManifestFood;
			infusedType = saveDataFoodPile.infusedType;
		}
	}

	public override void LoadSecondWave(SaveDataTileObject data)
	{
		base.LoadSecondWave(data);
		if (isFromManifestFood)
		{
			AddPlayerAction(PLAYER_SKILL_TYPE.LURE);
			AddPlayerAction(PLAYER_SKILL_TYPE.INFUSE);
		}
	}

	public override string ToString()
	{
		return "Food Pile " + base.id;
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		if (gridTileLocation != null && gridTileLocation.structure.structureType != STRUCTURE_TYPE.DWELLING && gridTileLocation.structure.structureType != STRUCTURE_TYPE.CITY_CENTER)
		{
			AddAdvertisedAction(INTERACTION_TYPE.BUY_FOOD);
		}
		else
		{
			RemoveAdvertisedAction(INTERACTION_TYPE.BUY_FOOD);
		}
		if (isFromManifestFood && _manifestFoodParticles == null)
		{
			_manifestFoodParticles = GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Manifest_Food);
		}
	}

	public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true)
	{
		base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
		RemoveAdvertisedAction(INTERACTION_TYPE.BUY_FOOD);
	}

	public override void DestroyMapVisualGameObject()
	{
		base.DestroyMapVisualGameObject();
		_manifestFoodParticles = null;
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		if (_manifestFoodParticles != null)
		{
			ObjectPoolManager.Instance.DestroyObject(_manifestFoodParticles);
			_manifestFoodParticles = null;
		}
		base.OnDestroyPOI(p_destroyer);
	}

	protected override string GetFlavorText()
	{
		string text = LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", "Food_Flavor_Text");
		if (infusedType != FOOD_INFUSE_TYPE.None)
		{
			text = text + "\n" + LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", infusedType.ToStringEnumWithPrefix());
		}
		return text;
	}

	public virtual void ApplyFoodEffectsToConsumer(Character p_consumer)
	{
	}

	public override void GeneralReactionToTileObject(Character actor, ref string debugLog)
	{
		base.GeneralReactionToTileObject(actor, ref debugLog);
		if (isFromManifestFood)
		{
			if (actor.traitContainer.HasTrait("Suspicious") && !actor.traitContainer.HasTrait("Demon Cultist"))
			{
				actor.jobComponent.TriggerDestroy(this, "Destroy_Suspicious");
			}
			else if (actor.race.IsSapient() && !actor.jobQueue.HasJob(JOB_TYPE.MANIFEST_FOOD_EAT) && (actor.needsComponent.isHungry || actor.needsComponent.isStarving))
			{
				GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MANIFEST_FOOD_EAT, INTERACTION_TYPE.EAT, this, actor);
				actor.jobQueue.AddJobInQueue(job);
			}
		}
		if (actor.race.IsSapient() && (base.tileObjectType == TILE_OBJECT_TYPE.ELF_MEAT || base.tileObjectType == TILE_OBJECT_TYPE.HUMAN_MEAT) && !actor.traitContainer.HasTrait("Cannibal") && !actor.traitContainer.HasTrait("Malnourished"))
		{
			if (!actor.defaultCharacterTrait.HasAlreadyReactedToFoodPile(this))
			{
				actor.defaultCharacterTrait.AddFoodPileAsReactedTo(this);
				actor.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, this, "", null, "Saw_Object");
			}
			actor.jobComponent.TryCreateDisposeFoodPileJob(this);
		}
		if (actor.homeSettlement == null || !actor.race.IsSapient())
		{
			return;
		}
		bool flag = false;
		flag = actor.traitContainer.HasTrait("Cannibal") || (base.tileObjectType != TILE_OBJECT_TYPE.HUMAN_MEAT && base.tileObjectType != TILE_OBJECT_TYPE.ELF_MEAT);
		if (!flag || !actor.needsComponent.HasNeeds() || actor.needsComponent.isStarving || actor.partyComponent.isActiveMember || actor.homeStructure == null || gridTileLocation == null || gridTileLocation.structure == actor.homeStructure || gridTileLocation.structure.structureType == STRUCTURE_TYPE.DWELLING || gridTileLocation.structure.structureType.IsFoodProducingStructure() || actor.jobQueue.HasJob(JOB_TYPE.STOCKPILE_FOOD, JOB_TYPE.HAUL, JOB_TYPE.HAUL_ON_SIGHT) || !actor.movementComponent.HasPathToEvenIfDiffRegion(actor.homeStructure) || actor.traitContainer.HasTrait("Vampire") || actor.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.HAUL_ON_SIGHT) || (base.characterOwner != null && !IsOwnedBy(actor) && !actor.traitContainer.HasTrait("Kleptomaniac")))
		{
			return;
		}
		if (actor.traitContainer.HasTrait("Kleptomaniac"))
		{
			if (!actor.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.KLEPTOMANIAC_STEAL))
			{
				actor.jobComponent.CreateStealItemJob(JOB_TYPE.KLEPTOMANIAC_STEAL, this);
			}
		}
		else
		{
			actor.jobComponent.CreateDropItemJob(JOB_TYPE.HAUL_ON_SIGHT, this, actor.homeStructure);
		}
	}

	public void SetFoodAsFromManifestFood()
	{
		isFromManifestFood = true;
		AddPlayerAction(PLAYER_SKILL_TYPE.LURE);
		AddPlayerAction(PLAYER_SKILL_TYPE.INFUSE);
		_manifestFoodParticles = GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Manifest_Food);
	}

	public void SetInfusedType(FOOD_INFUSE_TYPE p_infuseType)
	{
		infusedType = p_infuseType;
		if (UIManager.Instance != null && UIManager.Instance.tileObjectInfoUI.isShowing && UIManager.Instance.tileObjectInfoUI.activeTileObject == this)
		{
			UIManager.Instance.UpdateTileObjectInfo();
		}
	}
}
