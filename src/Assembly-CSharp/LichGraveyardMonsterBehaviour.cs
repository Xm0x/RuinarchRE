using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UtilityScripts;

public class LichGraveyardMonsterBehaviour : CharacterBehaviour
{
	public LichGraveyardMonsterBehaviour()
	{
		base.priority = 10;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		LocationStructure homeStructure = character.homeStructure;
		if (homeStructure != null && homeStructure.structureType == STRUCTURE_TYPE.LICH_GRAVEYARD && (character.faction == null || !character.faction.isMajorNonPlayer))
		{
			if (character.IsAtHome())
			{
				if (TryAttackVillage(character, ref log))
				{
					producedJob = null;
					return true;
				}
				return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
			}
			return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
		}
		producedJob = null;
		return false;
	}

	public bool TryAttackVillage(Character character, ref string log)
	{
		Area areaLocation = character.areaLocation;
		if (areaLocation != null)
		{
			BaseSettlement randomSettlementTargetNearbyTo = GetRandomSettlementTargetNearbyTo(areaLocation);
			if (randomSettlementTargetNearbyTo != null)
			{
				character.behaviourComponent.SetAttackVillageTarget(randomSettlementTargetNearbyTo as NPCSettlement);
				if (!character.behaviourComponent.HasBehaviour(typeof(AttackVillageBehaviour)))
				{
					character.behaviourComponent.AddBehaviourComponent(typeof(AttackVillageBehaviour));
				}
				return true;
			}
		}
		return false;
	}

	private BaseSettlement GetRandomSettlementTargetNearbyTo(Area p_area)
	{
		BaseSettlement result = null;
		List<BaseSettlement> list = RuinarchListPool<BaseSettlement>.Claim();
		Region mainRegion = GridMap.Instance.mainRegion;
		for (int i = 0; i < mainRegion.settlementsInRegion.Count; i++)
		{
			BaseSettlement baseSettlement = mainRegion.settlementsInRegion[i];
			if (baseSettlement.locationType == LOCATION_TYPE.VILLAGE && baseSettlement is NPCSettlement nPCSettlement && nPCSettlement.HasAliveResident() && nPCSettlement.owner != null && nPCSettlement.owner.isMajorNonPlayer)
			{
				_ = nPCSettlement.owner.GetRelationshipWith(PlayerManager.Instance.player.playerFaction).relationshipStatus;
				if (nPCSettlement.IsNearbyToArea(p_area))
				{
					list.Add(nPCSettlement);
				}
			}
		}
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<BaseSettlement>.Release(list);
		return result;
	}
}
