using UtilityScripts;

public class FallenAngel : Summon
{
	public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Aggressive;

	public FallenAngel()
		: base(SUMMON_TYPE.Fallen_Angel, "Fallen Angel", RACE.LESSER_DEMON, Utilities.GetRandomGender())
	{
	}

	public FallenAngel(string className)
		: base(SUMMON_TYPE.Fallen_Angel, className, RACE.LESSER_DEMON, Utilities.GetRandomGender())
	{
	}

	public FallenAngel(SaveDataSummon data)
		: base(data)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		base.traitContainer.AddTrait(this, "Sturdy");
		base.movementComponent.SetToFlying();
		SetDestroyMarkerOnDeath(state: true);
	}

	public override bool Agitate(ref JobQueueItem p_agitateJob)
	{
		return AgitateAttackNearbyVillager(ref p_agitateJob);
	}

	protected override string GetAgitateTooltipKey()
	{
		return AGITATE_MESSAGE_TYPE.Attack_Villager_Tooltip.ToStringEnum();
	}
}
