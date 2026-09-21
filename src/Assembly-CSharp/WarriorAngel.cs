using System.Collections.Generic;
using UtilityScripts;

public class WarriorAngel : Summon
{
	public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Defend;

	public override SUMMON_TYPE gainedKennelSummonType => SUMMON_TYPE.Fallen_Angel;

	public WarriorAngel()
		: base(SUMMON_TYPE.Warrior_Angel, "Warrior Angel", RACE.ANGEL, Utilities.GetRandomGender())
	{
	}

	public WarriorAngel(string className)
		: base(SUMMON_TYPE.Warrior_Angel, className, RACE.ANGEL, Utilities.GetRandomGender())
	{
	}

	public WarriorAngel(SaveDataSummon data)
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
