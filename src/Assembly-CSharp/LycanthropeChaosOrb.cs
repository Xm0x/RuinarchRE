using Inner_Maps;
using Traits;

public class LycanthropeChaosOrb : PassiveSkill
{
	public override string name => "Chaos Orbs from Lycanthropes";

	public override string description => "Chaos Orbs from Lycanthropes actions";

	public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Lycanthrope_Chaos_Orb;

	public override void ActivateSkill()
	{
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.AddListener<Character>(CharacterSignals.LYCANTHROPE_SHED_WOLF_PELT, OnLycanthropeShedWolfPelt);
	}

	private void OnCharacterDied(Character p_character)
	{
		LocationGridTile locationGridTile = p_character.gridTileLocation;
		if (locationGridTile == null)
		{
			locationGridTile = p_character.deathTilePosition;
		}
		if (!p_character.race.IsSapient() || locationGridTile == null)
		{
			return;
		}
		Character responsibleCharacter = p_character.traitContainer.GetTraitOrStatus<Trait>("Dead").responsibleCharacter;
		if (responsibleCharacter != null && responsibleCharacter.traitContainer.HasTrait("Lycanthrope"))
		{
			int p_amount = 2;
			if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.LYCANTHROPY).TryDecreaseRemainingChaosOrbs(ref p_amount))
			{
				Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, locationGridTile.centeredWorldLocation, p_amount, locationGridTile.parentMap);
			}
		}
	}

	private void OnLycanthropeShedWolfPelt(Character p_character)
	{
		int p_amount = 2;
		if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.LYCANTHROPY).TryDecreaseRemainingChaosOrbs(ref p_amount))
		{
			Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.gridTileLocation.centeredWorldLocation, p_amount, p_character.gridTileLocation.parentMap);
		}
	}
}
