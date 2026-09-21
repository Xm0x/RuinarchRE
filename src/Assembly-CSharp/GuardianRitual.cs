using Inner_Maps;
using Locations.Settlements;

public class GuardianRitual : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.VERBAL;

	public GuardianRitual()
		: base(INTERACTION_TYPE.GUARDIAN_RITUAL)
	{
		base.actionIconString = GoapActionStateDB.Magic_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Ritual Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, target, otherData, job))
		{
			return target.gridTileLocation != null;
		}
		return false;
	}

	public void AfterRitualSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		BaseSettlement homeSettlement = actor.homeSettlement;
		if (homeSettlement == null)
		{
			return;
		}
		SUMMON_TYPE summonType = SUMMON_TYPE.Forest_Ent;
		if (actor.faction != null)
		{
			if (actor.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Divine_Worship))
			{
				summonType = SUMMON_TYPE.Warrior_Angel;
			}
			else if (actor.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Demon_Worship))
			{
				summonType = SUMMON_TYPE.Imp;
			}
		}
		bool flag = false;
		for (int i = 0; i < 4; i++)
		{
			LocationGridTile locationGridTile = homeSettlement.GetRandomPassableTileFromAreas();
			if (locationGridTile == null)
			{
				locationGridTile = homeSettlement.GetRandomTileFromAreas();
			}
			if (locationGridTile != null)
			{
				flag = true;
				Summon summon = CharacterManager.Instance.CreateNewSummon(summonType, actor.faction, homeSettlement, GridMap.Instance.mainRegion, null, "", bypassIdeologyChecking: true);
				summon.SetDestroyMarkerOnDeath(state: true);
				summon.traitContainer.RemoveTrait(summon, "Hibernating");
				summon.traitContainer.RemoveTrait(summon, "Indestructible");
				summon.traitContainer.AddTrait(summon, "Temporal");
				summon.traitContainer.AddTrait(summon, "Ephemeral");
				CharacterManager.Instance.PlaceSummonInitially(summon, locationGridTile);
				summon.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
				summon.behaviourComponent.ChangeDefaultBehaviourSet("Settlement Protector Behaviour");
				GameManager.Instance.CreateParticleEffectAt(summon, PARTICLE_EFFECT.Spawn_Effect);
			}
		}
		if (flag && actor.faction != null)
		{
			Messenger.Broadcast(FactionSignals.UPDATE_FACTION_COUNT, actor.faction);
		}
	}
}
