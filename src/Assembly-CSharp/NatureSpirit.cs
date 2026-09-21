using System.Collections.Generic;
using UtilityScripts;

public class NatureSpirit : Summon
{
	public const string ClassName = "Nature Spirit";

	public NatureSpirit()
		: base(SUMMON_TYPE.Nature_Spirit, "Nature Spirit", RACE.SPIRIT, Utilities.GetRandomGender())
	{
	}

	public NatureSpirit(string className)
		: base(SUMMON_TYPE.Nature_Spirit, className, RACE.SPIRIT, Utilities.GetRandomGender())
	{
	}

	public NatureSpirit(SaveDataSummon data)
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
		AddPlayerAction(PLAYER_SKILL_TYPE.AGITATE, broadcastSignal);
	}

	public override bool Agitate(ref JobQueueItem p_agitateJob)
	{
		if (base.limiterComponent.IsIncapacitated())
		{
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Incapacitated);
			return false;
		}
		CreateAgitateLog(AGITATE_MESSAGE_TYPE.Agitate_Success);
		base.interruptComponent.TriggerInterrupt(INTERRUPT.Wide_Heal, this);
		return true;
	}

	public override bool ReactionToAnotherCharacter(Character actor, Character targetCharacter, Character disguisedActor, Character disguisedTarget, bool isHostile, ref string debugLog)
	{
		if (disguisedTarget.faction != null && actor.faction != null && (actor.faction.IsFriendlyWith(disguisedTarget.faction) || actor.faction == disguisedTarget.faction) && !targetCharacter.IsHealthFull() && !targetCharacter.isDead)
		{
			actor.interruptComponent.TriggerInterrupt(INTERRUPT.Heal_Other, targetCharacter);
		}
		return base.ReactionToAnotherCharacter(actor, targetCharacter, disguisedActor, disguisedTarget, isHostile, ref debugLog);
	}
}
