namespace Traits;

public class BlitzBuff : Status
{
	public BlitzBuff()
	{
		name = "Blitz Buff";
		description = "Attack speed is doubled";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.POSITIVE;
		isStacking = false;
		ticksDuration = GameManager.Instance.GetTicksBasedOnMinutes(30);
		isHidden = true;
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			character.combatComponent.AdjustAttackSpeedPercentModifier(-50f);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character.combatComponent.AdjustAttackSpeedPercentModifier(50f);
		}
	}
}
