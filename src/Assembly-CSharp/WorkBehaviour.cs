using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using UtilityScripts;

public class WorkBehaviour : CharacterBehaviour
{
	public static readonly List<JOB_TYPE> Allowed_Job_Types_Outside_Work = new List<JOB_TYPE>
	{
		JOB_TYPE.PLACE_BLUEPRINT,
		JOB_TYPE.CHANGE_CLASS,
		JOB_TYPE.BUILD_BLUEPRINT
	};

	public WorkBehaviour()
	{
		base.priority = 16;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.faction != null && character.faction.isMajorNonPlayer && !character.isFactionLeader && !character.isSettlementRuler && !character.crimeComponent.hasReportedCrime && character.crimeComponent.witnessedCrimes.Count > 0)
		{
			character.crimeComponent.SetHasReportedCrime(state: true);
			for (int i = 0; i < character.crimeComponent.witnessedCrimes.Count; i++)
			{
				CrimeData crimeData = character.crimeComponent.witnessedCrimes[i];
				if (!crimeData.isRemoved && !character.crimeComponent.IsReported(crimeData) && CrimeManager.Instance.GetPersonalCrimeSeverity(character, crimeData.criminal, crimeData.target, crimeData.crimeType) != CRIME_SEVERITY.None && character.jobComponent.TryCreateReportCrimeJob(crimeData.criminal, crimeData.target, crimeData, crimeData.crime, out producedJob))
				{
					return true;
				}
			}
		}
		if (character.dailyScheduleComponent.schedule.GetScheduleType(GameManager.Instance.currentTick) == DAILY_SCHEDULE.Work)
		{
			NPCSettlement homeSettlement = character.homeSettlement;
			if (homeSettlement != null && homeSettlement.locationType == LOCATION_TYPE.VILLAGE && !character.isVagrantOrFactionless)
			{
				if (character.structureComponent.workPlaceStructure == null)
				{
					STRUCTURE_TYPE workStructureType = CharacterManager.Instance.GetCharacterClass(character.characterClass.className).workStructureType;
					if (workStructureType != STRUCTURE_TYPE.NONE && homeSettlement.GetRandomStructureOfTypeThatCanAcceptWorker(workStructureType) is ManMadeStructure manMadeStructure && manMadeStructure.AddAssignedWorker(character))
					{
						character.interruptComponent.TriggerInterrupt(INTERRUPT.Claim_Work_Structure, character);
					}
				}
				if (character.currentSettlement != null && character.currentSettlement.HasHospiceClaimedByNonEnemyOrSelfAndNotBanned(character, out var foundStructure))
				{
					Hospice hospice = foundStructure as Hospice;
					if (character.traitContainer.HasTrait("Injured", "Burnt", "Poisoned", "Plagued") && !character.traitContainer.HasTrait("Plague Reservoir") && ChanceData.RollChance(CHANCE_TYPE.Plauged_Injured_Visit_Hospice))
					{
						BedClinic firstBedToRecuperate = hospice.GetFirstBedToRecuperate();
						if (firstBedToRecuperate != null && character.jobComponent.TryRecuperate(firstBedToRecuperate, out producedJob))
						{
							return true;
						}
					}
					if (ChanceData.RollChance(CHANCE_TYPE.Vampire_Lycan_Visit_Hospice, ref log) && character.currentStructure != foundStructure && hospice.HasWorkerWithLevel5HealingMagic())
					{
						_ = character.traitContainer.GetTraitOrStatus<Vampire>("Vampire")?.dislikedBeingVampire;
						if (character.lycanData != null)
						{
							_ = character.lycanData.dislikesBeingLycan;
						}
					}
					if (character.currentStructure == foundStructure && character.trapStructure.IsTrappedAndTrapStructureIs(foundStructure))
					{
						producedJob = null;
						return false;
					}
				}
				if (character.moodComponent.moodState != MOOD_STATE.Bad && character.moodComponent.moodState != MOOD_STATE.Critical && character.behaviourComponent.PlanSettlementOrFactionWorkActions(out producedJob))
				{
					return true;
				}
				if (character.moodComponent.moodState != MOOD_STATE.Critical && character.structureComponent.workPlaceStructure != null)
				{
					if (character.traitContainer.HasTrait("Lazy"))
					{
						Lazy traitOrStatus = character.traitContainer.GetTraitOrStatus<Lazy>("Lazy");
						if (GameUtilities.RollChance(traitOrStatus.GetTriggerChance(character), ref log))
						{
							traitOrStatus.TriggerLazy();
							producedJob = null;
							return false;
						}
					}
					if (GameUtilities.RollChance(ChanceData.GetChance(CHANCE_TYPE.Do_Work_Chance), ref log))
					{
						character.structureComponent.workPlaceStructure.ProcessWorkerBehaviour(character, out producedJob);
						if (producedJob != null)
						{
							return true;
						}
					}
				}
			}
			if (character.moodComponent.moodState != MOOD_STATE.Normal)
			{
				if (character.traitContainer.HasTrait("Despairing") && GameUtilities.RollChance(8, ref log))
				{
					if (GameUtilities.RollChance(80, ref log))
					{
						character.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, character, "", null, "Cry_Despair");
						producedJob = null;
						return true;
					}
					if (character.jobComponent.TriggerSuicideJob(out producedJob, "Suicide_Reason_Despair"))
					{
						return true;
					}
				}
				bool flag = false;
				if (TraitManager.Instance.CanStillTriggerFlaws(character) && ChanceData.RollChance(CHANCE_TYPE.Trigger_Flaw_During_Work, ref log))
				{
					List<Trait> list = RuinarchListPool<Trait>.Claim();
					for (int j = 0; j < character.traitContainer.traits.Count; j++)
					{
						Trait trait = character.traitContainer.traits[j];
						if (trait.type == TRAIT_TYPE.FLAW && trait.canBeTriggered)
						{
							list.Add(trait);
						}
					}
					if (list.Count > 0 && list[Random.Range(0, list.Count)].TriggerFlaw(character, isTriggeredByPlayer: false) == "flaw_effect")
					{
						flag = true;
					}
					RuinarchListPool<Trait>.Release(list);
				}
				if (flag)
				{
					producedJob = null;
					return true;
				}
				if (!character.traitContainer.HasTrait("Diplomatic") && character.characterClass.className != "Hero")
				{
					int num = Random.Range(0, 100);
					int num2 = 4;
					if (character.traitContainer.HasTrait("Treacherous"))
					{
						num2 += 4;
					}
					if (num < num2)
					{
						Character randomAliveEnemyCharacter = character.relationshipContainer.GetRandomAliveEnemyCharacter();
						if (randomAliveEnemyCharacter != null)
						{
							if (randomAliveEnemyCharacter.homeSettlement != null)
							{
								if (randomAliveEnemyCharacter.homeSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Vampire_Hunt))
								{
									Character randomSpreadRumorOrNegativeInfoTarget = character.rumorComponent.GetRandomSpreadRumorOrNegativeInfoTarget(randomAliveEnemyCharacter);
									if (randomSpreadRumorOrNegativeInfoTarget != null)
									{
										Rumor rumor = character.rumorComponent.CreateNewRumor(randomAliveEnemyCharacter, randomAliveEnemyCharacter, INTERACTION_TYPE.IS_VAMPIRE);
										if (rumor != null && character.jobComponent.CreateSpreadRumorJob(randomSpreadRumorOrNegativeInfoTarget, rumor, out producedJob))
										{
											return true;
										}
									}
								}
								if (randomAliveEnemyCharacter.homeSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Werewolf_Hunt))
								{
									Character randomSpreadRumorOrNegativeInfoTarget2 = character.rumorComponent.GetRandomSpreadRumorOrNegativeInfoTarget(randomAliveEnemyCharacter);
									if (randomSpreadRumorOrNegativeInfoTarget2 != null)
									{
										Rumor rumor2 = character.rumorComponent.CreateNewRumor(randomAliveEnemyCharacter, randomAliveEnemyCharacter, INTERACTION_TYPE.IS_WEREWOLF);
										if (rumor2 != null && character.jobComponent.CreateSpreadRumorJob(randomSpreadRumorOrNegativeInfoTarget2, rumor2, out producedJob))
										{
											return true;
										}
									}
								}
							}
							if (randomAliveEnemyCharacter.HasOwnedItemThatIsOnGroundInSameRegion())
							{
								if (GameUtilities.RollChance(50) && character.jobComponent.CreatePlaceTrapJob(randomAliveEnemyCharacter, out producedJob))
								{
									return true;
								}
								if (character.jobComponent.CreatePoisonFoodJob(randomAliveEnemyCharacter, out producedJob))
								{
									return true;
								}
							}
						}
					}
				}
			}
		}
		else if (character.moodComponent.moodState != MOOD_STATE.Bad && character.moodComponent.moodState != MOOD_STATE.Critical && character.behaviourComponent.TryTakeSettlementOrFactionWorkActionsOutsideOfWorkSchedule(out producedJob))
		{
			return true;
		}
		producedJob = null;
		return false;
	}
}
