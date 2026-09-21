using System;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

namespace Traits;

public class Poisoned : Status, IElementalTrait
{
	private Character characterOwner;

	private GameObject _poisonedEffect;

	private string _destroyScheduleKey;

	public List<Character> awareCharacters { get; }

	private ITraitable traitable { get; set; }

	public Character cleanser { get; private set; }

	public bool isVenomous { get; private set; }

	public bool isPlayerSource { get; private set; }

	public GameDate destroyDate { get; private set; }

	public override Type serializedData => typeof(SaveDataPoisoned);

	public override bool shouldBeLoadedInMainThread => true;

	public Poisoned()
	{
		name = "Poisoned";
		description = "Is suffering from progressive damage. | Is full of poison.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(4);
		isTangible = true;
		advertisedInteractions = new List<INTERACTION_TYPE>
		{
			INTERACTION_TYPE.REMOVE_POISON,
			INTERACTION_TYPE.HEALER_CURE
		};
		awareCharacters = new List<Character>();
		mutuallyExclusive = new string[1] { "Robust" };
		moodEffect = -6;
		isStacking = true;
		stackLimit = 3;
		stackModifier = 0.5f;
		AddTraitOverrideFunctionIdentifier("Initiate_Map_Visual_Trait");
		AddTraitOverrideFunctionIdentifier("Destroy_Map_Visual_Trait");
		AddTraitOverrideFunctionIdentifier("Execute_Pre_Effect_Trait");
		AddTraitOverrideFunctionIdentifier("Tick_Started_Trait");
		AddTraitOverrideFunctionIdentifier("Villager_Reaction");
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataPoisoned saveDataPoisoned = saveDataTrait as SaveDataPoisoned;
		isVenomous = saveDataPoisoned.isVenomous;
		isPlayerSource = saveDataPoisoned.isPlayerSource;
		destroyDate = saveDataPoisoned.destroyDate;
	}

