using System;
using System.Collections.Generic;
using Characters.Behaviour;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

namespace Traits;

public class Burning : Status, IElementalTrait
{
	private GameObject burningEffect;

	private readonly List<ITraitable> _burningSpreadChoices;

	private bool _hasBeenRemoved;

	private ITraitable owner { get; set; }

	public BurningSource sourceOfBurning { get; private set; }

	public bool isPlayerSource { get; private set; }

	public override bool isPersistent => true;

	public Character douser { get; private set; }

	public override Type serializedData => typeof(SaveDataBurning);

	public override bool shouldBeLoadedInMainThread => true;

	public Burning()
	{
		name = "Burning";
		description = "On fire!";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(3);
		isTangible = true;
		hindersSocials = true;
		moodEffect = -25;
		_burningSpreadChoices = new List<ITraitable>();
		advertisedInteractions = new List<INTERACTION_TYPE> { INTERACTION_TYPE.EXTRACT_ITEM };
		AddTraitOverrideFunctionIdentifier("Initiate_Map_Visual_Trait");
		AddTraitOverrideFunctionIdentifier("Destroy_Map_Visual_Trait");
		AddTraitOverrideFunctionIdentifier("Execute_Pre_Effect_Trait");
		AddTraitOverrideFunctionIdentifier("Execute_Pre_Effect_Trait");
		AddTraitOverrideFunctionIdentifier("Death_Trait");
		AddTraitOverrideFunctionIdentifier("Villager_Reaction");
		AddTraitOverrideFunctionIdentifier("See_Poi_Trait");
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataBurning saveDataBurning = saveDataTrait as SaveDataBurning;
		BurningSource orCreateBurningSourceWithID = DatabaseManager.Instance.burningSourceDatabase.GetOrCreateBurningSourceWithID(saveDataBurning.persistentID);
		LoadSourceOfBurning(orCreateBurningSourceWithID);
		isPlayerSource = saveDataBurning.isPlayerSource;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		owner = addTo;
	}

