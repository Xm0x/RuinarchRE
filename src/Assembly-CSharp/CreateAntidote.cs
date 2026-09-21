using Inner_Maps;

public class CreateAntidote : GoapAction
{
	public CreateAntidote()
		: base(INTERACTION_TYPE.CREATE_ANTIDOTE)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.actionIconString = GoapActionStateDB.Work_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, "Poison Flask", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasPoisonFlask);
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, "Antidote", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Create Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 250;
	}

	public void AfterCreateSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		if (actor.HasItem(TILE_OBJECT_TYPE.POISON_FLASK))
		{
			actor.ObtainItem(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.ANTIDOTE));
			actor.UnobtainItem(TILE_OBJECT_TYPE.POISON_FLASK);
		}
	}

	private bool HasPoisonFlask(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType)
	{
		return actor.HasItem("Poison Flask");
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return actor == poiTarget;
		}
		return false;
	}
}
