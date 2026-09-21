using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements.Settlement_Events;
using Object_Pools;
using UnityEngine;
using UtilityScripts;

public class JudgeCharacter : GoapAction
{
	public override bool shouldShowNotifRegardlessOfWatcher => true;

	public JudgeCharacter()
		: base(INTERACTION_TYPE.JUDGE_CHARACTER)
	{
		base.actionIconString = GoapActionStateDB.Judge_Icon;
		base.doesNotStopTargetCharacter = true;
		base.logTags = new LOG_TAG[3]
		{
			LOG_TAG.Work,
			LOG_TAG.Life_Changes,
			LOG_TAG.Crimes
		};
		base.showNotification = true;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Judge Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public void PreJudgeSuccess(ActualGoapNode goapNode)
	{
		WeightedDictionary<string> weightedDictionary = new WeightedDictionary<string>();
		Character character = goapNode.poiTarget as Character;
		Character actor = goapNode.actor;
		FactionRelationship relationshipWith = actor.faction.GetRelationshipWith(character.faction);
		string opinionLabel = actor.relationshipContainer.GetOpinionLabel(character);
		CrimeData firstCrimeWantedBy = character.crimeComponent.GetFirstCrimeWantedBy(actor.faction, CRIME_STATUS.Unpunished);
		if (firstCrimeWantedBy != null)
		{
			string empty = string.Empty;
			if (actor.relationshipContainer.HasGrudgeAgainst(character))
			{
				empty = "Execute";
			}
			else
			{
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				if ((relationshipWith != null && relationshipWith.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Hostile) || firstCrimeWantedBy == null)
				{
					num2 = 5;
					num3 = 100;
					num4 = 10;
				}
				else if (firstCrimeWantedBy.crimeSeverity == CRIME_SEVERITY.Misdemeanor)
				{
					num = 50;
					num2 = 100;
				}
				else if (firstCrimeWantedBy.crimeSeverity == CRIME_SEVERITY.Serious)
				{
					num = 5;
					num2 = 20;
					num3 = 50;
					num4 = 50;
				}
				else if (firstCrimeWantedBy.crimeSeverity == CRIME_SEVERITY.Heinous)
				{
					num2 = 5;
					num3 = 100;
					num4 = 50;
				}
				if (character.faction == actor.faction)
				{
					num = Mathf.RoundToInt((float)num * 1.5f);
					num2 = Mathf.RoundToInt((float)num2 * 1.5f);
				}
				switch (opinionLabel)
				{
				case "Close Friend":
					num *= 3;
					num2 *= 2;
					num3 = 0;
					num4 = Mathf.RoundToInt((float)num4 * 0.5f);
					break;
				case "Friend":
					num *= 2;
					num2 *= 2;
					num3 = Mathf.RoundToInt((float)num3 * 0.1f);
					num4 = Mathf.RoundToInt((float)num4 * 0.5f);
					break;
				case "Enemy":
					num = Mathf.RoundToInt((float)num * 0.1f);
					num2 = Mathf.RoundToInt((float)num2 * 0.5f);
					num3 *= 2;
					num4 = Mathf.RoundToInt((float)num4 * 1.5f);
					break;
				case "Rival":
					num = 0;
					num2 = Mathf.RoundToInt((float)num2 * 0.5f);
					num3 *= 3;
					num4 = Mathf.RoundToInt((float)num4 * 1.5f);
					break;
				}
				if (actor.traitContainer.HasTrait("Ruthless"))
				{
					num = 0;
					num2 = Mathf.RoundToInt((float)num2 * 0.5f);
					num3 *= 2;
					num4 = num4;
				}
				if (firstCrimeWantedBy.crimeType == CRIME_TYPE.Plagued && actor.homeSettlement != null)
				{
					PlaguedEvent activeEvent = actor.homeSettlement.eventManager.GetActiveEvent<PlaguedEvent>();
					if (activeEvent != null)
					{
						switch (activeEvent.rulerDecision)
						{
						case PLAGUE_EVENT_RESPONSE.Slay:
							num = 0;
							num2 = 0;
							num3 = num3;
							num4 = Mathf.RoundToInt((float)num4 * 0.2f);
							break;
						case PLAGUE_EVENT_RESPONSE.Exile:
							num = 0;
							num2 = 0;
							num3 = Mathf.RoundToInt((float)num3 * 0.2f);
							num4 = num4;
							break;
						}
					}
				}
				if (firstCrimeWantedBy.crimeType == CRIME_TYPE.Vampire)
				{
					if (actor.traitContainer.HasTrait("Hemophobic"))
					{
						num = 0;
						num2 = 0;
						num3 *= 2;
						num4 = num4;
					}
					else if (actor.traitContainer.HasTrait("Hemophiliac"))
					{
						num *= 3;
						num2 = Mathf.RoundToInt((float)num2 * 0.5f);
						num3 = 0;
						num4 = Mathf.RoundToInt((float)num2 * 0.5f);
					}
				}
				else if (firstCrimeWantedBy.crimeType == CRIME_TYPE.Werewolf)
				{
					if (actor.traitContainer.HasTrait("Lycanphobic"))
					{
						num = 0;
						num2 = 0;
						num3 *= 2;
						num4 = num4;
					}
					else if (actor.traitContainer.HasTrait("Lycanphiliac"))
					{
						num *= 3;
						num2 = Mathf.RoundToInt((float)num2 * 0.5f);
						num3 = 0;
						num4 = Mathf.RoundToInt((float)num2 * 0.5f);
					}
				}
				if (character.faction != actor.faction)
				{
					if (relationshipWith != null && relationshipWith.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Neutral)
					{
						num = Mathf.RoundToInt((float)num * 0.5f);
						num2 = Mathf.RoundToInt((float)num2 * 0.5f);
						num3 = Mathf.RoundToInt((float)num3 * 2f);
						num4 = 0;
					}
					else if (relationshipWith != null && relationshipWith.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Hostile)
					{
						num = Mathf.RoundToInt((float)num * 0.2f);
						num2 = Mathf.RoundToInt((float)num2 * 0.5f);
						num3 *= 3;
						num4 = 0;
					}
				}
				weightedDictionary.AddElement("Absolve", num);
				weightedDictionary.AddElement("Whip", num2);
				weightedDictionary.AddElement("Execute", num3);
				weightedDictionary.AddElement("Exile", num4);
				empty = weightedDictionary.PickRandomElementGivenWeights();
			}
			switch (empty)
			{
			case "Absolve":
				TargetAbsolved(goapNode);
				break;
			case "Whip":
				TargetWhip(goapNode, firstCrimeWantedBy);
				break;
			case "Execute":
				if (GameUtilities.RollChance(50) && !goapNode.actor.traitContainer.HasTrait("Pyrophobic"))
				{
					TargetBurnAtStake(goapNode, firstCrimeWantedBy);
				}
				else
				{
					TargetExecuted(goapNode);
				}
				break;
			case "Exile":
				TargetExiled(goapNode, firstCrimeWantedBy);
				break;
			}
		}
		else
		{
			TargetRelease(goapNode);
		}
	}

	private void TargetRelease(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		Character character = goapNode.target as Character;
		character.crimeComponent.SetDecisionAndJudgeToAllUnpunishedCrimesWantedBy(character.faction, CRIME_STATUS.Absolved, actor);
		character.crimeComponent.RemoveAllCrimesWantedBy(goapNode.actor.faction);
		character.traitContainer.RemoveRestrainAndImprison(character, actor);
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.goapName + " release", LOG_TAG.Work, null);
		log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		goapNode.LogAction(log);
		LogPool.Release(log);
	}

