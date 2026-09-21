using Inner_Maps.Location_Structures;
using UtilityScripts;

public class Golem : Summon
{
	public Golem()
		: base(SUMMON_TYPE.Golem, "Golem", RACE.GOLEM, Utilities.GetRandomGender())
	{
		base.visuals.SetHasBlood(state: false);
		base.traitContainer.AddTrait(this, "Indestructible");
		base.traitContainer.AddTrait(this, "Hibernating");
	}

	public Golem(string className)
		: base(SUMMON_TYPE.Golem, className, RACE.GOLEM, Utilities.GetRandomGender())
	{
		base.visuals.SetHasBlood(state: false);
		base.traitContainer.AddTrait(this, "Indestructible");
		base.traitContainer.AddTrait(this, "Hibernating");
	}

	public Golem(SaveDataSummon data)
		: base(data)
	{
	}

	public override void OnSummonAsPlayerMonster()
	{
		base.OnSummonAsPlayerMonster();
		base.traitContainer.RemoveTrait(this, "Indestructible");
		base.traitContainer.RemoveTrait(this, "Hibernating");
	}

	public override void SubscribeToSignals()
	{
		if (!base.hasSubscribedToSignals)
		{
			base.SubscribeToSignals();
			Messenger.AddListener<Character, Area>(CharacterSignals.CHARACTER_EXITED_AREA, OnCharacterExitedArea);
			Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
		}
	}

	public override void UnsubscribeSignals()
	{
		if (base.hasSubscribedToSignals)
		{
			base.UnsubscribeSignals();
			Messenger.RemoveListener<Character, Area>(CharacterSignals.CHARACTER_EXITED_AREA, OnCharacterExitedArea);
			Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
		}
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

	private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure)
	{
		if (!base.behaviourComponent.HasBehaviour(typeof(AttackVillageBehaviour)) && (base.faction == null || base.faction == defaultFaction) && character != this && base.combatComponent.isInCombat && base.homeStructure != null && structure != base.homeStructure)
		{
			base.combatComponent.RemoveHostileInRange(character);
			ForceCancelAllJobsTargetingPOI(character, string.Empty);
		}
	}

	private void OnCharacterExitedArea(Character character, Area p_area)
	{
		if (character != this && base.combatComponent.isInCombat && HasTerritory() && IsTerritory(p_area) && !character.IsInTerritoryOf(this))
		{
			base.combatComponent.RemoveHostileInRange(character);
			ForceCancelAllJobsTargetingPOI(character, string.Empty);
		}
	}
}
