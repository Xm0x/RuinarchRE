using Inner_Maps;

public class LayEgg : GoapAction
{
	public LayEgg()
		: base(INTERACTION_TYPE.LAY_EGG)
	{
		base.actionIconString = GoapActionStateDB.Question_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.logTags = new LOG_TAG[1];
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Lay Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (actor.gridTileLocation != null)
			{
				return actor.gridTileLocation.tileObjectComponent.objHere == null;
			}
			return false;
		}
		return false;
	}

	public void AfterLaySuccess(ActualGoapNode goapNode)
	{
		if (goapNode.actor is Summon { gridTileLocation: not null } summon && summon.gridTileLocation.tileObjectComponent.objHere == null)
		{
			TILE_OBJECT_TYPE eggType = CharacterManager.Instance.GetEggType(summon.summonType);
			if (eggType != TILE_OBJECT_TYPE.NONE)
			{
				MonsterEgg monsterEgg = InnerMapManager.Instance.CreateNewTileObject<MonsterEgg>(eggType);
				monsterEgg.SetCharacterThatLay(summon);
				summon.gridTileLocation.structure.AddPOI(monsterEgg, summon.gridTileLocation);
				summon.behaviourComponent.SetHasLayedAnEgg(state: true);
			}
		}
	}
}