	private void TargetExecuted(ActualGoapNode goapNode)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JUDGE_PRISONER, INTERACTION_TYPE.EXECUTE, goapNode.target, goapNode.actor);
		goapPlanJob.SetCannotBePushedBack(state: true);
		goapPlanJob.SetDoNotRecalculate(state: true);
		goapNode.actor.jobQueue.AddJobInQueue(goapPlanJob);
	}

	private void TargetAbsolved(ActualGoapNode goapNode)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JUDGE_PRISONER, INTERACTION_TYPE.ABSOLVE, goapNode.target, goapNode.actor);
		goapPlanJob.SetCannotBePushedBack(state: true);
		goapPlanJob.SetDoNotRecalculate(state: true);
		goapNode.actor.jobQueue.AddJobInQueue(goapPlanJob);
	}

	private void TargetExiled(ActualGoapNode goapNode, CrimeData crimeData)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JUDGE_PRISONER, INTERACTION_TYPE.EXILE, goapNode.target, goapNode.actor);
		goapPlanJob.AddOtherData(INTERACTION_TYPE.EXILE, new object[1] { crimeData });
		goapPlanJob.SetCannotBePushedBack(state: true);
		goapPlanJob.SetDoNotRecalculate(state: true);
		goapNode.actor.jobQueue.AddJobInQueue(goapPlanJob);
	}

	private void TargetWhip(ActualGoapNode goapNode, CrimeData crimeData)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JUDGE_PRISONER, INTERACTION_TYPE.WHIP, goapNode.target, goapNode.actor);
		goapPlanJob.AddOtherData(INTERACTION_TYPE.WHIP, new object[1] { crimeData });
		goapPlanJob.SetCannotBePushedBack(state: true);
		goapPlanJob.SetDoNotRecalculate(state: true);
		goapNode.actor.jobQueue.AddJobInQueue(goapPlanJob);
	}

	private void TargetBurnAtStake(ActualGoapNode goapNode, CrimeData crimeData)
	{
		Character actor = goapNode.actor;
		LocationStructure locationStructure = null;
		LocationGridTile locationGridTile = null;
		if (actor.currentRegion != null)
		{
			locationStructure = actor.currentRegion.wilderness;
		}
		if (locationStructure != null)
		{
			if (actor.homeSettlement != null)
			{
				locationGridTile = CollectionUtilities.GetRandomElement(locationStructure.unoccupiedTiles.Where((LocationGridTile tile) => tile.IsNextToSettlement(actor.homeSettlement) && actor.movementComponent.HasPathToEvenIfDiffRegion(tile)));
			}
			else if (goapNode.poiTarget.gridTileLocation != null)
			{
				locationGridTile = goapNode.poiTarget.gridTileLocation.GetNearestUnoccupiedTileFromThisWithStructure(locationStructure.structureType);
			}
			else if (actor.gridTileLocation != null)
			{
				locationGridTile = actor.gridTileLocation.GetNearestUnoccupiedTileFromThisWithStructure(locationStructure.structureType);
			}
		}
		if (locationStructure != null && locationGridTile != null)
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JUDGE_PRISONER, INTERACTION_TYPE.BURN_AT_STAKE, goapNode.target, goapNode.actor);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetDoNotRecalculate(state: true);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP, new object[2] { locationStructure, locationGridTile });
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[2] { locationStructure, locationGridTile });
			goapPlanJob.AddOtherData(INTERACTION_TYPE.BURN_AT_STAKE, new object[1] { crimeData });
			goapNode.actor.jobQueue.AddJobInQueue(goapPlanJob);
		}
		else
		{
			TargetExecuted(goapNode);
		}
	}
}
