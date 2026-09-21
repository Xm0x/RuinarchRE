using System.Collections.Generic;
using UtilityScripts;

namespace Traits;

public class Hothead : Trait
{
	public override bool isSingleton => true;

	public Hothead()
	{
		name = "Hothead";
		description = "Quick to anger. May flare up when seeing an enemy. If afflicted by the player, will produce a Chaos Orb each time it gets Angry.";
		type = TRAIT_TYPE.FLAW;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		AddTraitOverrideFunctionIdentifier("See_Poi_Trait");
		canBeTriggered = true;
	}

	public override string TriggerFlaw(Character character, bool isTriggeredByPlayer = true)
	{
		character.traitContainer.AddTrait(character, "Angry");
		return base.TriggerFlaw(character);
	}

	public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob)
	{
		if (targetPOI is Character character)
		{
			string log = string.Empty;
			if (GameUtilities.RollChance(PlayerSkillManager.Instance.GetTriggerRateForCurrentLevel(PLAYER_SKILL_TYPE.HOTHEADED), ref log))
			{
				bool flag = false;
				List<OPINIONS> list = RuinarchListPool<OPINIONS>.Claim();
				PlayerSkillManager.Instance.PopulateOpinionTriggersAtCurrentLevel(PLAYER_SKILL_TYPE.HOTHEADED, list);
				if ((!list.Contains(OPINIONS.NoOne) || list.Count != 1) && (list.Contains(OPINIONS.Everyone) || characterThatWillDoJob.relationshipContainer.HasOpinionLabelWithCharacter(character, list)))
				{
					characterThatWillDoJob.interruptComponent.TriggerInterrupt(INTERRUPT.Angered, character);
					return true;
				}
				RuinarchListPool<OPINIONS>.Release(list);
			}
		}
		return base.OnSeePOI(targetPOI, characterThatWillDoJob);
	}
}
