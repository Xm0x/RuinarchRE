using System.Collections.Generic;
using UtilityScripts;

public class MagicalAngel : Summon
{
	public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Defend;

	public override SUMMON_TYPE gainedKennelSummonType => SUMMON_TYPE.Fallen_Angel;

	public MagicalAngel()
		: base(SUMMON_TYPE.Magical_Angel, "Magical Angel", RACE.ANGEL, Utilities.GetRandomGender())
	{
	}

	public MagicalAngel(string className)
		: base(SUMMON_TYPE.Magical_Angel, className, RACE.ANGEL, Utilities.GetRandomGender())
	{
	}

	public MagicalAngel(SaveDataSummon data)
		: base(data)
	{
	}

	public override void ConstructDefaultPlayerActions(bool broadcastSignal = true)
	{
		if (base.actions == null)
		{
			base.actions = new List<PLAYER_SKILL_TYPE>();
		}
		else
		{
			base.actions.Clear();
		}
		AddPlayerAction(PLAYER_SKILL_TYPE.ZAP, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.SNATCH_MONSTER, broadcastSignal);
	}

	public override void Initialize()
	{
		base.Initialize();
		base.movementComponent.SetToFlying();
	}
}
