using System.Collections.Generic;

namespace Traits;

public class Grounded : Trait
{
	private Character _owner;

	private static readonly List<PLAYER_SKILL_TYPE> SkillsToIgnore = new List<PLAYER_SKILL_TYPE>
	{
		PLAYER_SKILL_TYPE.SNATCH_VILLAGER,
		PLAYER_SKILL_TYPE.EVANGELIZE,
		PLAYER_SKILL_TYPE.CULTIST_JOIN_FACTION,
		PLAYER_SKILL_TYPE.CULTIST_BOOBY_TRAP,
		PLAYER_SKILL_TYPE.CULTIST_POISON,
		PLAYER_SKILL_TYPE.ABSORB_CULTIST,
		PLAYER_SKILL_TYPE.KILL_VILLAGER
	};

	public Grounded()
	{
		name = "Grounded";
		description = "Drains all of player’s remaining Mana when hit by any player Power.";
		type = TRAIT_TYPE.BUFF;
		effect = TRAIT_EFFECT.POSITIVE;
		ticksDuration = 0;
		AddTraitOverrideFunctionIdentifier("Death_Trait");
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character owner)
		{
			_owner = owner;
			Messenger.AddListener<Character, int, object>(CharacterSignals.CHARACTER_ADJUSTED_HP, OnCharacterAdjustedHP);
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character owner)
		{
			_owner = owner;
			Messenger.AddListener<Character, int, object>(CharacterSignals.CHARACTER_ADJUSTED_HP, OnCharacterAdjustedHP);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		_owner = null;
		Messenger.RemoveListener<Character, int, object>(CharacterSignals.CHARACTER_ADJUSTED_HP, OnCharacterAdjustedHP);
	}

	public override bool OnDeath(Character character)
	{
		character.traitContainer.RemoveTrait(character, this);
		return true;
	}

	private void OnCharacterAdjustedHP(Character p_character, int p_amount, object p_source)
	{
		if (p_character == _owner && p_source is SkillData p_skill)
		{
			DrainManaFromGrounded(p_skill);
		}
	}

	public void DrainManaFromGrounded(SkillData p_skill)
	{
		if (!SkillsToIgnore.Contains(p_skill.type) && PlayerManager.Instance.player.currenciesComponent.mana > 0 && !_owner.isDead)
		{
			PlayerManager.Instance.player.currenciesComponent.AdjustMana(-PlayerManager.Instance.player.currenciesComponent.mana);
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Traits", "Traits_Table", "Grounded mana_drain", LOG_TAG.Player, LOG_TAG.Major);
			log.AddToFillers(null, p_skill.localizedName, LOG_IDENTIFIER.STRING_1);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = _owner;
	}
}
