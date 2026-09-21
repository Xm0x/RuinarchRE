using Inner_Maps;
using Traits;

public class SkillBaseChaosOrb : PassiveSkill
{
	public override string name => "Mana Orbs from skills";

	public override string description => "Mana Orbs from skills";

	public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Skill_Base_Chaos_Orb;

	public override void ActivateSkill()
	{
		Messenger.AddListener<IPointOfInterest>(PlayerSkillSignals.ZAP_ACTIVATED, OnZapDone);
		Messenger.AddListener<IPointOfInterest>(PlayerSkillSignals.AGITATE_ACTIVATED, OnAgitateDone);
		Messenger.AddListener<IPointOfInterest>(PlayerSkillSignals.EXPEL_ACTIVATED, OnExpelDone);
		Messenger.AddListener<IPointOfInterest>(PlayerSkillSignals.REMOVE_BUFF_ACTIVATED, OnRemoveBuffDone);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
		Messenger.AddListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedAction);
	}

	private void OnZapDone(IPointOfInterest p_character)
	{
		if (p_character is Character character && character.race.IsSapient() && character.isNormalAndNotAlliedWithPlayer && PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.ZAP).TryDecreaseRemainingChaosOrbs(1))
		{
			Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.worldPosition, 1, p_character.gridTileLocation.parentMap);
		}
	}

	private void OnAgitateDone(IPointOfInterest p_character)
	{
		if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.AGITATE).TryDecreaseRemainingChaosOrbs(1))
		{
			Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.worldPosition, 1, p_character.gridTileLocation.parentMap);
		}
	}

	private void OnRemoveBuffDone(IPointOfInterest p_character)
	{
		if (p_character is Character character && character.race.IsSapient() && character.isNormalAndNotAlliedWithPlayer && PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.REMOVE_BUFF).TryDecreaseRemainingChaosOrbs(1))
		{
			Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.worldPosition, 1, p_character.gridTileLocation.parentMap);
		}
	}

	private void OnExpelDone(IPointOfInterest p_character)
	{
		if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.EXPEL).TryDecreaseRemainingChaosOrbs(1))
		{
			Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.worldPosition, 1, p_character.gridTileLocation.parentMap);
		}
	}

	private void OnCharacterDied(Character p_character)
	{
		if (!p_character.isNormalAndNotAlliedWithPlayer)
		{
			return;
		}
		Character responsibleCharacter = p_character.traitContainer.GetTraitOrStatus<Trait>("Dead").responsibleCharacter;
		if (responsibleCharacter is Summon && responsibleCharacter.traitContainer.HasTrait("Agitated"))
		{
			int p_amount = 2;
			if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.AGITATE).TryDecreaseRemainingChaosOrbs(ref p_amount))
			{
				LocationGridTile locationGridTile = ((!responsibleCharacter.hasMarker) ? responsibleCharacter.deathTilePosition : responsibleCharacter.gridTileLocation);
				Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, locationGridTile.centeredWorldLocation, p_amount, locationGridTile.parentMap);
			}
		}
	}

	private void OnTraitableGainedTrait(ITraitable p_traitable, Trait p_trait)
	{
		Character character = p_traitable as Character;
		if (p_trait != null && character != null && character.race.IsSapient() && character.isNormalAndNotAlliedWithPlayer && character.hasMarker && (p_trait.name == "Malnourished" || p_trait.name == "Unconscious"))
		{
			LocationGridTile gridTileLocation = character.gridTileLocation;
			if (gridTileLocation != null)
			{
				Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, gridTileLocation.centeredWorldLocation, 1, gridTileLocation.parentMap);
			}
		}
	}

	private void OnCharacterFinishedAction(Character p_character, GoapPlanJob p_job)
	{
		if (p_job.isTriggeredFlaw)
		{
			int p_amount = 2;
			if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.TRIGGER_FLAW).TryDecreaseRemainingChaosOrbs(ref p_amount))
			{
				Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.worldPosition, p_amount, p_character.gridTileLocation.parentMap);
			}
		}
		else if (p_job.jobType == JOB_TYPE.GRUDGE)
		{
			int p_amount2 = 2;
			if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.TRIGGER_GRUDGE).TryDecreaseRemainingChaosOrbs(ref p_amount2))
			{
				Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.worldPosition, p_amount2, p_character.gridTileLocation.parentMap);
			}
		}
	}
}
