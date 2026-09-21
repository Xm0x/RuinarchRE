namespace Traits;

public class Glutton : Trait
{
	private float additionalFullnessDecreaseRate;

	private Character m_owner;

	public Glutton()
	{
		name = "Glutton";
		description = "Eats a lot! If afflicted by the player, will produce a Chaos Orb each time it eats.";
		type = TRAIT_TYPE.FLAW;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		canBeTriggered = true;
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			m_owner = character;
			if (character.HasAfflictedByPlayerWith(this))
			{
				character.traitComponent.SubscribeToGluttonLevelUpSignal();
				SubscribeToAfflictionSignals();
			}
			additionalFullnessDecreaseRate = GetHungerDecreaseRate(character);
			character.needsComponent.AdjustFullnessDecreaseRate(additionalFullnessDecreaseRate);
			character.behaviourComponent.AddBehaviourComponent(typeof(GluttonBehaviour));
		}
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character character)
		{
			m_owner = character;
			if (character.HasAfflictedByPlayerWith(this))
			{
				character.traitComponent.SubscribeToGluttonLevelUpSignal();
				SubscribeToAfflictionSignals();
			}
			additionalFullnessDecreaseRate = GetHungerDecreaseRate(character);
		}
	}

	public void OnGluttonLeveledUp()
	{
		m_owner.needsComponent.AdjustFullnessDecreaseRate(0f - additionalFullnessDecreaseRate);
		additionalFullnessDecreaseRate = GetHungerDecreaseRate(m_owner);
		m_owner.needsComponent.AdjustFullnessDecreaseRate(additionalFullnessDecreaseRate);
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			UnsubscribeToAfflictionSignals();
			character.traitComponent.UnsubscribeToGluttonLevelUpSignal();
			character.needsComponent.AdjustFullnessDecreaseRate(0f - additionalFullnessDecreaseRate);
			character.behaviourComponent.RemoveBehaviourComponent(typeof(GluttonBehaviour));
			m_owner = null;
		}
	}

	public override string TriggerFlaw(Character character, bool isTriggeredByPlayer = true)
	{
		if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW))
		{
			if (!character.traitContainer.HasTrait("Burning"))
			{
				if (!character.needsComponent.TriggerFlawFullnessRecovery(character, isTriggeredByPlayer))
				{
					return "cannot_perform";
				}
				return base.TriggerFlaw(character);
			}
			return "burning";
		}
		return "has_trigger_flaw";
	}

	private float GetHungerDecreaseRate(Character p_character)
	{
		float num = (p_character.HasAfflictedByPlayerWith(this) ? PlayerSkillManager.Instance.GetAfflictionHungerRatePerLevel(PLAYER_SKILL_TYPE.GLUTTONY) : PlayerSkillManager.Instance.GetAfflictionHungerRatePerLevel(PLAYER_SKILL_TYPE.GLUTTONY, 0));
		num /= 100f;
		return EditableValuesManager.Instance.baseFullnessDecreaseRate * num;
	}

	private void SubscribeToAfflictionSignals()
	{
		Messenger.AddListener<ActualGoapNode>(JobSignals.STARTED_PERFORMING_ACTION, OnActionPerformed);
	}

	private void UnsubscribeToAfflictionSignals()
	{
		Messenger.RemoveListener<ActualGoapNode>(JobSignals.STARTED_PERFORMING_ACTION, OnActionPerformed);
	}

	private void OnActionPerformed(ActualGoapNode p_action)
	{
		if (p_action.action.actionCategory == ACTION_CATEGORY.CONSUME && p_action.actor == m_owner)
		{
			DispenseChaosOrbsForAffliction(m_owner, PLAYER_SKILL_TYPE.GLUTTONY, 1);
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = m_owner;
	}
}
