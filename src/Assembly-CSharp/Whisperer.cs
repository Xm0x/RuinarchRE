using UtilityScripts;

public class Whisperer : Summon
{
	public const string ClassName = "Whisperer";

	public override bool defaultDigMode => true;

	public Whisperer()
		: base(SUMMON_TYPE.Whisperer, "Whisperer", RACE.WHISPERER, Utilities.GetRandomGender())
	{
	}

	public Whisperer(string className)
		: base(SUMMON_TYPE.Whisperer, className, RACE.WHISPERER, Utilities.GetRandomGender())
	{
	}

	public Whisperer(SaveDataSummon data)
		: base(data)
	{
	}

	public override bool Agitate(ref JobQueueItem p_agitateJob)
	{
		return AgitateAttackNearbyVillager(ref p_agitateJob);
	}

	protected override string GetAgitateTooltipKey()
	{
		return AGITATE_MESSAGE_TYPE.Attack_Villager_Tooltip.ToStringEnum();
	}

	public override bool ReactionToAnotherCharacter(Character actor, Character targetCharacter, Character disguisedActor, Character disguisedTarget, bool isHostile, ref string debugLog)
	{
		if (targetCharacter is Wisp && !targetCharacter.isDead && actor.limiterComponent.canPerform)
		{
			actor.jobComponent.TriggerAbsorbWisp(targetCharacter);
			return true;
		}
		return base.ReactionToAnotherCharacter(actor, targetCharacter, disguisedActor, disguisedTarget, isHostile, ref debugLog);
	}
}
