using Traits;
using UnityEngine;

namespace Goap.Job_Checkers;

public class CanTakeRemoveStatus : CanTakeJobChecker
{
	public override string key => "CanTakeRemoveStatus";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		GoapPlanJob obj = jobQueueItem as GoapPlanJob;
		Character character2 = obj.targetPOI as Character;
		string conditionKey = obj.goal.conditionKey;
		Trait traitOrStatus = character2.traitContainer.GetTraitOrStatus<Trait>(conditionKey);
		if (traitOrStatus == null)
		{
			Debug.LogWarning(character2.name + " has remove status " + conditionKey + " in settlement job queue but does not have that trait!");
			return false;
		}
		if (character.relationshipContainer.HasGrudgeAgainst(character2))
		{
			return false;
		}
		if (character.isAlliedWithPlayer && character2.limiterComponent.isTargetedByDemonicSnatch)
		{
			return false;
		}
		if (character.crimeComponent.IsWantedBy(character2.faction))
		{
			return false;
		}
		bool flag = !character.IsHostileWith(character2) && !character2.isDead;
		bool flag2 = traitOrStatus.IsResponsibleForTrait(character);
		if (traitOrStatus.name == "Poisoned")
		{
			if (flag && !character.relationshipContainer.HasOpinionLabelWithCharacter(character2, "Rival", "Enemy") && !flag2 && !character.traitContainer.HasTrait("Psychopath"))
			{
				return character.traitContainer.HasTrait("Poison Expert");
			}
			return false;
		}
		if (flag && !character.relationshipContainer.HasOpinionLabelWithCharacter(character2, "Rival", "Enemy") && !flag2)
		{
			return !character.traitContainer.HasTrait("Psychopath");
		}
		return false;
	}
}
