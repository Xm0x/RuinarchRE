using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine;
using UtilityScripts;

namespace Traits;

public class Psychopath : Trait
{
	public SerialVictim victim1Requirement { get; private set; }

	public SerialVictim victim2Requirement { get; private set; }

	public Character character { get; private set; }

	public Character targetVictim { get; private set; }

	public Dictionary<int, OpinionData> opinionCopy { get; private set; }

	public override Type serializedData => typeof(SaveDataPsychopath);

	public Psychopath()
	{
		name = "Psychopath";
		description = "Has a specific subset of target victims that it wants to abduct and then kill.";
		type = TRAIT_TYPE.FLAW;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		canBeTriggered = true;
		opinionCopy = new Dictionary<int, OpinionData>();
		AddTraitOverrideFunctionIdentifier("See_Poi_Trait");
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataPsychopath saveDataPsychopath = saveDataTrait as SaveDataPsychopath;
		victim1Requirement = saveDataPsychopath.victim1Requirement;
		if (saveDataPsychopath.victim2Requirement == null || saveDataPsychopath.victim2Requirement.isEmpty)
		{
			victim2Requirement = null;
		}
		else
		{
			victim2Requirement = saveDataPsychopath.victim2Requirement;
		}
		opinionCopy = saveDataPsychopath.opinionCopy;
	}

