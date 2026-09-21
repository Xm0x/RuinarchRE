using UnityEngine;

namespace Traits;

public class Drunk : Status
{
	public override bool isSingleton => true;

	public Drunk()
	{
		name = "Drunk";
		description = "Inebriated. Moves slowly. May be volatile towards its enemies.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(8);
		moodEffect = 4;
		isStacking = true;
		stackLimit = 5;
		stackModifier = 0.5f;
		AddTraitOverrideFunctionIdentifier("See_Poi_Trait");
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		(addedTo as Character).movementComponent.AdjustSpeedModifier(-0.4f);
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		(removedFrom as Character).movementComponent.AdjustSpeedModifier(0.4f);
		base.OnRemoveTrait(removedFrom, removedBy);
	}

	public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob)
	{
		if (targetPOI is Character { isDead: false } character && characterThatWillDoJob.relationshipContainer.IsEnemiesWith(character))
		{
			int num = 0;
			if (characterThatWillDoJob.relationshipContainer.HasOpinionLabelWithCharacter(character, "Enemy"))
			{
				num = 10;
			}
			else if (characterThatWillDoJob.relationshipContainer.HasOpinionLabelWithCharacter(character, "Rival"))
			{
				num = 25;
			}
			else
			{
				Debug.LogWarning("There is no drunk combat chance case for character " + characterThatWillDoJob.name + "!");
			}
			if (Random.Range(0, 100) < num)
			{
				if (!character.traitContainer.HasTrait("Unconscious") && characterThatWillDoJob.combatComponent.Fight(character, "Drunk", null, isLethal: false))
				{
					Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "drunk_assault", LOG_TAG.Combat);
					log.AddToFillers(characterThatWillDoJob, characterThatWillDoJob.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
					log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
					characterThatWillDoJob.logComponent.RegisterLog(log, releaseAfter: true);
				}
				return true;
			}
		}
		return base.OnSeePOI(targetPOI, characterThatWillDoJob);
	}
}
