namespace Traits;

public class Cleric : PowerLockerTrait
{
	public Cleric()
	{
		name = "Cleric";
		description = "An extreme Divine Worshiper.";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		canBeTriggered = false;
		AddTraitOverrideFunctionIdentifier("Death_Trait");
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character { isDead: false } character)
		{
			CharacterManager.Instance.IncreaseActiveReligiousCultist(RELIGION.Divine_Worship, character);
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			CharacterManager.Instance.IncreaseActiveReligiousCultist(RELIGION.Divine_Worship, character);
			character.behaviourComponent.AddBehaviourComponent(typeof(ClericBehaviour));
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			CharacterManager.Instance.DecreaseActiveReligiousCultist(RELIGION.Divine_Worship, character);
			character.behaviourComponent.RemoveBehaviourComponent(typeof(ClericBehaviour));
		}
	}

	public override bool OnDeath(Character character)
	{
		CharacterManager.Instance.DecreaseActiveReligiousCultist(RELIGION.Divine_Worship, character);
		character.behaviourComponent.RemoveBehaviourComponent(typeof(ClericBehaviour));
		return base.OnDeath(character);
	}
}
