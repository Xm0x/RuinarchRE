using System;
using System.Collections.Generic;
using UtilityScripts;

[Serializable]
public class SaveDataBehaviourComponent : SaveData<BehaviourComponent>
{
	public List<string> currentBehaviourComponents;

	public string attackVillageTarget;

	public string attackAreaTarget;

	public string attackDemonicStructureTarget;

	public bool isAttackingDemonicStructure;

	public bool hasLayedAnEgg;

	public bool subterraneanJustExitedCombat;

	public string defaultBehaviourSetName;

	public COMBAT_MODE combatModeBeforeAttackVillageBehaviour;

	public STRUCTURE_TYPE wildernessMonsterSpawnerStructureType;

	public bool hasBuiltWildernessMonsterSpawnerStructure;

	public string monsterSpawnerID;

	public COMBAT_MODE combatModeBeforePatrolling;

	public string dousingFireForSettlement;

	public string cleansingTilesForSettlement;

	public string settlementTargetForPurifyGround;

	public int numberOfPurifiedGrounds;

	public string currentAbductTarget;

	public int currentDeMoodCooldown;

	public List<string> deMoodVillageTarget;

	public List<string> invadeVillageTarget;

	public int followerCount;

	public int currentDisableCooldown;

	public TileLocationSave nest;

	public bool hasEatenInTheMorning;

	public bool hasEatenInTheNight;

	public int currentArsonCooldown;

	public List<string> arsonVillageTarget;

	public string abominationTarget;

	public bool isCurrentlySnatching;

	public string pestSettlementTarget;

	public bool pestHasFailedEat;

	public COMBAT_MODE combatModeBeforeAttackingDemonicStructure;

	public string targetSocializeStructure;

	public GameDate socializingEndTime;

	public string targetVisitVillage;

	public string targetVisitVillageStructure;

	public GameDate visitVillageEndTime;

	public VISIT_VILLAGE_INTENT visitVillageIntent;

	public bool shouldTryToBuildNewVillage;

	public Point chosenVillageSpotForNewVillage;

	public int tendedCropsForToday;

	public bool canFlirtOnActivePartyQuest;

	public string criticalBreakDestroyStructureTarget;

	public string criticalBreakKillTarget;

	public int criticalBreakFiresCreated;

