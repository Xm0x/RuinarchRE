using UtilityScripts;

public class VengefulGhost : Summon
{
	public override Faction defaultFaction => FactionManager.Instance.undeadFaction;

	public VengefulGhost()
		: base(SUMMON_TYPE.Vengeful_Ghost, "Vengeful Ghost", RACE.GHOST, Utilities.GetRandomGender())
	{
		base.visuals.SetHasBlood(state: false);
	}

	public VengefulGhost(string className)
		: base(SUMMON_TYPE.Vengeful_Ghost, className, RACE.GHOST, Utilities.GetRandomGender())
	{
		base.visuals.SetHasBlood(state: false);
	}

	public VengefulGhost(SaveDataSummon data)
		: base(data)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		base.movementComponent.SetToFlying();
		RemoveAdvertisedAction(INTERACTION_TYPE.BURY_CHARACTER);
		base.isWildMonster = false;
	}

	protected override void OnChangeFaction(Faction prevFaction, Faction newFaction)
	{
		base.OnChangeFaction(prevFaction, newFaction);
		base.behaviourComponent.ResetInvadeVillageTarget();
	}

	protected override void OnTickEnded()
	{
		base.OnTickEnded();
		PerTickOutsideCombatHPRecovery();
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

	public override bool ReactionToAnotherCharacter(Character actor, Character targetCharacter, Character disguisedActor, Character disguisedTarget, bool isHostile, ref string debugLog)
	{
		return true;
	}
}
