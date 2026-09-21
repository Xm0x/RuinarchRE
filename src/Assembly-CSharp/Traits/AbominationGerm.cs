namespace Traits;

public class AbominationGerm : Status
{
	private IPointOfInterest _owner;

	public AbominationGerm()
	{
		name = "Abomination Germ";
		description = "There is a mysterious germ growing inside...";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(8);
		moodEffect = -5;
		isStacking = true;
		stackLimit = 1;
		stackModifier = 1f;
		AddTraitOverrideFunctionIdentifier("Execute_Pre_Effect_Trait");
		AddTraitOverrideFunctionIdentifier("Execute_Pre_Effect_Trait");
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is IPointOfInterest pointOfInterest)
		{
			_owner = pointOfInterest;
			if (pointOfInterest is TileObject)
			{
				ticksDuration = 0;
			}
		}
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is IPointOfInterest owner)
		{
			_owner = owner;
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		_owner = null;
	}

	public override void OnRemoveStatusBySchedule(ITraitable removedFrom)
	{
		base.OnRemoveStatusBySchedule(removedFrom);
		if (removedFrom is Character { isNormalCharacter: not false, isDead: false } character)
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Abomination_Death, character);
		}
	}

	public override void ExecuteActionPreEffects(INTERACTION_TYPE action, ActualGoapNode goapNode)
	{
		base.ExecuteActionPreEffects(action, goapNode);
		if (goapNode.action.actionCategory == ACTION_CATEGORY.CONSUME && goapNode.target == _owner)
		{
			TransferAbominationGerm(goapNode.actor, goapNode.target);
		}
	}

	public override void ExecuteActionAfterEffects(INTERACTION_TYPE action, Character actor, IPointOfInterest target, ACTION_CATEGORY category, ref bool isRemoved)
	{
		base.ExecuteActionAfterEffects(action, actor, target, category, ref isRemoved);
		if (category == ACTION_CATEGORY.CONSUME && actor.race == RACE.RAT)
		{
			CharacterManager.Instance.GenerateRatman(actor.gridTileLocation, actor.homeStructure, actor.name);
			actor.SetDestroyMarkerOnDeath(state: true);
			actor.Death();
		}
	}

	public void TransferAbominationGerm(Character p_actor, IPointOfInterest p_target)
	{
		p_actor.traitContainer.AddTrait(p_actor, "Abomination Germ");
		p_target.traitContainer.RemoveTrait(p_target, "Abomination Germ");
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = _owner;
	}
}
