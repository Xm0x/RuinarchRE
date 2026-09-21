using System;

namespace Traits;

public class Protection : Status
{
	private StatusIcon _statusIcon;

	private Character _owner;

	public int appliedAllResistances { get; private set; }

	public override Type serializedData => typeof(SaveDataProtection);

	public override bool shouldBeLoadedInMainThread => true;

	public Protection()
	{
		name = "Protection";
		description = "Surrounded by a magical barrier that reduces damage.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.PROTECTION);
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataProtection saveDataProtection = saveDataTrait as SaveDataProtection;
		appliedAllResistances = saveDataProtection.appliedAllResistances;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character owner)
		{
			_owner = owner;
		}
	}

	public override void LoadTraitSecondWaveInMainThread(SaveDataTrait p_saveDataTrait)
	{
		base.LoadTraitSecondWaveInMainThread(p_saveDataTrait);
		UpdateVisualsOnAdd(_owner);
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		UpdateVisualsOnAdd(addedTo);
		if (addedTo is Character character)
		{
			_owner = character;
			PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.PROTECTION);
			SkillData skillData = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.PROTECTION);
			appliedAllResistances = (int)scriptableObjPlayerSkillData.skillUpgradeData.GetIncreaseStatsPercentagePerLevel(skillData.currentLevel);
			character.piercingAndResistancesComponent.AdjustAllResistances(appliedAllResistances);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		UpdateVisualsOnRemove(removedFrom);
		if (removedFrom is Character character)
		{
			_owner = null;
			character.piercingAndResistancesComponent.AdjustAllResistances(-appliedAllResistances);
		}
	}

	private void UpdateVisualsOnAdd(ITraitable addedTo)
	{
		if (addedTo is Character character && _statusIcon == null && character.hasMarker)
		{
			_statusIcon = character.marker.AddStatusIcon("Protected");
		}
	}

	private void UpdateVisualsOnRemove(ITraitable removedFrom)
	{
		if (removedFrom is Character { hasMarker: not false })
		{
			ObjectPoolManager.Instance.DestroyObject(_statusIcon.gameObject);
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = _owner;
	}
}
