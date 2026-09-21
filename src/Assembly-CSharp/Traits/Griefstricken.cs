namespace Traits;

public class Griefstricken : Status
{
	public Character owner { get; private set; }

	public Griefstricken()
	{
		name = "Griefstricken";
		description = "Lost a loved one.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(72);
		moodEffect = -20;
		isStacking = true;
		stackLimit = 5;
		stackModifier = 0.25f;
		hindersSocials = true;
		AddTraitOverrideFunctionIdentifier("See_Poi_Trait");
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character character)
		{
			owner = character;
		}
	}

	public override void OnAddTrait(ITraitable sourcePOI)
	{
		if (sourcePOI is Character character)
		{
			owner = character;
			_ = base.responsibleCharacter;
		}
		base.OnAddTrait(sourcePOI);
	}

	public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob)
	{
		if (targetPOI is Character { isDead: false } character && IsResponsibleForTrait(character))
		{
			characterThatWillDoJob.traitContainer.RemoveTrait(characterThatWillDoJob, this);
			return true;
		}
		return base.OnSeePOI(targetPOI, characterThatWillDoJob);
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		owner = null;
	}

	public bool TriggerGrieving()
	{
		return owner.interruptComponent.TriggerInterrupt(INTERRUPT.Grieving, owner);
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
	}
}
