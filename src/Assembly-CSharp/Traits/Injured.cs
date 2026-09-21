using System.Collections.Generic;

namespace Traits;

public class Injured : Status
{
	public override bool isSingleton => true;

	public Injured()
	{
		name = "Injured";
		description = "Sustained a physical trauma.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(24);
		advertisedInteractions = new List<INTERACTION_TYPE>
		{
			INTERACTION_TYPE.FIRST_AID_CHARACTER,
			INTERACTION_TYPE.HEALER_CURE
		};
		moodEffect = -4;
		isStacking = true;
		stackLimit = 5;
		stackModifier = 0.5f;
	}

	public override void OnAddTrait(ITraitable traitable)
	{
		base.OnAddTrait(traitable);
		if (traitable is Character character)
		{
			character.UpdateCanCombatState();
			character.movementComponent.AdjustSpeedModifier(-0.15f);
			if (base.gainedFromDoingType != INTERACTION_TYPE.ASSAULT)
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "add_trait", LOG_TAG.Life_Changes);
				log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(null, base.localizedName, LOG_IDENTIFIER.TARGET_CHARACTER);
				log.AddLogToDatabase(releaseLogAfter: true);
			}
		}
	}

	public override void OnRemoveTrait(ITraitable traitable, Character removedBy)
	{
		if (traitable is Character character)
		{
			character.UpdateCanCombatState();
			character.movementComponent.AdjustSpeedModifier(0.15f);
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "remove_trait", LOG_TAG.Life_Changes);
			log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(null, base.localizedName, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddLogToDatabase(releaseLogAfter: true);
		}
		base.OnRemoveTrait(traitable, removedBy);
	}
}
