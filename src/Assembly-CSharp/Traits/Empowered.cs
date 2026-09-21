using System;

namespace Traits;

public class Empowered : Status
{
	public float modificationPercent { get; private set; }

	public override Type serializedData => typeof(SaveDataEmpowered);

	public Empowered()
	{
		name = "Empowered";
		description = "Increases damage.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.POSITIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(12);
	}

	public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
		SaveDataEmpowered saveDataEmpowered = p_saveDataTrait as SaveDataEmpowered;
		modificationPercent = saveDataEmpowered.modificationPercent;
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			modificationPercent = PlayerSkillManager.Instance.GetAdditionalAttackPercentagePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.EMPOWER);
			character.combatComponent.AdjustAttackPercentModifier(modificationPercent);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character.combatComponent.AdjustAttackPercentModifier(0f - modificationPercent);
		}
	}
}
