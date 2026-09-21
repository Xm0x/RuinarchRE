using System.Collections.Generic;
using Locations.Settlements;
using UtilityScripts;

public class BroodmotherOrderAttack : GoapAction
{
	public BroodmotherOrderAttack()
		: base(INTERACTION_TYPE.BROODMOTHER_ORDER_ATTACK)
	{
		base.actionIconString = GoapActionStateDB.Hostile_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Order Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		BaseSettlement baseSettlement = node.otherData[0].obj as BaseSettlement;
		log.AddToFillers(baseSettlement, baseSettlement.name, LOG_IDENTIFIER.LANDMARK_1);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (actor.gridTileLocation != null)
			{
				return actor == poiTarget;
			}
			return false;
		}
		return false;
	}

	public void AfterOrderSuccess(ActualGoapNode goapNode)
	{
		BaseSettlement baseSettlement = null;
		if (goapNode.otherData != null && goapNode.otherData.Length != 0 && goapNode.otherData[0] is SettlementOtherData settlementOtherData)
		{
			baseSettlement = settlementOtherData.settlement;
		}
		if (baseSettlement == null)
		{
			return;
		}
		Character actor = goapNode.actor;
		List<Character> list = RuinarchListPool<Character>.Claim();
		if (actor.homeSettlement != null)
		{
			for (int i = 0; i < actor.homeSettlement.residents.Count; i++)
			{
				Character character = actor.homeSettlement.residents[i];
				if (!character.isDead && character.limiterComponent.canMove && character.limiterComponent.canPerform && !character.isBeingSeized && character is GiantSpider)
				{
					list.Add(character);
				}
			}
		}
		else if (actor.homeStructure != null)
		{
			for (int j = 0; j < actor.homeStructure.residents.Count; j++)
			{
				Character character2 = actor.homeStructure.residents[j];
				if (!character2.isDead && character2.limiterComponent.canMove && character2.limiterComponent.canPerform && !character2.isBeingSeized && character2 is GiantSpider)
				{
					list.Add(character2);
				}
			}
		}
		else if (actor.HasTerritory())
		{
			for (int k = 0; k < actor.homeRegion.residents.Count; k++)
			{
				Character character3 = actor.homeRegion.residents[k];
				if (!character3.isDead && character3.limiterComponent.canMove && character3.limiterComponent.canPerform && !character3.isBeingSeized && character3 is GiantSpider && character3.IsTerritory(actor.territory))
				{
					list.Add(character3);
				}
			}
		}
		for (int l = 0; l < list.Count; l++)
		{
			Character character4 = list[l];
			Character randomResidentForInvasionTargetThatIsInsideSettlement = baseSettlement.GetRandomResidentForInvasionTargetThatIsInsideSettlement(baseSettlement, actor);
			if (randomResidentForInvasionTargetThatIsInsideSettlement != null)
			{
				character4.jobComponent.TriggerAttackVillager(JOB_TYPE.SLAY_TARGET, randomResidentForInvasionTargetThatIsInsideSettlement, out var p_producedJob);
				if (p_producedJob != null)
				{
					character4.jobQueue.AddJobInQueue(p_producedJob);
				}
				continue;
			}
			break;
		}
	}
}