	public override void LoadTraitSecondWaveInMainThread(SaveDataTrait p_saveDataTrait)
	{
		base.LoadTraitSecondWaveInMainThread(p_saveDataTrait);
		if (owner is IPointOfInterest poi)
		{
			if (burningEffect == null)
			{
				burningEffect = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Burning, allowRotation: false);
			}
		}
		else if (owner is ThinWall wallObject && burningEffect == null)
		{
			burningEffect = GameManager.Instance.CreateParticleEffectAt(wallObject, PARTICLE_EFFECT.Burning);
		}
		sourceOfBurning?.AddObjectOnFire(owner);
		Messenger.AddListener(Signals.TICK_ENDED, PerTickEnded);
		if (owner is Character)
		{
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_MARKER_EXPIRED, OnCharacterMarkerExpired);
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		owner = addedTo;
		if (addedTo is IPointOfInterest pointOfInterest)
		{
			pointOfInterest.AddAdvertisedAction(INTERACTION_TYPE.DOUSE_FIRE);
			burningEffect = GameManager.Instance.CreateParticleEffectAt(pointOfInterest, PARTICLE_EFFECT.Burning, allowRotation: false);
			if (pointOfInterest is Character character)
			{
				character.AdjustDoNotRecoverHP(1);
				if (character.limiterComponent.canMove && character.limiterComponent.canWitness && character.limiterComponent.canPerform)
				{
					CreateJobsOnEnterVisionBasedOnTrait(character, character);
				}
				CharacterBurningProcess(character);
			}
			else
			{
				pointOfInterest.SetPOIState(POI_STATE.INACTIVE);
			}
			if (pointOfInterest is WinterRose winterRose)
			{
				winterRose.WinterRoseEffect();
			}
			else
			{
				Messenger.Broadcast(CharacterSignals.REPROCESS_POI, pointOfInterest);
			}
		}
		if (sourceOfBurning != null && !sourceOfBurning.objectsOnFire.Contains(owner))
		{
			SetSourceOfBurning(sourceOfBurning, owner);
		}
		Messenger.AddListener(Signals.TICK_ENDED, PerTickEnded);
		if (addedTo is Character)
		{
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_MARKER_EXPIRED, OnCharacterMarkerExpired);
		}
		base.OnAddTrait(addedTo);
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		_hasBeenRemoved = true;
		SetDouser(null);
		SetSourceOfBurning(null, removedFrom);
		Messenger.RemoveListener(Signals.TICK_ENDED, PerTickEnded);
		if (removedFrom is Character)
		{
			Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_MARKER_EXPIRED, OnCharacterMarkerExpired);
		}
		if ((bool)burningEffect)
		{
			ObjectPoolManager.Instance.DestroyObject(burningEffect);
			burningEffect = null;
		}
		if (removedFrom is IPointOfInterest pointOfInterest)
		{
			pointOfInterest.RemoveAdvertisedAction(INTERACTION_TYPE.DOUSE_FIRE);
			if (removedFrom is Character character)
			{
				character.AdjustDoNotRecoverHP(-1);
				DisablePlayerSourceChaosOrb(character);
			}
			else if (pointOfInterest is Bed bed)
			{
				if (bed.IsSlotAvailable())
				{
					pointOfInterest.SetPOIState(POI_STATE.ACTIVE);
				}
			}
			else
			{
				pointOfInterest.SetPOIState(POI_STATE.ACTIVE);
				if (pointOfInterest is Cinder { gridTileLocation: not null } cinder)
				{
					cinder.gridTileLocation.structure.RemovePOI(cinder);
				}
			}
		}
		owner = null;
	}

	public override void OnRemoveStatusBySchedule(ITraitable removedFrom)
	{
		base.OnRemoveStatusBySchedule(removedFrom);
		removedFrom.traitContainer.AddTrait(removedFrom, "Burnt");
	}

	public override bool OnDeath(Character character)
	{
		return character.traitContainer.RemoveTrait(character, this);
	}

	public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob)
	{
		if (targetPOI.gridTileLocation != null && characterThatWillDoJob.homeSettlement != null && (targetPOI.gridTileLocation.IsPartOfSettlement(characterThatWillDoJob.homeSettlement) || targetPOI.gridTileLocation.structure.settlementLocation == characterThatWillDoJob.homeSettlement) && !characterThatWillDoJob.traitContainer.HasTrait("Pyromaniac") && CanTriggerDouseFire())
		{
			characterThatWillDoJob.homeSettlement.settlementJobTriggerComponent.TriggerDouseFire();
		}
		characterThatWillDoJob.traitContainer.GetTraitOrStatus<Pyrophobic>("Pyrophobic")?.AddKnownBurningSource(sourceOfBurning, targetPOI);
		return base.OnSeePOI(targetPOI, characterThatWillDoJob);
	}

	public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob)
	{
		if (traitOwner.gridTileLocation != null && characterThatWillDoJob.homeSettlement != null && (traitOwner.gridTileLocation.IsPartOfSettlement(characterThatWillDoJob.homeSettlement) || traitOwner.gridTileLocation.structure.settlementLocation == characterThatWillDoJob.homeSettlement) && !characterThatWillDoJob.traitContainer.HasTrait("Pyromaniac") && CanTriggerDouseFire())
		{
			characterThatWillDoJob.homeSettlement.settlementJobTriggerComponent.TriggerDouseFire();
			if (!characterThatWillDoJob.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.DOUSE_FIRE))
			{
				JobQueueItem firstJobOfTypeThatCanBeAssignedTo = characterThatWillDoJob.homeSettlement.GetFirstJobOfTypeThatCanBeAssignedTo(JOB_TYPE.DOUSE_FIRE, characterThatWillDoJob);
				if (firstJobOfTypeThatCanBeAssignedTo != null)
				{
					characterThatWillDoJob.jobQueue.AddJobInQueue(firstJobOfTypeThatCanBeAssignedTo);
				}
			}
		}
		characterThatWillDoJob.traitContainer.GetTraitOrStatus<Pyrophobic>("Pyrophobic")?.AddKnownBurningSource(sourceOfBurning, traitOwner);
		return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
	}

	public override string GetTestingData(ITraitable traitable = null)
	{
		if (sourceOfBurning == null)
		{
			return base.GetTestingData(traitable);
		}
		return string.Format("Douser: {0}. {1}", douser?.name ?? "None", sourceOfBurning);
	}

	public override void ExecuteActionPreEffects(INTERACTION_TYPE action, ActualGoapNode goapNode)
	{
		base.ExecuteActionPreEffects(action, goapNode);
		if ((goapNode.action.actionCategory == ACTION_CATEGORY.CONSUME || goapNode.action.actionCategory == ACTION_CATEGORY.DIRECT) && UnityEngine.Random.Range(0, 100) < 10 && goapNode.actor.traitContainer.HasTrait("Flammable"))
		{
			goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Burning", out var trait, null, bypassElementalChance: true, -1, 0f, ELEMENTAL_TYPE.Fire);
			(trait as Burning)?.SetSourceOfBurning(sourceOfBurning, goapNode.actor);
			goapNode.actor.traitContainer.GetTraitOrStatus<Burning>("Burning")?.SetIsPlayerSource(isPlayerSource);
		}
	}

	public override void ExecuteActionPerTickEffects(INTERACTION_TYPE action, ActualGoapNode goapNode)
	{
		base.ExecuteActionPerTickEffects(action, goapNode);
		if ((goapNode.action.actionCategory == ACTION_CATEGORY.CONSUME || goapNode.action.actionCategory == ACTION_CATEGORY.DIRECT) && UnityEngine.Random.Range(0, 100) < 10 && goapNode.actor.traitContainer.HasTrait("Flammable"))
		{
			goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Burning", out var trait, null, bypassElementalChance: true, -1, 0f, ELEMENTAL_TYPE.Fire);
			(trait as Burning)?.SetSourceOfBurning(sourceOfBurning, goapNode.actor);
			goapNode.actor.traitContainer.GetTraitOrStatus<Burning>("Burning")?.SetIsPlayerSource(isPlayerSource);
		}
	}

	public override void OnInitiateMapObjectVisual(ITraitable traitable)
	{
		if (traitable is IPointOfInterest poi)
		{
			if ((bool)burningEffect)
			{
				ObjectPoolManager.Instance.DestroyObject(burningEffect);
				burningEffect = null;
			}
			burningEffect = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Burning, allowRotation: false);
		}
	}

	public override void OnDestroyMapObjectVisual(ITraitable traitable)
	{
		if ((bool)burningEffect)
		{
			ObjectPoolManager.Instance.DestroyObject(burningEffect);
			burningEffect = null;
		}
	}

	protected override string GetDescriptionInUI()
	{
		return base.GetDescriptionInUI();
	}

	public void LoadSourceOfBurning(BurningSource source)
	{
		sourceOfBurning = source;
	}

	public void SetSourceOfBurning(BurningSource source, ITraitable obj)
	{
		if (source == null || !source.hasBeenCleanedUp)
		{
			sourceOfBurning = source;
			sourceOfBurning?.AddObjectOnFire(obj);
		}
	}

	private void OnCharacterMarkerExpired(Character character)
	{
		if (owner == character)
		{
			character.traitContainer.RemoveTrait(character, this);
		}
	}

	private void PerTickEnded()
	{
		if (_hasBeenRemoved || PlayerManager.Instance.player.seizeComponent.seizedPOI == owner || owner.gridTileLocation == null || (owner is GenericTileObject && owner.gridTileLocation.structure.structureType.IsSpecialStructure()))
		{
			return;
		}
		if (owner is BaseBed baseBed)
		{
			owner.AdjustHP(-2, ELEMENTAL_TYPE.Normal, triggerDeath: true, this, null, showHPBar: true, 0f, isPlayerSource);
			if (baseBed.users != null && baseBed.users.Length != 0)
			{
				for (int i = 0; i < baseBed.users.Length; i++)
				{
					baseBed.users[i]?.AdjustHP(-2, ELEMENTAL_TYPE.Normal, triggerDeath: true, this, null, showHPBar: true, 0f, isPlayerSource);
				}
			}
		}
		else if (!(owner is Cinder))
		{
			owner.AdjustHP(-2, ELEMENTAL_TYPE.Normal, triggerDeath: true, this, null, showHPBar: true, 0f, isPlayerSource);
		}
		if (!GameUtilities.RollChance(2))
		{
			return;
		}
		_burningSpreadChoices.Clear();
		if (!ShouldSpreadFire())
		{
			return;
		}
		LocationGridTile gridTileLocation = owner.gridTileLocation;
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		gridTileLocation.PopulateTilesInRadius(list, 1, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
		for (int j = 0; j < list.Count; j++)
		{
			LocationGridTile locationGridTile = list[j];
			if (locationGridTile.tileObjectComponent.objHere != null && locationGridTile.tileObjectComponent.objHere.tileObjectType == TILE_OBJECT_TYPE.ICE_BLOCK_WALL)
			{
				continue;
			}
			List<ITraitable> list2 = RuinarchListPool<ITraitable>.Claim();
			locationGridTile.PopulateTraitablesOnTileThatCanHaveElementalTrait(list2, "Burning", bypassElementalChance: false, 0f, ELEMENTAL_TYPE.Fire);
			for (int k = 0; k < list2.Count; k++)
			{
				ITraitable traitable = list2[k];
				if (traitable.traitContainer.HasTrait("Flammable"))
				{
					_burningSpreadChoices.Add(traitable);
				}
			}
			RuinarchListPool<ITraitable>.Release(list2);
		}
		if (_burningSpreadChoices.Count > 0)
		{
			ITraitable traitable2 = _burningSpreadChoices[UnityEngine.Random.Range(0, _burningSpreadChoices.Count)];
			if (traitable2.gridTileLocation != null)
			{
				bool num = owner.gridTileLocation.structure is ManMadeStructure && owner.gridTileLocation.structure == traitable2.gridTileLocation.structure;
				Character characterResponsible = null;
				if (num)
				{
					characterResponsible = base.responsibleCharacter;
				}
				traitable2.traitContainer.AddTrait(traitable2, "Burning", out var trait, characterResponsible, bypassElementalChance: true, -1, 0f, ELEMENTAL_TYPE.Fire);
				(trait as Burning)?.SetSourceOfBurning(sourceOfBurning, traitable2);
				traitable2.traitContainer.GetTraitOrStatus<Burning>("Burning")?.SetIsPlayerSource(isPlayerSource);
			}
		}
		RuinarchListPool<LocationGridTile>.Release(list);
	}

	public void SetDouser(Character character)
	{
		douser = character;
		if (douser == null)
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
		if (douser == character && jqi.jobType == JOB_TYPE.DOUSE_FIRE)
		{
			SetDouser(null);
		}
	}

	public void CharacterBurningProcess(Character character)
	{
		if (!character.isDead)
		{
			if (character.traitContainer.HasTrait("Pyrophobic"))
			{
				character.traitContainer.AddTrait(character, "Traumatized");
				character.traitContainer.AddTrait(character, "Unconscious");
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Trait", "Traits_Table", name + " pyrophobic_burn", LOG_TAG.Life_Changes);
				log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddLogToDatabase(releaseLogAfter: true);
			}
			else if (character.traitContainer.HasTrait("Pyromaniac"))
			{
				character.traitContainer.AddTrait(character, "Cheery");
			}
		}
	}

	private bool ShouldSpreadFire()
	{
		if (owner is IPointOfInterest)
		{
			return owner.gridTileLocation != null;
		}
		return false;
	}

	public bool CanTriggerDouseFire()
	{
		if (owner is Cinder cinder)
		{
			if (cinder.level == 0 || cinder.level == 3)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public void SetIsPlayerSource(bool p_state)
	{
		if (isPlayerSource == p_state)
		{
			return;
		}
		isPlayerSource = p_state;
		if (owner is Character p_owner)
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
	}

	public override void VillagerReactionToTileObjectTrait(TileObject owner, Character actor, ref string debugLog)
	{
		base.VillagerReactionToTileObjectTrait(owner, actor, ref debugLog);
		if (actor.partyComponent.isMemberThatJoinedQuest && actor.partyComponent.currentParty.currentQuest is RaidPartyQuest raidPartyQuest && raidPartyQuest.targetSettlement.IsAtTargetDestination(actor))
		{
			return;
		}
		Lazy traitOrStatus = actor.traitContainer.GetTraitOrStatus<Lazy>("Lazy");
		if (actor.combatComponent.isInActualCombat || actor.defaultCharacterTrait.hasSeenFire || (actor.jobQueue.jobsInQueue.Count > 0 && actor.jobQueue.jobsInQueue[0].priority > JOB_TYPE.DOUSE_FIRE.GetJobTypePriority()) || owner.gridTileLocation == null || actor.homeSettlement == null || (!owner.gridTileLocation.IsPartOfSettlement(actor.homeSettlement) && owner.gridTileLocation.structure.settlementLocation != actor.homeSettlement) || actor.traitContainer.HasTrait("Pyrophobic") || actor.traitContainer.HasTrait("Dousing") || actor.traitContainer.HasTrait("Pyromaniac") || actor.jobQueue.HasJob(JOB_TYPE.DOUSE_FIRE) || actor.behaviourComponent.HasBehaviour(typeof(CriticalBreakBehaviour)))
		{
			return;
		}
		actor.defaultCharacterTrait.SetHasSeenFire(state: true);
		if (traitOrStatus == null || !traitOrStatus.TryIgnoreUrgentTask(JOB_TYPE.DOUSE_FIRE))
		{
			if (CanTriggerDouseFire())
			{
				actor.homeSettlement.settlementJobTriggerComponent.TriggerDouseFire();
			}
			actor.homeSettlement.HasJob(JOB_TYPE.DOUSE_FIRE);
			JobQueueItem firstJobOfTypeThatCanBeAssignedTo = actor.homeSettlement.GetFirstJobOfTypeThatCanBeAssignedTo(JOB_TYPE.DOUSE_FIRE, actor);
			if (firstJobOfTypeThatCanBeAssignedTo != null && !IsResponsibleForTrait(actor))
			{
				actor.jobQueue.AddJobInQueue(firstJobOfTypeThatCanBeAssignedTo);
			}
			else if (actor.combatComponent.combatMode == COMBAT_MODE.Aggressive)
			{
				actor.combatComponent.Flight(owner, "Saw_Fire");
			}
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
		_ = douser;
		_burningSpreadChoices.Contains(p_character);
	}

	public override void DisconnectFromCharacter(IPointOfInterest p_owner, Character p_character)
	{
		base.DisconnectFromCharacter(p_owner, p_character);
		_burningSpreadChoices.Remove(p_character);
		if (douser == p_character)
		{
			SetDouser(null);
		}
	}
}