	public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
		SaveDataPsychopath saveDataPsychopath = p_saveDataTrait as SaveDataPsychopath;
		if (!string.IsNullOrEmpty(saveDataPsychopath.targetVictimID))
		{
			targetVictim = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveDataPsychopath.targetVictimID);
		}
	}

	public override void OnAddTrait(ITraitable sourceCharacter)
	{
		base.OnAddTrait(sourceCharacter);
		if (sourceCharacter is Character)
		{
			character = sourceCharacter as Character;
			character.needsComponent.SetHappiness(50f, bypassPsychopathChecking: true);
			character.needsComponent.AdjustDoNotGetBored(1);
			CopyOpinionAndSetAllOpinionToZero();
			character.behaviourComponent.AddBehaviourComponent(typeof(PsychopathBehaviour));
		}
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character character)
		{
			this.character = character;
		}
	}

	public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy)
	{
		if (character != null)
		{
			character.needsComponent.AdjustDoNotGetBored(-1);
			BringBackOpinion();
			character.behaviourComponent.RemoveBehaviourComponent(typeof(PsychopathBehaviour));
			character = null;
			targetVictim = null;
		}
		base.OnRemoveTrait(sourceCharacter, removedBy);
	}

	public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob)
	{
		if (targetPOI is Character)
		{
			Character character = targetPOI as Character;
			if (!character.isDead && DoesCharacterFitAnyVictimRequirements(character))
			{
				CheckTargetVictimIfStillAvailable();
				if (targetVictim == null)
				{
					SetTargetVictim(character);
					Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "serial_killer_new_victim", LOG_TAG.Crimes);
					log.AddToFillers(this.character, this.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
					log.AddToFillers(targetVictim, targetVictim.name, LOG_IDENTIFIER.TARGET_CHARACTER);
					log.AddLogToDatabase();
					PlayerManager.Instance.player.ShowNotificationFrom(this.character.gridTileLocation, log, releaseLogAfter: true);
					return true;
				}
			}
		}
		return base.OnSeePOI(targetPOI, characterThatWillDoJob);
	}

	public override string TriggerFlaw(Character character, bool isTriggeredByPlayer = true)
	{
		CheckTargetVictimIfStillAvailable();
		if (targetVictim == null)
		{
			List<Character> list = RuinarchListPool<Character>.Claim();
			for (int i = 0; i < character.currentRegion.charactersAtLocation.Count; i++)
			{
				Character character2 = character.currentRegion.charactersAtLocation[i];
				if (character2 != character && character2.currentRegion == character.currentRegion && !IsCharacterNotApplicableAsVictim(character2) && !character2.isDead && DoesCharacterFitAnyVictimRequirements(character2))
				{
					list.Add(character2);
				}
			}
			if (list.Count > 0)
			{
				Character character3 = list[UnityEngine.Random.Range(0, list.Count)];
				SetTargetVictim(character3);
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "serial_killer_new_victim", LOG_TAG.Crimes);
				log.AddToFillers(this.character, this.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(targetVictim, targetVictim.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log.AddLogToDatabase();
				PlayerManager.Instance.player.ShowNotificationFrom(character.gridTileLocation, log, releaseLogAfter: true);
			}
			RuinarchListPool<Character>.Release(list);
			if (targetVictim == null)
			{
				return "no_target";
			}
		}
		if (targetVictim == null || !CreateHuntVictimJob(isTriggeredByPlayer))
		{
			return "fail";
		}
		return base.TriggerFlaw(character);
	}

	public override void DisconnectFromCharacter(IPointOfInterest p_owner, Character p_character)
	{
		base.DisconnectFromCharacter(p_owner, p_character);
		if (targetVictim == p_character)
		{
			SetTargetVictim(null);
		}
	}

	private void SetVictimRequirements(SerialVictim serialVictim1, SerialVictim serialVictim2)
	{
		victim1Requirement = serialVictim1;
		victim2Requirement = serialVictim2;
		string text = victim1Requirement.text;
		if (victim2Requirement != null)
		{
			text = text + " " + LocalizationManager.And + " " + victim2Requirement.text;
		}
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "became_serial_killer", LOG_TAG.Crimes);
		log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(null, text, LOG_IDENTIFIER.STRING_1);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
	}

	public void SetVictimRequirements(SERIAL_VICTIM_TYPE victimFirstType, string victimFirstDesc, SERIAL_VICTIM_TYPE victimSecondType, string victimSecondDesc, string conjunction, string victimFirstLocalized, string victimSecondLocalized)
	{
		if (conjunction == "And" || victimFirstType == SERIAL_VICTIM_TYPE.None || victimSecondType == SERIAL_VICTIM_TYPE.None)
		{
			SetVictimRequirements(new SerialVictim(victimFirstType, victimFirstDesc, victimSecondType, victimSecondDesc, victimFirstLocalized, victimSecondLocalized), null);
		}
		else
		{
			SetVictimRequirements(new SerialVictim(victimFirstType, victimFirstDesc, SERIAL_VICTIM_TYPE.None, string.Empty, victimFirstLocalized, string.Empty), new SerialVictim(victimSecondType, victimSecondDesc, SERIAL_VICTIM_TYPE.None, string.Empty, victimSecondLocalized, string.Empty));
		}
	}

	public void SetTargetVictim(Character victim)
	{
		if (targetVictim != null)
		{
			targetVictim.RemoveAdvertisedAction(INTERACTION_TYPE.RITUAL_KILLING);
		}
		victim?.AddAdvertisedAction(INTERACTION_TYPE.RITUAL_KILLING);
		targetVictim = victim;
	}

	private void OnCharacterDied(Character deadCharacter)
	{
		if (deadCharacter == targetVictim)
		{
			SetTargetVictim(null);
		}
	}

	private void OnCharacterMissing(Character missingCharacter)
	{
		if (missingCharacter == targetVictim)
		{
			SetTargetVictim(null);
		}
	}

	public void CheckTargetVictimIfStillAvailable()
	{
		if (targetVictim != null && IsCharacterNotApplicableAsVictim(targetVictim))
		{
			SetTargetVictim(null);
		}
	}

	private bool IsCharacterNotApplicableAsVictim(Character target)
	{
		if (target.isBeingSeized || target.isDead || !target.isNormalCharacter || !character.movementComponent.HasPathToEvenIfDiffRegion(target.gridTileLocation) || target.traitContainer.HasTrait("Travelling"))
		{
			return true;
		}
		AWARENESS_STATE awarenessState = character.relationshipContainer.GetAwarenessState(character, target);
		if (awarenessState == AWARENESS_STATE.Missing || awarenessState == AWARENESS_STATE.Presumed_Dead)
		{
			return true;
		}
		return false;
	}

	private bool CreateHuntVictimJob(bool isTriggeredByPlayer)
	{
		if (character.jobQueue.HasJob(JOB_TYPE.RITUAL_KILLING))
		{
			return false;
		}
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RITUAL_KILLING, INTERACTION_TYPE.RITUAL_KILLING, targetVictim, character);
		goapPlanJob.SetIsTriggeredByPlayer(isTriggeredByPlayer);
		if (character.homeStructure?.residents == null || character.homeStructure.residents.Count > 1)
		{
			LocationGridTile locationGridTile = null;
			BaseSettlement settlement = null;
			if (character.gridTileLocation.IsPartOfSettlement(out settlement))
			{
				locationGridTile = settlement.GetAPlainAdjacentArea()?.gridTileComponent.GetRandomPassableTile();
			}
			if (locationGridTile != null)
			{
				goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP, new object[2] { locationGridTile.structure, locationGridTile });
				goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[2] { locationGridTile.structure, locationGridTile });
				goapPlanJob.AddOtherData(INTERACTION_TYPE.RITUAL_KILLING, new object[1] { locationGridTile.area });
			}
			else if (character.homeStructure != null)
			{
				goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP, new object[1] { character.homeStructure });
				goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[1] { character.homeStructure });
				goapPlanJob.AddOtherData(INTERACTION_TYPE.RITUAL_KILLING, new object[1] { character.homeStructure });
			}
			else
			{
				Area nearestAreaWithinRegionThatIsNotMountainAndWaterAndHasNoSettlement = character.gridTileLocation.GetNearestAreaWithinRegionThatIsNotMountainAndWaterAndHasNoSettlement();
				if (nearestAreaWithinRegionThatIsNotMountainAndWaterAndHasNoSettlement != null)
				{
					LocationGridTile randomPassableTile = nearestAreaWithinRegionThatIsNotMountainAndWaterAndHasNoSettlement.gridTileComponent.GetRandomPassableTile();
					goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP, new object[2] { randomPassableTile.structure, randomPassableTile });
					goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[2] { randomPassableTile.structure, randomPassableTile });
					goapPlanJob.AddOtherData(INTERACTION_TYPE.RITUAL_KILLING, new object[1] { randomPassableTile });
				}
				else
				{
					LocationStructure structureOfTypeWithoutSettlement = character.currentRegion.GetStructureOfTypeWithoutSettlement(STRUCTURE_TYPE.WILDERNESS);
					goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP, new object[1] { structureOfTypeWithoutSettlement });
					goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[1] { structureOfTypeWithoutSettlement });
					goapPlanJob.AddOtherData(INTERACTION_TYPE.RITUAL_KILLING, new object[1] { structureOfTypeWithoutSettlement });
				}
			}
		}
		else
		{
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP, new object[1] { character.homeStructure });
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[1] { character.homeStructure });
			goapPlanJob.AddOtherData(INTERACTION_TYPE.RITUAL_KILLING, new object[1] { character.homeStructure });
		}
		goapPlanJob.isTriggeredFlaw = true;
		character.jobQueue.AddJobInQueue(goapPlanJob);
		return true;
	}

	public bool CreateHuntVictimJob(out JobQueueItem producedJob)
	{
		if (character.jobQueue.HasJob(JOB_TYPE.RITUAL_KILLING))
		{
			producedJob = null;
			return false;
		}
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RITUAL_KILLING, INTERACTION_TYPE.RITUAL_KILLING, targetVictim, character);
		if (character.homeStructure?.residents == null || character.homeStructure.residents.Count > 1)
		{
			LocationGridTile locationGridTile = null;
			BaseSettlement settlement = null;
			if (targetVictim.gridTileLocation.IsPartOfSettlement(out settlement))
			{
				locationGridTile = settlement.GetAPlainAdjacentArea()?.gridTileComponent.GetRandomPassableTile();
			}
			if (locationGridTile != null)
			{
				goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP, new object[2] { locationGridTile.structure, locationGridTile });
				goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[2] { locationGridTile.structure, locationGridTile });
				goapPlanJob.AddOtherData(INTERACTION_TYPE.RITUAL_KILLING, new object[1] { locationGridTile.area });
			}
			else if (character.homeStructure != null)
			{
				goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP, new object[1] { character.homeStructure });
				goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[1] { character.homeStructure });
				goapPlanJob.AddOtherData(INTERACTION_TYPE.RITUAL_KILLING, new object[1] { character.homeStructure });
			}
			else
			{
				Area nearestAreaWithinRegionThatIsNotMountainAndWaterAndHasNoSettlement = targetVictim.gridTileLocation.GetNearestAreaWithinRegionThatIsNotMountainAndWaterAndHasNoSettlement();
				if (nearestAreaWithinRegionThatIsNotMountainAndWaterAndHasNoSettlement != null)
				{
					LocationGridTile randomPassableTile = nearestAreaWithinRegionThatIsNotMountainAndWaterAndHasNoSettlement.gridTileComponent.GetRandomPassableTile();
					goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP, new object[2] { randomPassableTile.structure, randomPassableTile });
					goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[2] { randomPassableTile.structure, randomPassableTile });
					goapPlanJob.AddOtherData(INTERACTION_TYPE.RITUAL_KILLING, new object[1] { randomPassableTile });
				}
				else
				{
					LocationStructure structureOfTypeWithoutSettlement = targetVictim.currentRegion.GetStructureOfTypeWithoutSettlement(STRUCTURE_TYPE.WILDERNESS);
					goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP, new object[1] { structureOfTypeWithoutSettlement });
					goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[1] { structureOfTypeWithoutSettlement });
					goapPlanJob.AddOtherData(INTERACTION_TYPE.RITUAL_KILLING, new object[1] { structureOfTypeWithoutSettlement });
				}
			}
		}
		else
		{
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP, new object[1] { character.homeStructure });
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[1] { character.homeStructure });
			goapPlanJob.AddOtherData(INTERACTION_TYPE.RITUAL_KILLING, new object[1] { character.homeStructure });
		}
		producedJob = goapPlanJob;
		return true;
	}

	private bool DoesCharacterFitAnyVictimRequirements(Character target)
	{
		bool flag = false;
		if (victim1Requirement != null)
		{
			flag = victim1Requirement.DoesCharacterFitVictimRequirements(target);
		}
		if (flag)
		{
			return true;
		}
		if (victim2Requirement == null)
		{
			return flag;
		}
		if (victim2Requirement.DoesCharacterFitVictimRequirements(target))
		{
			return true;
		}
		return false;
	}

	private void CopyOpinionAndSetAllOpinionToZero()
	{
		foreach (KeyValuePair<int, IRelationshipData> relationship in character.relationshipContainer.relationships)
		{
			Character characterByID = CharacterManager.Instance.GetCharacterByID(relationship.Key);
			if (characterByID != null)
			{
				OpinionData opinions = relationship.Value.opinions;
				OpinionData opinionData = ObjectPoolManager.Instance.CreateNewOpinionData();
				opinionData.SetCompatibilityValue(opinions.compatibilityValue);
				List<string> list = opinions.allOpinions.Keys.ToList();
				for (int i = 0; i < list.Count; i++)
				{
					string text = list[i];
					opinionData.SetOpinion(text, opinions.allOpinions[text]);
					opinions.allOpinions[text] = 0;
				}
				opinionCopy.Add(characterByID.id, opinionData);
			}
		}
	}

	private void BringBackOpinion()
	{
		foreach (KeyValuePair<int, OpinionData> item in opinionCopy)
		{
			if (character.relationshipContainer.HasRelationshipWith(item.Key))
			{
				foreach (KeyValuePair<string, int> allOpinion in item.Value.allOpinions)
				{
					if (!(allOpinion.Key == "Base") && character.relationshipContainer.HasOpinion(item.Key, allOpinion.Key))
					{
						character.relationshipContainer.SetOpinion(character, DatabaseManager.Instance.characterDatabase.GetCharacterByID(item.Key), allOpinion.Key, allOpinion.Value);
					}
				}
			}
			ObjectPoolManager.Instance.ReturnOpinionDataToPool(item.Value);
		}
		opinionCopy.Clear();
	}

	public void AdjustOpinion(Character target, string opinionText, int opinionValue)
	{
		if (!(opinionText == "Base"))
		{
			if (!opinionCopy.ContainsKey(target.id))
			{
				opinionCopy.Add(target.id, ObjectPoolManager.Instance.CreateNewOpinionData());
			}
			opinionCopy[target.id].AdjustOpinion(opinionText, opinionValue);
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = character;
		_ = targetVictim;
	}
}
