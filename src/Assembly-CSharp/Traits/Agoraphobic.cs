using UtilityScripts;

namespace Traits;

public class Agoraphobic : Trait
{
	public override bool isSingleton => true;

	public Agoraphobic()
	{
		name = "Agoraphobic";
		description = "Crowds? Oh no! If afflicted by the player, will produce a Chaos Orb each time it sees a crowd.";
		type = TRAIT_TYPE.FLAW;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		canBeTriggered = true;
		AddTraitOverrideFunctionIdentifier("See_Poi_Trait");
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character character)
		{
			character.traitComponent.SubscribeToAgoraphobiaLevelUpSignal();
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			ApplyLeavePartyEffect(character);
			ApplyAgoraphobicEffect(character);
			character.traitComponent.SubscribeToAgoraphobiaLevelUpSignal();
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character.traitComponent.UnsubscribeToAgoraphobiaLevelUpSignal();
		}
	}

	public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob)
	{
		if (targetPOI.poiType == POINT_OF_INTEREST_TYPE.CHARACTER)
		{
			if (characterThatWillDoJob.traitComponent.hasAgoraphobicReactedThisTick)
			{
				return false;
			}
			if (ApplyAgoraphobicEffect(characterThatWillDoJob))
			{
				characterThatWillDoJob.traitComponent.SetHasAgoraphobicReactedThisTick(p_state: true);
			}
			return true;
		}
		return base.OnSeePOI(targetPOI, characterThatWillDoJob);
	}

	public override string TriggerFlaw(Character character, bool isTriggeredByPlayer = true)
	{
		if (ApplyAgoraphobicEffect(character, isTriggeredByPlayer))
		{
			return base.TriggerFlaw(character);
		}
		return "fail_default";
	}

	private void ApplyLeavePartyEffect(Character character)
	{
		if (character.HasAfflictedByPlayerWith(name) && PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.AGORAPHOBIA).HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.No_Longer_Join_Parties) && character.partyComponent.hasParty)
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Left_Party, character, "", null, "Agoraphobic_Reason");
		}
	}

	private bool ApplyAgoraphobicEffect(Character character, bool isTriggeredByPlayer = false)
	{
		if (!character.limiterComponent.canWitness)
		{
			return false;
		}
		if (!isTriggeredByPlayer && character.traitContainer.HasTrait("Anxious"))
		{
			int stacks = character.traitContainer.GetStacks("Anxious");
			Status traitOrStatus = character.traitContainer.GetTraitOrStatus<Status>("Anxious");
			if (stacks >= traitOrStatus.stackLimit)
			{
				return false;
			}
		}
		if (!WillTriggerAgoraphobia(character))
		{
			return false;
		}
		character.jobQueue.CancelAllJobs();
		bool flag = true;
		if (character.HasAfflictedByPlayerWith(name))
		{
			AfflictData afflictionData = PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.AGORAPHOBIA);
			flag = afflictionData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Make_Anxious);
			if (afflictionData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.No_Longer_Join_Parties) && character.partyComponent.hasParty)
			{
				character.interruptComponent.TriggerInterrupt(INTERRUPT.Left_Party, character, "", null, "Agoraphobic_Reason");
			}
			DispenseChaosOrbsForAffliction(character, PLAYER_SKILL_TYPE.AGORAPHOBIA, 1);
		}
		if (flag)
		{
			character.traitContainer.AddTrait(character, "Anxious");
		}
		if (GameUtilities.RollChance(10))
		{
			character.traitContainer.AddTrait(character, "Catatonic");
		}
		else if (GameUtilities.RollChance(15))
		{
			character.traitContainer.AddTrait(character, "Berserked");
		}
		else if (GameUtilities.RollChance(15))
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Seizure, character);
		}
		else if (GameUtilities.RollChance(10) && (character.characterClass.className == "Druid" || character.characterClass.className == "Shaman" || character.characterClass.className == "Mage"))
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Loss_Of_Control, character);
		}
		else
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character, "", null, "Agoraphobic_Reason");
		}
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Trait", "Traits_Table", "Agoraphobic on_see_first", LOG_TAG.Social);
		log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddLogToDatabase(releaseLogAfter: true);
		return true;
	}

	private bool WillTriggerAgoraphobia(Character character)
	{
		int num = 3;
		if (character.HasAfflictedByPlayerWith(PLAYER_SKILL_TYPE.AGORAPHOBIA))
		{
			num = PlayerSkillManager.Instance.GetAfflictionCrowdNumberPerLevel(PLAYER_SKILL_TYPE.AGORAPHOBIA);
		}
		int num2 = 0;
		bool flag = character.traitContainer.HasTrait("Necromancer");
		if (character.marker.inVisionCharacters.Count >= num)
		{
			for (int i = 0; i < character.marker.inVisionCharacters.Count; i++)
			{
				Character character2 = character.marker.inVisionCharacters[i];
				if (!character2.isDead && character2.petComponent.petOwner != character && (!flag || character2.race != RACE.SKELETON))
				{
					num2++;
				}
			}
		}
		return num2 >= num;
	}
}
