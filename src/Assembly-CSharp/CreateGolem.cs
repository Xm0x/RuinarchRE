using Inner_Maps;
using Inner_Maps.Location_Structures;

public class CreateGolem : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;

	public CreateGolem()
		: base(INTERACTION_TYPE.CREATE_GOLEM)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
		base.actionIconString = GoapActionStateDB.Magic_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		base.showNotification = true;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Create Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override LocationStructure GetTargetStructure(ActualGoapNode node)
	{
		Character actor = node.actor;
		if (actor.homeSettlement != null)
		{
			return actor.homeSettlement.cityCenter;
		}
		return null;
	}

	public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode)
	{
		return null;
	}

	public void AfterCreateSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Golem, actor.faction, actor.homeSettlement, GridMap.Instance.mainRegion, null, "", bypassIdeologyChecking: true);
		CharacterManager.Instance.PlaceSummonInitially(summon, goapNode.actor.gridTileLocation);
		summon.traitContainer.RemoveTrait(summon, "Hibernating");
		summon.traitContainer.RemoveTrait(summon, "Indestructible");
		GameManager.Instance.CreateParticleEffectAt(summon, PARTICLE_EFFECT.Spawn_Effect);
	}
}
