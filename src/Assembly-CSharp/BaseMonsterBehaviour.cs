using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public abstract class BaseMonsterBehaviour : CharacterBehaviour
{
	private static STRUCTURE_TYPE[] allowedWorkStructures = new STRUCTURE_TYPE[8]
	{
		STRUCTURE_TYPE.TAVERN,
		STRUCTURE_TYPE.FISHERY,
		STRUCTURE_TYPE.FARM,
		STRUCTURE_TYPE.BUTCHERS_SHOP,
		STRUCTURE_TYPE.HUNTER_LODGE,
		STRUCTURE_TYPE.LUMBERYARD,
		STRUCTURE_TYPE.MINE,
		STRUCTURE_TYPE.WORKSHOP
	};

	public sealed override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character is Summon { isTamed: not false })
		{
			return TamedBehaviour(character, ref log, out producedJob);
		}
		if (character.minion != null && character.faction != null && character.faction.isMajorNonPlayer)
		{
			return TamedBehaviour(character, ref log, out producedJob);
		}
		if (character.faction != null && character.faction.isPlayerFaction)
		{
			return MonsterUnderlingBehaviour(character, ref log, out producedJob);
		}
		return WildBehaviour(character, ref log, out producedJob);
	}

	protected virtual bool TamedBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob)
	{
		RaceData raceData = RaceManager.Instance.GetRaceData(p_character.race);
		if (raceData.category == CHARACTER_CATEGORY.Humanoid || raceData.category == CHARACTER_CATEGORY.Demonic)
		{
			TryTakeWorkStructure(p_character, ref p_log);
		}
		if (TryTakeSettlementJob(p_character, ref p_log, out p_producedJob))
		{
			return true;
		}
		if (TryTakeWorkStructureJob(p_character, ref p_log, out p_producedJob))
		{
			return true;
		}
		if (TryTakePersonalPatrolJob(p_character, 15, ref p_log, out p_producedJob))
		{
			return true;
		}
		return TriggerRoamAroundTerritory(p_character, ref p_log, out p_producedJob);
	}

	protected abstract bool WildBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob);

	protected virtual bool MonsterUnderlingBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob)
	{
		return WildBehaviour(p_character, ref p_log, out p_producedJob);
	}

	private bool TryTakeWorkStructure(Character p_character, ref string p_log)
	{
		if (p_character.structureComponent.workPlaceStructure == null && p_character.homeSettlement != null)
		{
			List<STRUCTURE_TYPE> list = RuinarchListPool<STRUCTURE_TYPE>.Claim();
			for (int i = 0; i < allowedWorkStructures.Length; i++)
			{
				STRUCTURE_TYPE sTRUCTURE_TYPE = allowedWorkStructures[i];
				if (p_character.homeSettlement.HasStructureOfTypeThatCanAcceptWorkerAndIsNotReserved(sTRUCTURE_TYPE))
				{
					list.Add(sTRUCTURE_TYPE);
				}
			}
			STRUCTURE_TYPE sTRUCTURE_TYPE2 = STRUCTURE_TYPE.NONE;
			if (list.Count > 0)
			{
				sTRUCTURE_TYPE2 = CollectionUtilities.GetRandomElement(list);
			}
			RuinarchListPool<STRUCTURE_TYPE>.Release(list);
			if (sTRUCTURE_TYPE2 != STRUCTURE_TYPE.NONE && p_character.homeSettlement.GetRandomStructureOfTypeThatCanAcceptWorker(sTRUCTURE_TYPE2) is ManMadeStructure manMadeStructure && manMadeStructure.AddAssignedWorker(p_character))
			{
				p_character.interruptComponent.TriggerInterrupt(INTERRUPT.Claim_Work_Structure, p_character);
				return true;
			}
		}
		return false;
	}

	private bool TryTakeWorkStructureJob(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.structureComponent.workPlaceStructure != null && GameUtilities.RollChance(ChanceData.GetChance(CHANCE_TYPE.Do_Work_Chance), ref log))
		{
			character.structureComponent.workPlaceStructure.ProcessWorkerBehaviour(character, out producedJob);
			if (producedJob != null)
			{
				return true;
			}
		}
		producedJob = null;
		return false;
	}

	protected bool TryTakeSettlementJob(Character p_character, ref string p_log, out JobQueueItem p_producedJob)
	{
		if (p_character.behaviourComponent.PlanSettlementOrFactionWorkActions(out p_producedJob))
		{
			return true;
		}
		p_producedJob = null;
		return false;
	}

	protected bool TryTakePersonalPatrolJob(Character p_character, int chance, ref string p_log, out JobQueueItem p_producedJob)
	{
		if (GameUtilities.RollChance(chance, ref p_log))
		{
			if (p_character.jobComponent.TriggerPersonalPatrol(out p_producedJob))
			{
				return true;
			}
		}
		else if (p_character is Summon)
		{
			TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick();
			if ((currentTimeInWordsOfTick == TIME_IN_WORDS.LATE_NIGHT || currentTimeInWordsOfTick == TIME_IN_WORDS.AFTER_MIDNIGHT) && p_character.jobComponent.TriggerPersonalPatrol(out p_producedJob))
			{
				return true;
			}
		}
		p_producedJob = null;
		return false;
	}

	protected bool TriggerRoamAroundTerritory(Character p_character, ref string p_log, out JobQueueItem p_producedJob)
	{
		return p_character.jobComponent.TriggerRoamAroundTerritory(out p_producedJob);
	}

	protected bool DefaultWildMonsterBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob)
	{
		p_producedJob = null;
		if (p_character is Summon { gridTileLocation: not null } summon)
		{
			if ((summon.homeStructure == null || summon.homeStructure.hasBeenDestroyed) && !summon.HasTerritory() && (summon.faction == null || !summon.faction.isPlayerFaction))
			{
				summon.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, summon);
				if (summon.homeStructure == null && !summon.HasTerritory())
				{
					if (summon.combatComponent.combatMode == COMBAT_MODE.Passive)
					{
						return summon.jobComponent.TriggerRoamAroundTileAvoidVillageStructures(out p_producedJob);
					}
					return summon.jobComponent.TriggerRoamAroundTile(out p_producedJob);
				}
				return true;
			}
			if (summon.isAtHomeStructure || summon.IsInTerritory())
			{
				bool flag = false;
				int num = Mathf.RoundToInt((float)summon.maxHP * 0.5f);
				if (summon.currentHP < num && !summon.traitContainer.HasTrait("Poisoned") && !summon.traitContainer.HasTrait("Burning") && ShouldSleep(summon))
				{
					flag = summon.jobComponent.TriggerMonsterSleep(out p_producedJob);
				}
				else if (Random.Range(0, 100) < 35)
				{
					flag = (summon.isAtHomeStructure ? summon.jobComponent.TriggerRoamAroundTerritory(out p_producedJob) : ((summon.combatComponent.combatMode != COMBAT_MODE.Passive) ? summon.jobComponent.TriggerRoamAroundTile(out p_producedJob) : summon.jobComponent.TriggerRoamAroundTileAvoidVillageStructures(out p_producedJob)));
				}
				else if (ShouldSleep(summon))
				{
					TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick();
					if (currentTimeInWordsOfTick == TIME_IN_WORDS.LATE_NIGHT || currentTimeInWordsOfTick == TIME_IN_WORDS.AFTER_MIDNIGHT)
					{
						if (Random.Range(0, 100) < 40)
						{
							flag = summon.jobComponent.TriggerMonsterSleep(out p_producedJob);
						}
					}
					else if (Random.Range(0, 100) < 5)
					{
						flag = summon.jobComponent.TriggerMonsterSleep(out p_producedJob);
					}
				}
				if (!flag)
				{
					if (summon is Animal)
					{
						if (summon.combatComponent.combatMode == COMBAT_MODE.Passive)
						{
							return summon.jobComponent.TriggerRoamAroundTileAvoidVillageStructures(out p_producedJob);
						}
						return summon.jobComponent.TriggerRoamAroundTile(out p_producedJob);
					}
					summon.jobComponent.TriggerStand(out p_producedJob);
				}
				return true;
			}
			int num2 = Mathf.RoundToInt((float)summon.maxHP * 0.5f);
			if (summon.currentHP < num2)
			{
				if (summon.homeStructure != null || summon.HasTerritory())
				{
					return summon.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out p_producedJob);
				}
			}
			else
			{
				if (Random.Range(0, 100) < 50)
				{
					if (summon.combatComponent.combatMode == COMBAT_MODE.Passive)
					{
						summon.jobComponent.TriggerRoamAroundTileAvoidVillageStructures(out p_producedJob);
					}
					else
					{
						summon.jobComponent.TriggerRoamAroundTile(out p_producedJob);
					}
					return true;
				}
				if (summon.homeStructure != null || summon.HasTerritory())
				{
					return summon.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out p_producedJob);
				}
			}
		}
		return false;
	}

	private bool ShouldSleep(Character p_character)
	{
		if (!p_character.IsUndead())
		{
			return p_character.race != RACE.GOLEM;
		}
		return false;
	}

	protected void PopulateAreasSurroundingHome(List<Area> areas, Character character)
	{
		Area area = null;
		if (character.homeSettlement != null)
		{
			character.homeSettlement.PopulateSurroundingAreas(areas);
		}
		else if (character.homeStructure != null)
		{
			area = ((!(character.homeStructure is Cave cave)) ? character.homeStructure.occupiedArea : CollectionUtilities.GetRandomElement(cave.occupiedAreas.Keys));
		}
		else if (character.HasTerritory())
		{
			area = character.territory;
		}
		if (area == null)
		{
			return;
		}
		for (int i = 0; i < area.neighbourComponent.neighbours.Count; i++)
		{
			Area area2 = area.neighbourComponent.neighbours[i];
			if (area2.region == area.region)
			{
				areas.Add(area2);
			}
		}
	}

	protected void PopulateFoodPilesAtHome(List<TileObject> foodPiles, Character character)
	{
		if (character.homeSettlement != null)
		{
			character.homeSettlement.PopulateTileObjectsOfType<FoodPile>(foodPiles);
		}
		else if (character.homeStructure != null)
		{
			character.homeStructure.PopulateTileObjectsOfType<FoodPile>(foodPiles);
		}
		else if (character.HasTerritory())
		{
			character.territory.tileObjectComponent.PopulateTileObjectsInArea<FoodPile>(foodPiles);
		}
	}
}