	public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
		SaveDataPoisoned saveDataPoisoned = p_saveDataTrait as SaveDataPoisoned;
		awareCharacters.AddRange(SaveUtilities.ConvertIDListToCharacters(saveDataPoisoned.awareCharacterIDs));
		if (destroyDate.hasValue)
		{
			_destroyScheduleKey = SchedulingManager.Instance.AddEntry(destroyDate, ExpireTreeAfter24Hours, this);
		}
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		traitable = addTo;
		if (traitable is Character character)
		{
			characterOwner = character;
		}
	}

	public override void LoadTraitSecondWaveInMainThread(SaveDataTrait p_saveDataTrait)
	{
		base.LoadTraitSecondWaveInMainThread(p_saveDataTrait);
		UpdateVisualsOnAdd(traitable);
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		traitable = addedTo;
		isVenomous = addedTo.traitContainer.HasTrait("Venomous");
		if (traitable is Character character)
		{
			characterOwner = character;
			if (!isVenomous)
			{
				characterOwner.AdjustDoNotRecoverHP(1);
			}
		}
		else if (addedTo is TileObject)
		{
			ticksDuration = GameManager.Instance.GetTicksBasedOnHour(24);
			if (traitable is GenericTileObject genericTileObject)
			{
				genericTileObject.AddAdvertisedAction(INTERACTION_TYPE.CLEANSE_TILE);
				if (genericTileObject.gridTileLocation.groundType == LocationGridTile.Ground_Type.Desert_Grass || genericTileObject.gridTileLocation.groundType == LocationGridTile.Ground_Type.Desert_Stone || genericTileObject.gridTileLocation.groundType == LocationGridTile.Ground_Type.Sand)
				{
					ticksDuration = GameManager.Instance.GetTicksBasedOnHour(2);
				}
			}
			else if (traitable is Crops crops)
			{
				crops.AdjustGrowthRate(-1);
			}
			else if (traitable is TreeObject)
			{
				destroyDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(24));
				_destroyScheduleKey = SchedulingManager.Instance.AddEntry(destroyDate, ExpireTreeAfter24Hours, this);
			}
			addedTo.traitContainer.RemoveStatusAndStacks(addedTo, "Wet");
		}
		UpdateVisualsOnAdd(addedTo);
	}

	public override void OnStackStatus(ITraitable addedTo)
	{
		base.OnStackStatus(addedTo);
		UpdateVisualsOnAdd(addedTo);
	}

	public override void OnStackStatusAddedButStackIsAtLimit(ITraitable traitable)
	{
		base.OnStackStatusAddedButStackIsAtLimit(traitable);
		UpdateVisualsOnAdd(traitable);
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		UpdateVisualsOnRemove(removedFrom);
		if (!isVenomous)
		{
			characterOwner?.AdjustDoNotRecoverHP(-1);
		}
		if (traitable is GenericTileObject genericTileObject)
		{
			genericTileObject.RemoveAdvertisedAction(INTERACTION_TYPE.CLEANSE_TILE);
		}
		else if (traitable is Crops crops)
		{
			crops.AdjustGrowthRate(1);
		}
		else if (traitable is Character p_owner)
		{
			DisablePlayerSourceChaosOrb(p_owner);
		}
		awareCharacters.Clear();
		base.responsibleCharacters?.Clear();
		if (!string.IsNullOrEmpty(_destroyScheduleKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_destroyScheduleKey);
		}
		characterOwner = null;
		traitable = null;
		cleanser = null;
	}

	public override void ExecuteActionAfterEffects(INTERACTION_TYPE action, Character actor, IPointOfInterest target, ACTION_CATEGORY category, ref bool isRemoved)
	{
		base.ExecuteActionAfterEffects(action, actor, target, category, ref isRemoved);
		if (category != ACTION_CATEGORY.CONSUME || !(traitable is IPointOfInterest pointOfInterest) || target != pointOfInterest)
		{
			return;
		}
		actor.interruptComponent.TriggerInterrupt(INTERRUPT.Ingested_Poison, pointOfInterest);
		if (target is Table p_tileObject && !actor.isDead)
		{
			actor.jobComponent.TryCreateCleanItemJob(p_tileObject, out var p_producedJob);
			if (p_producedJob != null)
			{
				actor.jobQueue.AddJobInQueue(p_producedJob);
			}
		}
	}

	public override void OnTickStarted(ITraitable traitable)
	{
		base.OnTickStarted(traitable);
		if (!isVenomous)
		{
			traitable.AdjustHP(-Mathf.RoundToInt(traitable.traitContainer.stacks[name]), ELEMENTAL_TYPE.Normal, triggerDeath: true, null, null, showHPBar: true, 0f, isPlayerSource);
		}
	}

	public override void OnInitiateMapObjectVisual(ITraitable traitable)
	{
		if (traitable is IPointOfInterest poi)
		{
			if ((bool)_poisonedEffect)
			{
				ObjectPoolManager.Instance.DestroyObject(_poisonedEffect);
				_poisonedEffect = null;
			}
			_poisonedEffect = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Poison, allowRotation: false);
		}
	}

	public override void OnDestroyMapObjectVisual(ITraitable traitable)
	{
		if ((bool)_poisonedEffect)
		{
			ObjectPoolManager.Instance.DestroyObject(_poisonedEffect);
			_poisonedEffect = null;
		}
	}

	public override string GetTestingData(ITraitable traitable = null)
	{
		string text = base.GetTestingData(traitable);
		if (traitable is TreeObject)
		{
			text = text + "\n\tDestroy Date: " + destroyDate.ToString();
		}
		text = text + "\n\tCleanser: " + (cleanser?.name ?? "None");
		text += "\n\tAware Characters: ";
		for (int i = 0; i < awareCharacters.Count; i++)
		{
			Character character = awareCharacters[i];
			text = text + character.name + ",";
		}
		return text;
	}

	public override void OnCopyStatus(Status statusToCopy, ITraitable from, ITraitable to)
	{
		base.OnCopyStatus(statusToCopy, from, to);
		if (statusToCopy is Poisoned poisoned)
		{
			awareCharacters.AddRange(poisoned.awareCharacters);
			cleanser = poisoned.cleanser;
			isVenomous = poisoned.isVenomous;
		}
	}

	protected override string GetDescriptionInUI()
	{
		return base.GetDescriptionInUI();
	}

	public override void DisconnectFromCharacter(IPointOfInterest p_owner, Character p_character)
	{
		base.DisconnectFromCharacter(p_owner, p_character);
		if (cleanser == p_character)
		{
			SetCleanser(null);
		}
		characterOwner = null;
		awareCharacters.Remove(p_character);
	}

	public void AddAwareCharacter(Character character)
	{
		if (!awareCharacters.Contains(character))
		{
			awareCharacters.Add(character);
			if (traitable is TileObject tileObject && base.responsibleCharacter != null && (!CharacterManager.Instance.IsCultistOfSameReligion(character, base.responsibleCharacter) || tileObject.IsOwnedBy(character)))
			{
				character.jobComponent.TriggerRemoveStatusTarget(tileObject, "Poisoned");
			}
		}
	}

	public void RemoveAwareCharacter(Character character)
	{
		awareCharacters.Remove(character);
	}

	public void SetIsVenomous()
	{
		if (!isVenomous)
		{
			isVenomous = true;
			characterOwner.AdjustDoNotRecoverHP(1);
		}
	}

	private void UpdateVisualsOnAdd(ITraitable addedTo)
	{
		if (addedTo is IPointOfInterest pointOfInterest && _poisonedEffect == null && !(pointOfInterest is MovingTileObject))
		{
			_poisonedEffect = GameManager.Instance.CreateParticleEffectAt(pointOfInterest, PARTICLE_EFFECT.Poison, allowRotation: false);
		}
		if (addedTo is TileObject tileObject && tileObject is GenericTileObject)
		{
			tileObject.gridTileLocation.parentMap.SetUpperGroundVisual(tileObject.gridTileLocation.localPlace, InnerMapManager.Instance.assetManager.poisonRuleTile);
			tileObject.gridTileLocation.ClearAllSeamlessEdges(tileObject.gridTileLocation.parentMap);
		}
	}

	private void UpdateVisualsOnRemove(ITraitable removedFrom)
	{
		if ((bool)_poisonedEffect)
		{
			ObjectPoolManager.Instance.DestroyObject(_poisonedEffect);
			_poisonedEffect = null;
		}
		if (removedFrom is TileObject tileObject && tileObject is GenericTileObject)
		{
			tileObject.gridTileLocation.parentMap.SetUpperGroundVisual(tileObject.gridTileLocation.localPlace, null);
			tileObject.gridTileLocation.CreateSeamlessEdgesForTile(tileObject.gridTileLocation.parentMap);
		}
	}

	public void SetCleanser(Character character)
	{
		cleanser = character;
		if (cleanser == null)
		{
			Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
		}
		else
		{
			Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
		}
	}

	private void OnJobRemovedFromCharacter(JobQueueItem jqi, Character character)
	{
		if (cleanser == character && jqi.jobType == JOB_TYPE.CLEANSE_TILES)
		{
			SetCleanser(null);
		}
	}

	public void SetIsPlayerSource(bool p_state)
	{
		if (isPlayerSource == p_state)
		{
			return;
		}
		isPlayerSource = p_state;
		if (traitable is Character p_owner)
		{
			if (isPlayerSource)
			{
				EnablePlayerSourceChaosOrb(p_owner);
			}
			else
			{
				DisablePlayerSourceChaosOrb(p_owner);
			}
		}
		else if (traitable is TileObject && traitable.traitContainer.HasTrait("Edible"))
		{
			PlayerManager.Instance.player.goalComponent.CompleteSubGoal(SUB_GOAL.GOAL_POISON_FOOD);
		}
	}

	public override void VillagerReactionToTileObjectTrait(TileObject owner, Character actor, ref string debugLog)
	{
		base.VillagerReactionToTileObjectTrait(owner, actor, ref debugLog);
		Lazy traitOrStatus = actor.traitContainer.GetTraitOrStatus<Lazy>("Lazy");
		if (actor.combatComponent.isInActualCombat || actor.defaultCharacterTrait.hasSeenPoisoned || (traitOrStatus != null && traitOrStatus.TryIgnoreUrgentTask(JOB_TYPE.CLEANSE_TILES)) || owner.gridTileLocation == null || actor.homeSettlement == null || !owner.gridTileLocation.IsPartOfSettlement(actor.homeSettlement) || actor.jobQueue.HasJob(JOB_TYPE.CLEANSE_TILES))
		{
			return;
		}
		actor.defaultCharacterTrait.SetHasSeenPoisoned(state: true);
		actor.homeSettlement.settlementJobTriggerComponent.TriggerCleanseTiles();
		for (int i = 0; i < actor.homeSettlement.availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = actor.homeSettlement.availableJobs[i];
			if (jobQueueItem.jobType == JOB_TYPE.CLEANSE_TILES && jobQueueItem.assignedCharacter == null && actor.jobQueue.CanJobBeAddedToQueue(jobQueueItem))
			{
				actor.jobQueue.AddJobInQueue(jobQueueItem);
			}
		}
	}

	private void ExpireTreeAfter24Hours()
	{
		if (traitable is TreeObject treeObject)
		{
			_destroyScheduleKey = string.Empty;
			if (treeObject.gridTileLocation != null)
			{
				treeObject.gridTileLocation.structure.RemovePOI(treeObject);
			}
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = traitable;
		_ = cleanser;
		_ = characterOwner;
		awareCharacters.Contains(p_character);
	}
}
