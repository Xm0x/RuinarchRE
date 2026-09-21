using System;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Ruinarch;
using Traits;
using UnityEngine;
using UtilityScripts;

public class POITestingUI : MonoBehaviour
{
	public RectTransform rt;

	[SerializeField]
	private GameObject _secondColumnGO;

	[SerializeField]
	private RectTransform _secondColumnParent;

	[SerializeField]
	private GameObject _poiTestingItemUI;

	public IPointOfInterest poi { get; private set; }

	public LocationGridTile gridTile { get; private set; }

	public Character activeCharacter { get; private set; }

	public LocationStructure activeStructure { get; private set; }

	public void ShowUI(IPointOfInterest poi, Character activeCharacter)
	{
		this.activeCharacter = activeCharacter;
		this.poi = poi;
		UIManager.Instance.HideSmallInfo();
		RectTransformUtility.ScreenPointToLocalPointInRectangle(UIManager.Instance.canvas.transform as RectTransform, InputManager.Instance.mousePosition, null, out var localPoint);
		rt.transform.localPosition = localPoint;
		base.gameObject.SetActive(value: true);
		_secondColumnGO.SetActive(value: false);
		Messenger.AddListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputAction);
	}

	public void ShowUI(IPointOfInterest poi, LocationStructure activeStructure)
	{
		if (activeStructure != null)
		{
			this.activeStructure = activeStructure;
			this.poi = poi;
			UIManager.Instance.HideSmallInfo();
			RectTransformUtility.ScreenPointToLocalPointInRectangle(UIManager.Instance.canvas.transform as RectTransform, InputManager.Instance.mousePosition, null, out var localPoint);
			rt.transform.localPosition = localPoint;
			base.gameObject.SetActive(value: true);
			_secondColumnGO.SetActive(value: false);
			Messenger.AddListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputAction);
		}
	}

	public void HideUI()
	{
		base.gameObject.SetActive(value: false);
		_secondColumnGO.SetActive(value: false);
		poi = null;
		gridTile = null;
		Messenger.RemoveListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputAction);
	}

	private void OnReceivePlayerInputAction(SHORTCUT_ACTION p_action)
	{
		if (p_action == SHORTCUT_ACTION.Left_Click && !UIManager.Instance.IsMouseOnUI())
		{
			HideUI();
		}
		else if (p_action == SHORTCUT_ACTION.Cancel)
		{
			HideUI();
		}
	}

	public void KnockoutThisCharacter()
	{
		if (poi is Character targetCharacter)
		{
			CreateKnockoutJob(activeCharacter, targetCharacter);
		}
		HideUI();
	}

	public void FightThisCharacter()
	{
		if (poi is Character character)
		{
			if (activeStructure != null)
			{
				IDamageable nearestDamageableThatContributeToHP = activeStructure.GetNearestDamageableThatContributeToHP(character.gridTileLocation);
				if (nearestDamageableThatContributeToHP != null && nearestDamageableThatContributeToHP is IPointOfInterest target)
				{
					character.combatComponent.Fight(target, "Anger");
					return;
				}
			}
			activeCharacter.combatComponent.Fight(character, "Anger");
		}
		else
		{
			activeCharacter.combatComponent.Fight(poi, "Anger");
		}
		HideUI();
	}

	public bool CreateKnockoutJob(Character character, Character targetCharacter)
	{
		GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BRAWL, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Unconscious", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), targetCharacter, character);
		character.jobQueue.AddJobInQueue(job);
		return true;
	}

	public void ChatWithThisCharacter()
	{
		if (poi is Character targetPOI)
		{
			activeCharacter.interruptComponent.TriggerInterrupt(INTERRUPT.Flirt, targetPOI);
		}
		else
		{
			Debug.LogError(poi.name + " is not a character!");
		}
		HideUI();
	}

	public void InviteToMakeLove()
	{
		if (poi is Character targetPOI)
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, INTERACTION_TYPE.MAKE_LOVE, targetPOI, activeCharacter);
			activeCharacter.jobQueue.AddJobInQueue(job);
		}
		else
		{
			Debug.LogError(poi.name + " is not a character!");
		}
		HideUI();
	}

	public void StealFromThisCharacter()
	{
		if (poi is Character p_targetCharacter)
		{
			activeCharacter.traitContainer.BecomeObsessWith(activeCharacter, p_targetCharacter);
		}
		else
		{
			Debug.LogError(poi.name + " is not a character!");
		}
		HideUI();
	}

	public void DrinkBlood()
	{
		if (poi is Character)
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FULLNESS_RECOVERY_URGENT, INTERACTION_TYPE.DRINK_BLOOD, poi, activeCharacter);
			activeCharacter.jobQueue.AddJobInQueue(job);
		}
		else
		{
			Debug.LogError(poi.name + " is not a character!");
		}
		HideUI();
	}

	public void Feed()
	{
		if (poi is Character)
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FULLNESS_RECOVERY_URGENT, INTERACTION_TYPE.FEED, poi, activeCharacter);
			activeCharacter.jobQueue.AddJobInQueue(job);
		}
		else
		{
			Debug.LogError(poi.name + " is not a character!");
		}
		HideUI();
	}

	public void SpreadRumor()
	{
		if (poi is Character character)
		{
			Character randomAliveEnemyCharacter = activeCharacter.relationshipContainer.GetRandomAliveEnemyCharacter();
			if (randomAliveEnemyCharacter != null)
			{
				ActualGoapNode randomKnownNegativeInfo = activeCharacter.rumorComponent.GetRandomKnownNegativeInfo(character, randomAliveEnemyCharacter);
				if (randomKnownNegativeInfo != null && activeCharacter.jobComponent.CreateSpreadNegativeInfoJob(JOB_TYPE.SHARE_NEGATIVE_INFO, character, randomKnownNegativeInfo))
				{
					HideUI();
					return;
				}
				Rumor rumor = activeCharacter.rumorComponent.GenerateNewRandomRumor(character, randomAliveEnemyCharacter);
				if (rumor != null)
				{
					activeCharacter.jobComponent.CreateSpreadRumorJob(character, rumor);
				}
			}
		}
		else
		{
			Debug.LogError(poi.name + " is not a character!");
		}
		HideUI();
	}

	public void StrangleSelf()
	{
		GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.COMMIT_SUICIDE, INTERACTION_TYPE.STRANGLE, activeCharacter, activeCharacter);
		activeCharacter.jobQueue.AddJobInQueue(job);
		HideUI();
	}

	public void Recruit()
	{
		if (poi is Character targetCharacter)
		{
			activeCharacter.jobComponent.TriggerRecruitJob(targetCharacter, out var producedJob);
			activeCharacter.jobQueue.AddJobInQueue(producedJob);
		}
		HideUI();
	}

	public void GoToCharacter()
	{
		if (poi is Character)
		{
			activeCharacter.jobComponent.CreateGoToJob(poi);
		}
		else
		{
			Debug.LogError(poi.name + " is not a character!");
		}
		HideUI();
	}

	public void Sing()
	{
		if (poi is Character)
		{
			if (activeCharacter.jobComponent.TriggerSingJob(out var producedJob))
			{
				activeCharacter.jobQueue.AddJobInQueue(producedJob);
			}
		}
		else
		{
			Debug.LogError(poi.name + " is not a character!");
		}
		HideUI();
	}

	public void Pray()
	{
		if (poi is Character)
		{
			activeCharacter.jobComponent.TriggerPray();
		}
		else
		{
			Debug.LogError(poi.name + " is not a character!");
		}
		HideUI();
	}

	public void OnClickPlaceBlueprint()
	{
		Utilities.DestroyChildrenObjectPool(_secondColumnParent);
		if (poi is Character character)
		{
			if (character.homeSettlement != null && character.faction != null)
			{
				STRUCTURE_TYPE[] enumValues = CollectionUtilities.GetEnumValues<STRUCTURE_TYPE>();
				foreach (STRUCTURE_TYPE structureType in enumValues)
				{
					if (structureType.IsVillageStructure())
					{
						CreatePOITestingItemUI(structureType.ToStringEnum(), delegate
						{
							OnSelectStructureBlueprintToPlace(structureType);
						});
					}
				}
				_secondColumnGO.SetActive(value: true);
			}
			else
			{
				Debug.LogError(character.name + " does not have a home settlement or faction!");
			}
		}
		else
		{
			Debug.LogError(poi.name + " is not a character!");
		}
	}

	private void OnSelectStructureBlueprintToPlace(STRUCTURE_TYPE p_structureType)
	{
		if (poi is Character character)
		{
			string log = string.Empty;
			if (character.jobComponent.TryCreatePlaceBlueprintJob(character.faction.factionType.type, p_structureType, character, character.homeSettlement, out var producedJob, ref log))
			{
				character.jobQueue.AddJobInQueue(producedJob);
			}
		}
		HideUI();
	}

	public void ClaimHallowedGround()
	{
		if (poi is Character)
		{
			if (activeCharacter.traitContainer.IsReligiousCultist())
			{
				if (activeCharacter.currentRegion.HasStructure(STRUCTURE_TYPE.HALLOWED_GROUND))
				{
					HallowedGround tileObjectOfType = (activeCharacter.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.HALLOWED_GROUND) as Inner_Maps.Location_Structures.HallowedGround).GetTileObjectOfType<HallowedGround>();
					if (activeCharacter.jobComponent.CreateClaimHallowedGroundJob(tileObjectOfType, out var producedJob))
					{
						activeCharacter.jobQueue.AddJobInQueue(producedJob);
					}
				}
			}
			else
			{
				Debug.LogError(activeCharacter.name + " is not a religious cultist!");
			}
		}
		else
		{
			Debug.LogError(poi.name + " is not a character!");
		}
		HideUI();
	}

	public void CreateAndJoinBanditFaction()
	{
		if (poi is Character p_character)
		{
			FactionManager.Instance.JoinOrCreateBanditFaction(p_character);
		}
		else
		{
			Debug.LogError(poi.name + " is not a character!");
		}
		HideUI();
	}

	public void Arouse()
	{
		if (poi is Character p_character)
		{
			activeCharacter.traitContainer.AddTrait(activeCharacter, "Aroused");
			activeCharacter.traitContainer.GetTraitOrStatus<Aroused>("Aroused").AddArousedTarget(p_character);
		}
		HideUI();
	}

	public void Grudge()
	{
		if (poi is Character p_target)
		{
			activeCharacter.relationshipContainer.SetHasGrudgeAgainst(activeCharacter, p_target, p_state: true);
		}
		HideUI();
	}

	public void CreateNewFactionAndLiveHere()
	{
		if (poi.gridTileLocation != null && poi.gridTileLocation.structure.structureType.IsSpecialStructure() && poi.gridTileLocation.structure.settlementLocation is NPCSettlement newHomeSettlement)
		{
			activeCharacter.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, activeCharacter);
			activeCharacter.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Faction, activeCharacter);
			activeCharacter.MigrateHomeTo(newHomeSettlement, poi.gridTileLocation.structure);
		}
	}

	public void CreateNewVillage()
	{
		if (!WorldSettings.Instance.worldSettingsData.villageSettings.disableNewVillages && !activeCharacter.currentRegion.IsRegionVillageCapacityReached() && activeCharacter.faction != null)
		{
			VillageSpot firstUnoccupiedVillageSpotThatCanAccomodateFaction = activeCharacter.currentRegion.GetFirstUnoccupiedVillageSpotThatCanAccomodateFaction(activeCharacter.faction.factionType.type);
			if (firstUnoccupiedVillageSpotThatCanAccomodateFaction != null)
			{
				Area coreSpot = firstUnoccupiedVillageSpotThatCanAccomodateFaction.coreSpot;
				StructureSetting structureSetting = new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, activeCharacter.faction.factionType.mainResource);
				GameObject randomElement = CollectionUtilities.GetRandomElement(InnerMapManager.Instance.GetStructurePrefabsForStructure(activeCharacter.faction.factionType.type, structureSetting));
				if (LandmarkManager.Instance.HasEnoughSpaceForStructure(randomElement.name, coreSpot.gridTileComponent.centerGridTile) && activeCharacter.jobComponent.TriggerFindNewVillage(coreSpot.gridTileComponent.centerGridTile, out var producedJob, randomElement.name))
				{
					activeCharacter.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Village, activeCharacter);
					activeCharacter.jobQueue.AddJobInQueue(producedJob);
				}
			}
		}
		HideUI();
	}

	public void Cry()
	{
		Character character = activeCharacter;
		if (poi is Character character2)
		{
			character = character2;
		}
		if (character.isDead)
		{
			activeCharacter.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, character, "", null, "Saw_Dead");
		}
		else
		{
			activeCharacter.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, character, "", null, "Cry_Sadness");
		}
		HideUI();
	}

	public void PoisonTable()
	{
		if (poi is Table)
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.POISON_FOOD, INTERACTION_TYPE.POISON, poi, activeCharacter);
			activeCharacter.jobQueue.AddJobInQueue(job);
		}
		else if (poi is Character targetCharacter)
		{
			activeCharacter.jobComponent.CreatePoisonFoodJob(targetCharacter);
		}
		else
		{
			Debug.LogError(poi.name + " is not a table or a character!");
		}
		HideUI();
	}

	public void EatAtTable()
	{
		GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FULLNESS_RECOVERY_URGENT, INTERACTION_TYPE.EAT, poi, activeCharacter);
		activeCharacter.jobQueue.AddJobInQueue(job);
		HideUI();
	}

	public void Sleep()
	{
		if (poi is Bed)
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL, INTERACTION_TYPE.SLEEP, poi, activeCharacter);
			activeCharacter.jobQueue.AddJobInQueue(job);
		}
		else
		{
			Debug.LogError(poi.name + " is not a bed!");
		}
		HideUI();
	}

	public void BoobyTrap()
	{
		if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT)
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PLACE_TRAP, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Booby Trapped", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), poi, activeCharacter);
			activeCharacter.jobQueue.AddJobInQueue(job);
		}
		else
		{
			Debug.LogError(poi.name + " is not a tile object!");
		}
		HideUI();
	}

	public void KleptomaniacStealAnything()
	{
		if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT)
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.KLEPTOMANIAC_STEAL, INTERACTION_TYPE.STEAL_ANYTHING, poi, activeCharacter);
			activeCharacter.jobQueue.AddJobInQueue(job);
		}
		else
		{
			Debug.LogError(poi.name + " is not a tile object!");
		}
		HideUI();
	}

	public void HarvestPlant()
	{
		if (poi is Crops)
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PRODUCE_FOOD, INTERACTION_TYPE.HARVEST_PLANT, poi, activeCharacter);
			activeCharacter.jobQueue.AddJobInQueue(job);
		}
		else
		{
			Debug.LogWarning(poi.name + " is not a crop!");
		}
		HideUI();
	}

	public void Butcher()
	{
		if (poi is Tombstone tombstone)
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PRODUCE_FOOD, INTERACTION_TYPE.BUTCHER, tombstone.character, activeCharacter);
			activeCharacter.jobQueue.AddJobInQueue(job);
		}
		else if (poi is Character targetPOI)
		{
			GoapPlanJob job2 = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PRODUCE_FOOD, INTERACTION_TYPE.BUTCHER, targetPOI, activeCharacter);
			activeCharacter.jobQueue.AddJobInQueue(job2);
		}
		else
		{
			Debug.LogError(poi.name + " is not a table or a character!");
		}
		HideUI();
	}

	public void RestrainPersonal()
	{
		poi.traitContainer.RestrainAndImprison(poi, activeCharacter, null, activeCharacter);
		HideUI();
	}

	public void RestrainFaction()
	{
		poi.traitContainer.RestrainAndImprison(poi, activeCharacter, activeCharacter.faction);
		HideUI();
	}

	public void MakeDirty()
	{
		poi.traitContainer.AddTrait(poi, "Dirty");
		HideUI();
	}

	public void CleanUpDirt()
	{
		if (poi is TileObject tileObject && tileObject.traitContainer.HasTrait("Dirty", "Wet", "Burnt"))
		{
			activeCharacter.jobComponent.TryCreateCleanItemJob(tileObject, out var p_producedJob);
			activeCharacter.jobQueue.AddJobInQueue(p_producedJob);
		}
		else
		{
			Debug.LogWarning(poi.name + " is not a tile object that is dirty or wet!");
		}
		HideUI();
	}

	public void DevastationRitual()
	{
		JobQueueItem p_producedJob = null;
		activeCharacter.jobComponent.CreateDevastationRitualJob(poi as MagicCircle, poi.gridTileLocation.structure, ref p_producedJob);
		if (p_producedJob != null)
		{
			activeCharacter.jobQueue.AddJobInQueue(p_producedJob);
		}
		HideUI();
	}

	public void SetCharacterOwner()
	{
		if (poi is TileObject tileObject)
		{
			tileObject.SetCharacterOwner(activeCharacter);
		}
		else
		{
			Debug.LogError(poi.name + " is not a tile object!");
		}
		HideUI();
	}

	public void GoHere()
	{
		if (poi is Character)
		{
			GoToCharacter();
			return;
		}
		activeCharacter.jobComponent.CreateGoToSpecificTileJob(poi.gridTileLocation);
		HideUI();
	}

	public void AddRandomArtifact()
	{
		ARTIFACT_TYPE[] enumValues = CollectionUtilities.GetEnumValues<ARTIFACT_TYPE>();
		bool flag = false;
		while (!flag)
		{
			ARTIFACT_TYPE aRTIFACT_TYPE = enumValues[UnityEngine.Random.Range(0, enumValues.Length)];
			if (aRTIFACT_TYPE != ARTIFACT_TYPE.None)
			{
				flag = true;
				LocationGridTile firstNearestTileFromThisWithNoObject = poi.gridTileLocation.GetFirstNearestTileFromThisWithNoObject();
				Artifact artifact = InnerMapManager.Instance.CreateNewArtifact(aRTIFACT_TYPE);
				firstNearestTileFromThisWithNoObject.structure.AddPOI(artifact, firstNearestTileFromThisWithNoObject);
			}
		}
		HideUI();
	}

	private void CreatePOITestingItemUI(string p_title, Action p_onClick)
	{
		ObjectPoolManager.Instance.InstantiateObjectFromPool(_poiTestingItemUI.name, Vector3.zero, Quaternion.identity, _secondColumnParent).GetComponent<POITestingItemUI>().Initialize(p_title, p_onClick);
	}
}
