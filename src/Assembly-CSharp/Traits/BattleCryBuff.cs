namespace Traits;

public class BattleCryBuff : Status
{
	public BattleCryBuff()
	{
		name = "Battle Cry Buff";
		description = "Damage is increased by 20%";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.POSITIVE;
		isStacking = false;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(1);
		isHidden = true;
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			character.combatComponent.AdjustAttackPercentModifier(20f);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character.combatComponent.AdjustAttackPercentModifier(-20f);
		}
	}
}
