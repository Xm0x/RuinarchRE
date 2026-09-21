using System.Collections.Generic;

namespace Traits;

public class Dead : Status
{
	public Dead()
	{
		name = "Dead";
		description = "Simply no longer alive.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		advertisedInteractions = new List<INTERACTION_TYPE>
		{
			INTERACTION_TYPE.RAISE_CORPSE,
			INTERACTION_TYPE.MUMMIFY
		};
		hindersMovement = true;
		hindersWitness = true;
		hindersAttackTarget = true;
		hindersPerform = true;
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (!(addedTo is Character character))
		{
			return;
		}
		character.jobComponent.TriggerBuryMe();
		if (base.responsibleCharacter != null)
		{
			base.responsibleCharacter.combatComponent.AdjustNumOfKilledCharacters(1);
		}
		else if (base.responsibleCharacters != null)
		{
			for (int i = 0; i < base.responsibleCharacters.Count; i++)
			{
				base.responsibleCharacters[i].combatComponent.AdjustNumOfKilledCharacters(1);
			}
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character)
		{
			(removedFrom as Character).ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.BURY);
		}
	}
}
