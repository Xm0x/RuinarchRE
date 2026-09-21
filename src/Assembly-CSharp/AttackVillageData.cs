using System.Collections.Generic;
using Locations.Settlements;
using UtilityScripts;

public class AttackVillageData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.ATTACK_VILLAGE;

	public override string name => "Attack Village";

	public override string description => "This Ability will induce spawned monsters to attack a nearby Village owned by a faction that is not aligned with the player.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.PLAYER_ACTION;

	public AttackVillageData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE_OBJECT };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is MonsterSpawner monsterSpawner)
		{
			Area area = monsterSpawner.gridTileLocation?.area;
			if (area != null && monsterSpawner.HasValidSpawnedMonster() && GetRandomTargetSettlementNearbyToArea(area) is NPCSettlement attackVillageTarget)
			{
				for (int i = 0; i < monsterSpawner.spawnedCharacterIDs.Count; i++)
				{
					Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(monsterSpawner.spawnedCharacterIDs[i]);
					if (characterByPersistentID.isDead)
					{
						continue;
					}
					if (characterByPersistentID.traitContainer.HasTrait("Resting"))
					{
						characterByPersistentID.currentJob?.CancelJob();
					}
					characterByPersistentID.jobQueue.CancelAllJobs(JOB_TYPE.ROAM_AROUND_TILE, JOB_TYPE.ROAM_AROUND_CORRUPTION, JOB_TYPE.ROAM_AROUND_PORTAL, JOB_TYPE.ROAM_AROUND_STRUCTURE, JOB_TYPE.ROAM_AROUND_TERRITORY, JOB_TYPE.IDLE_STAND, JOB_TYPE.IDLE_RETURN_HOME, JOB_TYPE.IDLE_RETURN_HOME_HIGHER, JOB_TYPE.STAND, JOB_TYPE.IDLE);
					if (characterByPersistentID.limiterComponent.canPerform && characterByPersistentID.limiterComponent.canMove)
					{
						characterByPersistentID.behaviourComponent.SetAttackVillageTarget(attackVillageTarget);
						if (!characterByPersistentID.behaviourComponent.HasBehaviour(typeof(AttackVillageBehaviour)))
						{
							characterByPersistentID.behaviourComponent.AddBehaviourComponent(typeof(AttackVillageBehaviour));
						}
					}
				}
			}
		}
		base.ActivateAbility(targetPOI);
	}

	public override bool CanPerformAbilityTowards(TileObject tileObject)
	{
		Area area = tileObject.gridTileLocation?.area;
		if (area == null)
		{
			return false;
		}
		if (!(tileObject is MonsterSpawner monsterSpawner) || !monsterSpawner.HasValidSpawnedMonsterThatIsNotAttackingVillage())
		{
			return false;
		}
		if (!HasRandomTargetSettlementNearbyToArea(area))
		{
			return false;
		}
		return base.CanPerformAbilityTowards(tileObject);
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(TileObject targetTileObject)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetTileObject);
		Area area = targetTileObject.gridTileLocation?.area;
		if (area == null)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Attack_Village_No_Location") + "|";
		}
		if (!HasRandomTargetSettlementNearbyToArea(area))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Attack_Village_No_Nearby") + "|";
		}
		MonsterSpawner monsterSpawner = targetTileObject as MonsterSpawner;
		if (monsterSpawner == null || !monsterSpawner.HasValidSpawnedMonsterThatIsNotAttackingVillage())
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Attack_Village_No_Monster", monsterSpawner.gridTileLocation.structure) + "|";
		}
		return text;
	}

	private bool HasRandomTargetSettlementNearbyToArea(Area p_area)
	{
		Region mainRegion = GridMap.Instance.mainRegion;
		for (int i = 0; i < mainRegion.settlementsInRegion.Count; i++)
		{
			BaseSettlement p_settlement = mainRegion.settlementsInRegion[i];
			if (IsSettlementValidToBeTarget(p_settlement, p_area))
			{
				return true;
			}
		}
		return false;
	}

	private BaseSettlement GetRandomTargetSettlementNearbyToArea(Area p_area)
	{
		BaseSettlement result = null;
		List<BaseSettlement> list = RuinarchListPool<BaseSettlement>.Claim();
		Region mainRegion = GridMap.Instance.mainRegion;
		for (int i = 0; i < mainRegion.settlementsInRegion.Count; i++)
		{
			BaseSettlement baseSettlement = mainRegion.settlementsInRegion[i];
			if (IsSettlementValidToBeTarget(baseSettlement, p_area))
			{
				list.Add(baseSettlement);
			}
		}
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<BaseSettlement>.Release(list);
		return result;
	}

	private bool IsSettlementValidToBeTarget(BaseSettlement p_settlement, Area p_relativeArea)
	{
		if (p_settlement.locationType == LOCATION_TYPE.VILLAGE && p_settlement is NPCSettlement nPCSettlement && nPCSettlement.HasAliveResident() && nPCSettlement.owner != null && nPCSettlement.owner.isMajorNonPlayer && nPCSettlement.owner.GetRelationshipWith(PlayerManager.Instance.player.playerFaction).relationshipStatus != FACTION_RELATIONSHIP_STATUS.Friendly && nPCSettlement.IsNearbyToArea(p_relativeArea))
		{
			return true;
		}
		return false;
	}
}
