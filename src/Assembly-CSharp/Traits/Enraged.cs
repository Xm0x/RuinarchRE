namespace Traits;

public class Enraged : Status
{
	public Enraged()
	{
		name = "Enraged";
		description = "Increased Piercing and Crit Rate.";
		thoughtText = "These pitiful attacks will only strengthen my resolve!";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(1);
		moodEffect = -2;
		isStacking = true;
		stackLimit = 5;
		stackModifier = 1f;
		AddTraitOverrideFunctionIdentifier("Death_Trait");
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			character.piercingAndResistancesComponent.AdjustBasePiercing(5f);
			character.combatComponent.AdjustCritRate(1);
		}
	}

	public override void OnStackStatus(ITraitable addedTo)
	{
		base.OnStackStatus(addedTo);
		if (addedTo is Character)
		{
			Character obj = addedTo as Character;
			obj.piercingAndResistancesComponent.AdjustBasePiercing(5f);
			obj.combatComponent.AdjustCritRate(1);
		}
	}

	public override void OnUnstackStatus(ITraitable addedTo, bool bySchedule)
	{
		base.OnUnstackStatus(addedTo, bySchedule);
		if (addedTo is Character)
		{
			Character obj = addedTo as Character;
			obj.piercingAndResistancesComponent.AdjustBasePiercing(-5f);
			obj.combatComponent.AdjustCritRate(-1);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character.piercingAndResistancesComponent.AdjustBasePiercing(-5f);
			character.combatComponent.AdjustCritRate(-1);
		}
	}

	public override bool OnDeath(Character character)
	{
		return character.traitContainer.RemoveTrait(character, this);
	}
}
