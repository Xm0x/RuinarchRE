using Traits;
using UtilityScripts;

public class FireElemental : Summon
{
	public const string ClassName = "Fire Elemental";

	public FireElemental()
		: base(SUMMON_TYPE.Fire_Elemental, "Fire Elemental", RACE.ELEMENTAL, Utilities.GetRandomGender())
	{
	}

	public FireElemental(string className)
		: base(SUMMON_TYPE.Fire_Elemental, className, RACE.ELEMENTAL, Utilities.GetRandomGender())
	{
	}

	public FireElemental(SaveDataSummon data)
		: base(data)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		base.traitContainer.AddTrait(this, "Fire Resistant");
	}

	public override void SubscribeToSignals()
	{
		if (!base.hasSubscribedToSignals)
		{
			base.SubscribeToSignals();
			Messenger.AddListener<Character, IPointOfInterest, INTERACTION_TYPE, ACTION_STATUS>(JobSignals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
		}
	}

	public override void UnsubscribeSignals()
	{
		if (base.hasSubscribedToSignals)
		{
			base.UnsubscribeSignals();
			Messenger.RemoveListener<Character, IPointOfInterest, INTERACTION_TYPE, ACTION_STATUS>(JobSignals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
		}
	}

	private void OnCharacterFinishedAction(Character p_actor, IPointOfInterest p_target, INTERACTION_TYPE p_type, ACTION_STATUS p_status)
	{
		if (p_actor == this && p_type == INTERACTION_TYPE.STAND)
		{
			Burning burning = TraitManager.Instance.CreateNewInstancedTraitClass<Burning>("Burning");
			burning.SetSourceOfBurning(new BurningSource(), base.gridTileLocation.tileObjectComponent.genericTileObject);
			base.gridTileLocation.tileObjectComponent.genericTileObject.traitContainer.AddTrait(base.gridTileLocation.tileObjectComponent.genericTileObject, burning, this, bypassElementalChance: true);
		}
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
