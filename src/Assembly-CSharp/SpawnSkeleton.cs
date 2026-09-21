using Inner_Maps;

public class SpawnSkeleton : GoapAction
{
	public SpawnSkeleton()
		: base(INTERACTION_TYPE.SPAWN_SKELETON)
	{
		base.actionIconString = GoapActionStateDB.Magic_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Spawn Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public void AfterSpawnSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.actor.necromancerTrait != null)
		{
			goapNode.actor.necromancerTrait.AdjustEnergy(-5);
		}
		LocationGridTile locationGridTile = goapNode.actor.gridTileLocation.GetRandomUnoccupiedNeighbor();
		if (locationGridTile == null)
		{
			locationGridTile = goapNode.actor.gridTileLocation;
		}
		Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Skeleton, goapNode.actor.faction, null, locationGridTile.parentMap.region, null, "", bypassIdeologyChecking: true);
		CharacterManager.Instance.PlaceSummonInitially(summon, locationGridTile);
		GameManager.Instance.CreateParticleEffectAt(summon, PARTICLE_EFFECT.Spawn_Effect);
	}
}
