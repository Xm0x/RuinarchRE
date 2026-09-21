using Inner_Maps;
using Traits;

public class NightCreatureChaosOrb : PassiveSkill
{
	public override string name => "Chaos Orbs from Night Creatures";

	public override string description => "Chaos Orbs from Night Creature actions";

	public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Night_Creature_Chaos_Orb;

	public override void ActivateSkill()
	{
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_BECAME_VAMPIRE, OnCharacterBecameVampire);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.AddListener<Summon>(CharacterSignals.ON_CHARACTER_RAISE_DEAD_BY_NECRO, OnCharacterRaiseDeadByNecro);
	}

	private void OnCharacterRaiseDeadByNecro(Summon p_character)
	{
		if (CharacterManager.Instance.necromancerInTheWorld != null && CharacterManager.Instance.necromancerInTheWorld.faction.factionType.type == FACTION_TYPE.Undead)
		{
			LocationGridTile locationGridTile = ((!p_character.hasMarker) ? p_character.deathTilePosition : p_character.gridTileLocation);
			if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.SPAWN_NECRONOMICON).TryDecreaseRemainingChaosOrbs(1))
			{
				Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, locationGridTile.centeredWorldLocation, 1, locationGridTile.parentMap);
			}
		}
	}

	private void OnCharacterBecameVampire(Character p_character)
	{
		int p_amount = 3;
		if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.VAMPIRISM).TryDecreaseRemainingChaosOrbs(ref p_amount))
		{
			Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.gridTileLocation.centeredWorldLocation, p_amount, p_character.gridTileLocation.parentMap);
		}
	}

	private void OnCharacterDied(Character p_character)
	{
		if (!p_character.race.IsSapient())
		{
			return;
		}
		Character responsibleCharacter = p_character.traitContainer.GetTraitOrStatus<Trait>("Dead").responsibleCharacter;
		if (responsibleCharacter == null)
		{
			return;
		}
		int p_amount = 2;
		bool flag = false;
		if (responsibleCharacter.race == RACE.SKELETON && responsibleCharacter.faction.factionType.type == FACTION_TYPE.Undead && CharacterManager.Instance.necromancerInTheWorld != null && CharacterManager.Instance.necromancerInTheWorld.faction == responsibleCharacter.faction)
		{
			flag = true;
			if (!PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.SPAWN_NECRONOMICON).TryDecreaseRemainingChaosOrbs(ref p_amount))
			{
				flag = false;
			}
		}
		else if (responsibleCharacter.traitContainer.HasTrait("Necromancer"))
		{
			flag = true;
			if (!PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.SPAWN_NECRONOMICON).TryDecreaseRemainingChaosOrbs(ref p_amount))
			{
				flag = false;
			}
		}
		else if (responsibleCharacter.traitContainer.HasTrait("Vampire"))
		{
			flag = true;
			if (!PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.VAMPIRISM).TryDecreaseRemainingChaosOrbs(ref p_amount))
			{
				flag = false;
			}
		}
		if (flag)
		{
			LocationGridTile locationGridTile = ((!responsibleCharacter.hasMarker) ? responsibleCharacter.deathTilePosition : responsibleCharacter.gridTileLocation);
			Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, locationGridTile.centeredWorldLocation, p_amount, locationGridTile.parentMap);
		}
	}
}
