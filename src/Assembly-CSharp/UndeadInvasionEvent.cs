using System.Collections.Generic;
using Locations.Settlements;
using UtilityScripts;

public class UndeadInvasionEvent : PrismEvent
{
	public bool hasAliveNecromancer
	{
		get
		{
			Character necromancerInTheWorld = CharacterManager.Instance.necromancerInTheWorld;
			if (necromancerInTheWorld != null && !necromancerInTheWorld.isDead && necromancerInTheWorld.hasMarker)
			{
				return true;
			}
			return false;
		}
	}

	public UndeadInvasionEvent(PrismEventData p_data)
		: base(p_data)
	{
		base.requirements = new PrismEventRequirement[3]
		{
			new PrismEventRequirement("Undead_Invasion_Event_Requirement_1", IsNecromancerRequirementSatisfied),
			new PrismEventRequirement("Undead_Invasion_Event_Requirement_2", IsSkeletonCountRequirementSatisfied),
			new PrismEventRequirement("Undead_Invasion_Event_Requirement_3", IsGhostCountRequirementSatisfied)
		};
	}

	private void Invade()
	{
		NPCSettlement attackVillageTarget = null;
		List<BaseSettlement> list = RuinarchListPool<BaseSettlement>.Claim();
		for (int i = 0; i < LandmarkManager.Instance.allNonPlayerSettlements.Count; i++)
		{
			NPCSettlement nPCSettlement = LandmarkManager.Instance.allNonPlayerSettlements[i];
			if (!nPCSettlement.hasBeenDestroyed && nPCSettlement.HasResidents() && nPCSettlement.locationType == LOCATION_TYPE.VILLAGE && nPCSettlement.owner != null && nPCSettlement.owner.isMajorNonPlayer && nPCSettlement.owner.factionType.type != FACTION_TYPE.Demons && nPCSettlement.owner.factionType.type != FACTION_TYPE.Demon_Cult)
			{
				list.Add(nPCSettlement);
			}
		}
		if (list.Count <= 0)
		{
			for (int j = 0; j < LandmarkManager.Instance.allNonPlayerSettlements.Count; j++)
			{
				NPCSettlement nPCSettlement2 = LandmarkManager.Instance.allNonPlayerSettlements[j];
				if (!nPCSettlement2.hasBeenDestroyed && nPCSettlement2.HasResidents() && nPCSettlement2.locationType == LOCATION_TYPE.VILLAGE && nPCSettlement2.owner != null && nPCSettlement2.owner.isMajorNonPlayer && nPCSettlement2.owner.factionType.type == FACTION_TYPE.Demon_Cult)
				{
					list.Add(nPCSettlement2);
				}
			}
		}
		if (list.Count > 0)
		{
			attackVillageTarget = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)] as NPCSettlement;
		}
		RuinarchListPool<BaseSettlement>.Release(list);
		Faction undeadFaction = FactionManager.Instance.undeadFaction;
		for (int k = 0; k < undeadFaction.characters.Count; k++)
		{
			Character character = undeadFaction.characters[k];
			if (!character.isDead && !character.isInLimbo && character.hasMarker && !character.behaviourComponent.HasBehaviour(typeof(AttackVillageBehaviour)))
			{
				character.behaviourComponent.SetAttackVillageTarget(attackVillageTarget);
				character.behaviourComponent.AddBehaviourComponent(typeof(AttackVillageBehaviour));
			}
		}
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Prism", "PrismEvents_Table", "Undead_Invasion_Effect", LOG_TAG.Major);
		log.AddLogToDatabase();
		PlayerManager.Instance?.player?.ShowNotificationFromPlayer(log, releaseLogAfter: true);
	}

	private bool IsNecromancerRequirementSatisfied()
	{
		return hasAliveNecromancer;
	}

	private bool IsSkeletonCountRequirementSatisfied()
	{
		int num = 0;
		Faction undeadFaction = FactionManager.Instance.undeadFaction;
		for (int i = 0; i < undeadFaction.characters.Count; i++)
		{
			Character character = undeadFaction.characters[i];
			if (!character.isDead && character is Summon { summonType: SUMMON_TYPE.Skeleton })
			{
				num++;
			}
		}
		return num >= 10;
	}

	private bool IsGhostCountRequirementSatisfied()
	{
		int num = 0;
		Faction undeadFaction = FactionManager.Instance.undeadFaction;
		for (int i = 0; i < undeadFaction.characters.Count; i++)
		{
			Character character = undeadFaction.characters[i];
			if (!character.isDead && character is Summon { summonType: SUMMON_TYPE.Ghost })
			{
				num++;
			}
		}
		return num >= 2;
	}

	protected override void TriggerEventBase()
	{
		base.TriggerEventBase();
		Invade();
	}
}
