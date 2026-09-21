using System;
using UtilityScripts;

public class Wyvern : Summon
{
	public override bool defaultDigMode => true;

	public override Type serializedData => typeof(SaveDataWyvern);

	public Wyvern()
		: base(SUMMON_TYPE.Wyvern, "Wyvern", RACE.WYVERN, Utilities.GetRandomGender())
	{
	}

	public Wyvern(string className)
		: base(SUMMON_TYPE.Wyvern, className, RACE.WYVERN, Utilities.GetRandomGender())
	{
	}

	public Wyvern(SaveDataWyvern data)
		: base(data)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		base.movementComponent.SetToFlying();
	}

	public override void LoadReferences(SaveDataCharacter data)
	{
		base.LoadReferences(data);
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