	public override void Save(BehaviourComponent data)
	{
		currentBehaviourComponents = RuinarchListPool<string>.Claim();
		for (int i = 0; i < data.currentBehaviourComponents.Count; i++)
		{
			currentBehaviourComponents.Add(data.currentBehaviourComponents[i].ToString());
		}
		isAttackingDemonicStructure = data.isAttackingDemonicStructure;
		hasLayedAnEgg = data.hasLayedAnEgg;
		subterraneanJustExitedCombat = data.subterraneanJustExitedCombat;
		defaultBehaviourSetName = data.defaultBehaviourSetName;
		currentDeMoodCooldown = data.currentDeMoodCooldown;
		followerCount = data.followerCount;
		currentDisableCooldown = data.currentDisableCooldown;
		currentArsonCooldown = data.currentArsonCooldown;
		hasEatenInTheMorning = data.hasEatenInTheMorning;
		hasEatenInTheNight = data.hasEatenInTheNight;
		isCurrentlySnatching = data.isCurrentlySnatching;
		combatModeBeforeAttackingDemonicStructure = data.combatModeBeforeAttackingDemonicStructure;
		pestHasFailedEat = data.pestHasFailedEat;
		combatModeBeforeAttackVillageBehaviour = data.combatModeBeforeAttackVillageBehaviour;
		wildernessMonsterSpawnerStructureType = data.wildernessMonsterSpawnerStructureType;
		hasBuiltWildernessMonsterSpawnerStructure = data.hasBuiltWildernessMonsterSpawnerStructure;
		monsterSpawnerID = data.monsterSpawnerID;
		combatModeBeforePatrolling = data.combatModeBeforePatrolling;
		canFlirtOnActivePartyQuest = data.canFlirtOnActivePartyQuest;
		if (data.attackVillageTarget != null)
		{
			attackVillageTarget = data.attackVillageTarget.persistentID;
		}
		if (data.attackAreaTarget != null)
		{
			attackAreaTarget = data.attackAreaTarget.persistentID;
		}
		if (data.attackDemonicStructureTarget != null)
		{
			attackDemonicStructureTarget = data.attackDemonicStructureTarget.persistentID;
		}
		if (data.dousingFireForSettlement != null)
		{
			dousingFireForSettlement = data.dousingFireForSettlement.persistentID;
		}
		if (data.cleansingTilesForSettlement != null)
		{
			cleansingTilesForSettlement = data.cleansingTilesForSettlement.persistentID;
		}
		settlementTargetForPurifyGround = data.settlementTargetForPurifyGround;
		numberOfPurifiedGrounds = data.numberOfPurifiedGrounds;
		if (data.currentAbductTarget != null)
		{
			currentAbductTarget = data.currentAbductTarget.persistentID;
		}
		if (data.nest != null)
		{
			nest = new TileLocationSave(data.nest);
		}
		if (data.abominationTarget != null)
		{
			abominationTarget = data.abominationTarget.persistentID;
		}
		if (data.deMoodVillageTarget != null)
		{
			deMoodVillageTarget = RuinarchListPool<string>.Claim();
			for (int j = 0; j < data.deMoodVillageTarget.Count; j++)
			{
				deMoodVillageTarget.Add(data.deMoodVillageTarget[j].persistentID);
			}
		}
		if (data.invadeVillageTarget != null)
		{
			invadeVillageTarget = RuinarchListPool<string>.Claim();
			for (int k = 0; k < data.invadeVillageTarget.Count; k++)
			{
				invadeVillageTarget.Add(data.invadeVillageTarget[k].persistentID);
			}
		}
		if (data.arsonVillageTarget != null)
		{
			arsonVillageTarget = RuinarchListPool<string>.Claim();
			for (int l = 0; l < data.arsonVillageTarget.Count; l++)
			{
				arsonVillageTarget.Add(data.arsonVillageTarget[l].persistentID);
			}
		}
		if (data.pestSettlementTarget != null)
		{
			pestSettlementTarget = data.pestSettlementTarget.persistentID;
		}
		if (data.targetSocializeStructure != null)
		{
			targetSocializeStructure = data.targetSocializeStructure.persistentID;
		}
		if (data.socializingEndTime.hasValue)
		{
			socializingEndTime = data.socializingEndTime;
		}
		if (data.targetVisitVillage != null)
		{
			targetVisitVillage = data.targetVisitVillage.persistentID;
		}
		if (data.targetVisitVillageStructure != null)
		{
			targetVisitVillageStructure = data.targetVisitVillageStructure.persistentID;
		}
		if (data.visitVillageEndTime.hasValue)
		{
			visitVillageEndTime = data.visitVillageEndTime;
		}
		visitVillageIntent = data.visitVillageIntent;
		shouldTryToBuildNewVillage = data.shouldTryToBuildNewVillage;
		if (data.chosenVillageSpotForNewVillage != null)
		{
			chosenVillageSpotForNewVillage = new Point(data.chosenVillageSpotForNewVillage.coreSpot.areaData.xCoordinate, data.chosenVillageSpotForNewVillage.coreSpot.areaData.yCoordinate);
		}
		tendedCropsForToday = data.tendedCropsForToday;
		if (data.criticalBreakDestroyStructureTarget != null)
		{
			criticalBreakDestroyStructureTarget = data.criticalBreakDestroyStructureTarget.persistentID;
		}
		if (data.criticalBreakKillTarget != null)
		{
			criticalBreakKillTarget = data.criticalBreakKillTarget.persistentID;
		}
		criticalBreakFiresCreated = data.criticalBreakFiresCreated;
	}

	public override BehaviourComponent Load()
	{
		return new BehaviourComponent(this);
	}

	public override void CleanUp()
	{
		if (currentBehaviourComponents != null)
		{
			RuinarchListPool<string>.Release(currentBehaviourComponents);
			currentBehaviourComponents = null;
		}
		if (deMoodVillageTarget != null)
		{
			RuinarchListPool<string>.Release(deMoodVillageTarget);
			deMoodVillageTarget = null;
		}
		if (invadeVillageTarget != null)
		{
			RuinarchListPool<string>.Release(invadeVillageTarget);
			invadeVillageTarget = null;
		}
		if (arsonVillageTarget != null)
		{
			RuinarchListPool<string>.Release(arsonVillageTarget);
			arsonVillageTarget = null;
		}
	}
}
