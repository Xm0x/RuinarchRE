using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class NecromancerBehaviour : CharacterBehaviour
{
	public NecromancerBehaviour()
	{
		base.priority = 30;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (character.HasHome())
		{
			if ((bool)character.marker)
			{
				Character character2 = null;
				Character character3 = null;
				for (int i = 0; i < character.marker.inVisionCharacters.Count; i++)
				{
					Character character4 = character.marker.inVisionCharacters[i];
					if (character4.isDead)
					{
						if (!(character4 is Summon))
						{
							character2 = character4;
							break;
						}
						if (character3 == null)
						{
							character3 = character4;
						}
					}
				}
				if (character2 != null)
				{
					if (Random.Range(0, 100) < 80 && character.necromancerTrait.numOfSkeletonFollowers <= 25)
					{
						character.jobComponent.TriggerRaiseCorpse(JOB_TYPE.RAISE_CORPSE, character2, out producedJob);
						return true;
					}
				}
				else
				{
					Tombstone tombstone = null;
					for (int j = 0; j < character.marker.inVisionTileObjects.Count; j++)
					{
						if (character.marker.inVisionTileObjects[j] is Tombstone { character: var character5 } tombstone2 && !(character5 is Summon) && !character5.hasBeenRaisedFromDead)
						{
							tombstone = tombstone2;
							break;
						}
					}
					if (tombstone != null && Random.Range(0, 100) < 80 && character.necromancerTrait.numOfSkeletonFollowers <= 25)
					{
						character.jobComponent.TriggerRaiseCorpse(JOB_TYPE.RAISE_CORPSE, tombstone, out producedJob);
						return true;
					}
				}
			}
			TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick();
			if (currentTimeInWordsOfTick == TIME_IN_WORDS.EARLY_NIGHT || currentTimeInWordsOfTick == TIME_IN_WORDS.LATE_NIGHT || currentTimeInWordsOfTick == TIME_IN_WORDS.AFTER_MIDNIGHT)
			{
				if (character.necromancerTrait.GetNumOfSkeletonFollowersThatAreNotAttackingAndIsAlive() > 5 && Random.Range(0, 100) < 4)
				{
					NPCSettlement randomVillageSettlementForNecromancerAttack = LandmarkManager.Instance.GetRandomVillageSettlementForNecromancerAttack(character.currentRegion, character.faction, character);
					if (randomVillageSettlementForNecromancerAttack != null)
					{
						character.necromancerTrait.SetAttackVillageTarget(randomVillageSettlementForNecromancerAttack);
						character.interruptComponent.TriggerInterrupt(INTERRUPT.Order_Attack, character);
					}
				}
				else
				{
					if (character.currentStructure != null)
					{
						if (character.currentStructure.structureType == STRUCTURE_TYPE.ANCIENT_GRAVEYARD || character.currentStructure.structureType == STRUCTURE_TYPE.CEMETERY)
						{
							if (Random.Range(0, 100) < 30)
							{
								character.jobComponent.TriggerRoamAroundStructure(out producedJob);
								return true;
							}
						}
						else if (character.necromancerTrait.numOfSkeletonFollowers <= 25 && Random.Range(0, 100) < 50)
						{
							int num = Random.Range(0, 100);
							STRUCTURE_TYPE sTRUCTURE_TYPE = STRUCTURE_TYPE.CEMETERY;
							if (num < 90)
							{
								sTRUCTURE_TYPE = STRUCTURE_TYPE.ANCIENT_GRAVEYARD;
							}
							LocationStructure randomStructureOfTypeThatHasTombstone = character.currentRegion.GetRandomStructureOfTypeThatHasTombstone(sTRUCTURE_TYPE);
							if (randomStructureOfTypeThatHasTombstone != null)
							{
								LocationGridTile randomElement = CollectionUtilities.GetRandomElement(randomStructureOfTypeThatHasTombstone.passableTiles);
								character.jobComponent.CreateGoToJob(randomElement, out producedJob);
								return true;
							}
							sTRUCTURE_TYPE = ((sTRUCTURE_TYPE != STRUCTURE_TYPE.CEMETERY) ? STRUCTURE_TYPE.CEMETERY : STRUCTURE_TYPE.ANCIENT_GRAVEYARD);
							randomStructureOfTypeThatHasTombstone = character.currentRegion.GetRandomStructureOfTypeThatHasTombstone(sTRUCTURE_TYPE);
							if (randomStructureOfTypeThatHasTombstone != null)
							{
								LocationGridTile randomElement2 = CollectionUtilities.GetRandomElement(randomStructureOfTypeThatHasTombstone.passableTiles);
								character.jobComponent.CreateGoToJob(randomElement2, out producedJob);
								return true;
							}
						}
					}
					bool flag = false;
					if (character.necromancerTrait.energy >= 5)
					{
						if (character.necromancerTrait.numOfSkeletonFollowers <= 15 && GameUtilities.RollChance(80, ref log))
						{
							flag = character.jobComponent.TriggerSpawnSkeleton(out producedJob);
						}
					}
					else if (GameUtilities.RollChance(70, ref log))
					{
						flag = character.jobComponent.TriggerRegainEnergy(out producedJob);
					}
					if (!flag)
					{
						character.jobComponent.TriggerRoamAroundTile(out producedJob);
					}
				}
			}
			else if (!character.IsAtHome())
			{
				character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
			}
			else
			{
				for (int k = 0; k < character.faction.characters.Count; k++)
				{
					if (character.faction.characters[k].race == RACE.SKELETON && !character.faction.characters[k].isDead && character.behaviourComponent.attackVillageTarget != null)
					{
						character.necromancerTrait.SetAttackVillageTarget(null);
						character.interruptComponent.TriggerInterrupt(INTERRUPT.Recall_Attack, character);
						break;
					}
				}
				if (character.HasItem("Necronomicon") && Random.Range(0, 100) < 30 && character.jobComponent.TriggerReadNecronomicon(out producedJob))
				{
					return true;
				}
				if (Random.Range(0, 100) < 40 && character.jobComponent.TriggerMeditate(out producedJob))
				{
					return true;
				}
				character.jobComponent.TriggerRoamAroundTile(out producedJob);
			}
		}
		else
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Lair, character);
			if (character.necromancerTrait.lairStructure != null && character.homeStructure == character.necromancerTrait.lairStructure)
			{
				character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
			}
			else if (character.necromancerTrait.doNotSpawnLair)
			{
				character.jobComponent.TriggerRoamAroundTile(out producedJob);
			}
			else if (character.necromancerTrait.lairStructure == null || character.necromancerTrait.lairStructure.hasBeenDestroyed)
			{
				character.necromancerTrait.SetLairStructure(null);
				Area nearestAreaForNecromancerSpawnLairOrBuildingBanditCamp = character.gridTileLocation.GetNearestAreaForNecromancerSpawnLairOrBuildingBanditCamp(character);
				if (nearestAreaForNecromancerSpawnLairOrBuildingBanditCamp != null)
				{
					LocationGridTile centerGridTile = nearestAreaForNecromancerSpawnLairOrBuildingBanditCamp.gridTileComponent.centerGridTile;
					character.jobComponent.TriggerSpawnLair(centerGridTile, out producedJob);
				}
				else
				{
					LocationStructure structureForNecromancerLair = GetStructureForNecromancerLair(character);
					if (structureForNecromancerLair != null)
					{
						character.necromancerTrait.SetLairStructure(structureForNecromancerLair);
						character.MigrateHomeStructureTo(structureForNecromancerLair);
					}
					else
					{
						character.necromancerTrait.SetDoNotSpawnLair(p_state: true);
						character.jobComponent.TriggerRoamAroundTile(out producedJob);
					}
				}
			}
			else
			{
				if (character.homeStructure != character.necromancerTrait.lairStructure)
				{
					character.MigrateHomeStructureTo(character.necromancerTrait.lairStructure);
				}
				character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
			}
		}
		return true;
	}

	private LocationStructure GetStructureForNecromancerLair(Character p_necromancer)
	{
		Region currentRegion = p_necromancer.currentRegion;
		if (currentRegion != null)
		{
			for (int i = 0; i < currentRegion.allSpecialStructures.Count; i++)
			{
				LocationStructure locationStructure = currentRegion.allSpecialStructures[i];
				if (!locationStructure.hasBeenDestroyed && (!locationStructure.IsOccupied() || IsStructureOccupiedByUndead(locationStructure)) && !(locationStructure is Cave { hasConnectedMine: not false }))
				{
					return locationStructure;
				}
			}
		}
		return null;
	}

	private bool IsStructureOccupiedByUndead(LocationStructure p_structure)
	{
		for (int i = 0; i < p_structure.residents.Count; i++)
		{
			Character character = p_structure.residents[i];
			if (character.faction != null && character.faction.factionType.type == FACTION_TYPE.Undead)
			{
				return true;
			}
		}
		return false;
	}

	private Area GetNoStructurePlainAreaInRegion(Region region)
	{
		return region.GetRandomAreaThatIsUncorruptedFullyPlainNoStructureAndNotNextToOrPartOfVillage();
	}
}
