namespace Inner_Maps.Location_Structures;

public class Cemetery : ManMadeStructure
{
	public Cemetery(Region location)
		: base(STRUCTURE_TYPE.CEMETERY, location)
	{
		base.wallsAreMadeOf = WALL_RESOURCE.Wood;
	}

	public Cemetery(Region location, SaveDataManMadeStructure data)
		: base(location, data)
	{
		base.wallsAreMadeOf = WALL_RESOURCE.Wood;
	}

	public override void OnTileDamaged(LocationGridTile tile, int amount, bool isPlayerSource)
	{
		AdjustHP(amount, null, isPlayerSource);
		OnStructureDamaged();
	}

	public override bool DoesTileContributeToDamage(LocationGridTile tile)
	{
		return true;
	}

	protected override void AfterStructureDestruction(Character p_responsibleCharacter = null)
	{
		base.AfterStructureDestruction(p_responsibleCharacter);
		Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY_OF_ALL_JOBS_OF_TYPE, JOB_TYPE.BURY);
	}

	public override void OnBuiltNewStructure()
	{
		base.OnBuiltNewStructure();
		Messenger.Broadcast(CharacterSignals.TRY_CREATE_BURY_JOBS, base.settlementLocation);
	}
}
