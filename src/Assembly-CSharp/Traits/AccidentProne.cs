using UtilityScripts;

namespace Traits;

public class AccidentProne : Trait
{
	public Character owner { get; private set; }

	public AccidentProne()
	{
		name = "Accident Prone";
		description = "A walking and talking disaster.";
		type = TRAIT_TYPE.FLAW;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		canBeTriggered = true;
		AddTraitOverrideFunctionIdentifier("Start_Perform_Trait");
		AddTraitOverrideFunctionIdentifier("Per_Tick_While_Stationary_Unoccupied");
	}

	public override void OnAddTrait(ITraitable sourceCharacter)
	{
		base.OnAddTrait(sourceCharacter);
		if (sourceCharacter is Character)
		{
			owner = sourceCharacter as Character;
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		owner = null;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character character)
		{
			owner = character;
		}
	}

	public override bool PerTickWhileStationaryOrUnoccupied(Character p_character)
	{
		if (owner.hasMarker && owner.marker.isMoving)
		{
			if (GameUtilities.RollChance(0.5f))
			{
				return owner.interruptComponent.TriggerInterrupt(INTERRUPT.Stumble, owner);
			}
			if (GameUtilities.RollChance(0.5f))
			{
				return owner.interruptComponent.TriggerInterrupt(INTERRUPT.Stumble_Knockout, owner);
			}
		}
		return false;
	}

	public override bool OnStartPerformGoapAction(ActualGoapNode node, ref bool willStillContinueAction)
	{
		if (node.goapType == INTERACTION_TYPE.STAND || node.goapType == INTERACTION_TYPE.STAND_STILL || node.goapType == INTERACTION_TYPE.LONG_STAND_STILL)
		{
			return false;
		}
		if (GameUtilities.RollChance(5))
		{
			willStillContinueAction = false;
			return node.actor.interruptComponent.TriggerInterrupt(INTERRUPT.Accident, node.actor, "", null, node.action.localizedName);
		}
		return false;
	}

	public override string TriggerFlaw(Character character, bool isTriggeredByPlayer = true)
	{
		if (character.marker.isMoving)
		{
			owner.interruptComponent.TriggerInterrupt(INTERRUPT.Stumble, owner);
		}
		else if (character.currentActionNode != null)
		{
			owner.interruptComponent.TriggerInterrupt(INTERRUPT.Accident, owner, "", null, character.currentActionNode.action.localizedName);
		}
		return base.TriggerFlaw(character);
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
	}
}
