using UtilityScripts;

public class Skeleton : Summon
{
	public override Faction defaultFaction => FactionManager.Instance.undeadFaction;

	public Skeleton()
		: base(SUMMON_TYPE.Skeleton, "Skeleton", RACE.SKELETON, Utilities.GetRandomGender())
	{
		base.visuals.SetHasBlood(state: false);
	}

	public Skeleton(string className)
		: base(SUMMON_TYPE.Skeleton, className, RACE.SKELETON, Utilities.GetRandomGender())
	{
		base.visuals.SetHasBlood(state: false);
	}

	public Skeleton(SaveDataSummon data)
		: base(data)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		base.isWildMonster = false;
	}

	protected override void OnTickEnded()
	{
		base.OnTickEnded();
		PerTickOutsideCombatHPRecovery();
	}

	public override void SubscribeToSignals()
	{
		if (!base.hasSubscribedToSignals)
		{
			base.SubscribeToSignals();
			Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterChangesFaction);
		}
	}

	public override void UnsubscribeSignals()
	{
		if (base.hasSubscribedToSignals)
		{
			base.UnsubscribeSignals();
			Messenger.RemoveListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterChangesFaction);
		}
	}

	public override void LoadReferencesMainThread(SaveDataCharacter data)
	{
		base.LoadReferencesMainThread(data);
		base.visuals.SetHasBlood(state: false);
	}

	public override bool Agitate(ref JobQueueItem p_agitateJob)
	{
		return AgitateAttackNearbyVillager(ref p_agitateJob);
	}

	protected override string GetAgitateTooltipKey()
	{
		return AGITATE_MESSAGE_TYPE.Attack_Villager_Tooltip.ToStringEnum();
	}

	private void OnCharacterChangesFaction(Character p_character, Faction p_faction)
	{
		if (p_character.necromancerTrait != null && p_faction != base.faction && p_character.prevFaction == base.faction && p_character.prevFaction != null)
		{
			base.interruptComponent.TriggerInterrupt(INTERRUPT.Join_Faction, p_character, "join_faction_necro");
		}
	}
}
