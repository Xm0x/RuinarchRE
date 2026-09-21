using Inner_Maps.Location_Structures;
using UtilityScripts;

public class BoneGolem : Summon
{
	public BoneGolem()
		: base(SUMMON_TYPE.Bone_Golem, "Bone Golem", RACE.GOLEM, Utilities.GetRandomGender())
	{
		base.visuals.SetHasBlood(state: false);
		base.traitContainer.AddTrait(this, "Fire Prone");
	}

	public BoneGolem(string className)
		: base(SUMMON_TYPE.Bone_Golem, className, RACE.GOLEM, Utilities.GetRandomGender())
	{
		base.visuals.SetHasBlood(state: false);
		base.traitContainer.AddTrait(this, "Fire Prone");
	}

	public BoneGolem(SaveDataSummon data)
		: base(data)
	{
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

	public override void LoadReferencesMainThread(SaveDataCharacter data)
	{
		base.LoadReferencesMainThread(data);
		base.visuals.SetHasBlood(state: false);
	}

	private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure)
	{
		if (!base.behaviourComponent.HasBehaviour(typeof(AttackVillageBehaviour)) && (base.faction == null || base.faction == defaultFaction) && character != this && base.combatComponent.isInCombat && base.homeStructure != null && structure != base.homeStructure)
		{
			base.combatComponent.RemoveHostileInRange(character);
		}
	}

	private void OnCharacterExitedArea(Character character, Area p_area)
	{
		if (character != this && base.combatComponent.isInCombat && HasTerritory() && IsTerritory(p_area) && !IsTerritory(character.gridTileLocation.area))
		{
			base.combatComponent.RemoveHostileInRange(character);
		}
	}
}
