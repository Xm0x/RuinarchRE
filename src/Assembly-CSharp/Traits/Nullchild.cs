using System;
using System.Collections.Generic;

namespace Traits;

public class Nullchild : Trait
{
	private Character _owner;

	private static List<PLAYER_SKILL_TYPE> _skillsToIgnore;

	public List<PLAYER_SKILL_TYPE> lockedSkills { get; private set; }

	public override Type serializedData => typeof(SaveDataNullchild);

	public Nullchild()
	{
		name = "Nullchild";
		description = "Locks player Powers used on it until it dies.";
		type = TRAIT_TYPE.BUFF;
		effect = TRAIT_EFFECT.POSITIVE;
		ticksDuration = 0;
		lockedSkills = new List<PLAYER_SKILL_TYPE>();
		if (_skillsToIgnore == null)
		{
			_skillsToIgnore = new List<PLAYER_SKILL_TYPE>();
			_skillsToIgnore.AddRange(PlayerSkillManager.Instance.constantSkills);
			_skillsToIgnore.Remove(PLAYER_SKILL_TYPE.SCHEME);
		}
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

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		if (saveDataTrait is SaveDataNullchild saveDataNullchild)
		{
			lockedSkills = saveDataNullchild.lockedSkill;
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
		UnlockLockedSkills();
		Messenger.RemoveListener<Character, int, object>(CharacterSignals.CHARACTER_ADJUSTED_HP, OnCharacterAdjustedHP);
	}

	public override bool OnDeath(Character character)
	{
		UnlockLockedSkills();
		Messenger.RemoveListener<Character, int, object>(CharacterSignals.CHARACTER_ADJUSTED_HP, OnCharacterAdjustedHP);
		return false;
	}

	protected override string GetDescriptionInUI()
	{
		string text = base.GetDescriptionInUI();
		if (lockedSkills.Count > 0)
		{
			text = text + "\n" + LocalizationManager.Instance.GetLocalizedValue("Traits_Table", "Locked_Skills") + ": ";
			for (int i = 0; i < lockedSkills.Count; i++)
			{
				PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = lockedSkills[i];
				SkillData skillData = PlayerSkillManager.Instance.GetSkillData(pLAYER_SKILL_TYPE);
				text = text + skillData.localizedName + ", ";
			}
		}
		return text;
	}

	private void OnCharacterAdjustedHP(Character p_character, int p_amount, object p_source)
	{
		if (p_character == _owner && !p_character.isDead && p_source is SkillData { type: not PLAYER_SKILL_TYPE.SNARE_TRAP, type: not PLAYER_SKILL_TYPE.FREEZING_TRAP, type: not PLAYER_SKILL_TYPE.LANDMINE } skillData)
		{
			TryLockSkillUsedOnCharacter(skillData);
		}
	}

	public void TryLockSkillUsedOnCharacter(SkillData p_data)
	{
		if (!_owner.isDead && p_data.isUsable && !lockedSkills.Contains(p_data.type) && !_skillsToIgnore.Contains(p_data.type))
		{
			lockedSkills.Add(p_data.type);
			PlayerManager.Instance.player.playerSkillComponent.LockSkillFromNullchild(p_data.type, _owner);
		}
	}

	private void UnlockLockedSkills()
	{
		for (int i = 0; i < lockedSkills.Count; i++)
		{
			PLAYER_SKILL_TYPE p_type = lockedSkills[i];
			PlayerManager.Instance.player.playerSkillComponent.UnlockSkill(p_type);
		}
		lockedSkills.Clear();
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = _owner;
	}
}
