namespace Traits;

public class Heartbroken : Status
{
	public Character owner { get; private set; }

	public Heartbroken()
	{
		name = "Heartbroken";
		description = "Experiencing a strong amount of emotional stress.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(48);
		moodEffect = -8;
		isStacking = true;
		stackLimit = 5;
		stackModifier = 0.25f;
		hindersSocials = true;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character)
		{
			owner = addTo as Character;
		}
	}

	public override void OnAddTrait(ITraitable sourcePOI)
	{
		if (sourcePOI is Character)
		{
			owner = sourcePOI as Character;
		}
		base.OnAddTrait(sourcePOI);
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		owner = null;
	}

	public bool TriggerBrokenhearted()
	{
		return owner.interruptComponent.TriggerInterrupt(INTERRUPT.Feeling_Brokenhearted, owner);
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
	}
}
