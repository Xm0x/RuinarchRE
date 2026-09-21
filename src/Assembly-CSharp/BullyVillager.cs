using System;
using Traits;

public class BullyVillager : GoalTask
{
	private const int NeededNegativeStatus = 10;

	public override Type serializedData => typeof(SaveDataBullyVillager);

	public BullyVillager()
	{
		base.taskName = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Bully_Villager_Task");
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Bully_Villager_Task_Tooltip");
	}

	public BullyVillager(SaveDataBullyVillager p_data)
		: base(p_data)
	{
	}

	public override void StartTask()
	{
		Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
	}

	protected override void EndTask()
	{
		Messenger.RemoveListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
	}

	protected override void ReevaluateLocalizedTexts()
	{
		base.taskName = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Bully_Villager_Task");
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Bully_Villager_Task_Tooltip");
	}

	private void OnCharacterGainedTrait(Character p_character, Trait p_trait)
	{
		if (!(p_trait is Status) || p_character.isDead || !p_character.isNormalCharacter || !p_character.race.IsSapient() || p_trait.effect != TRAIT_EFFECT.NEGATIVE)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < p_character.traitContainer.statuses.Count; i++)
		{
			if (p_character.traitContainer.statuses[i].effect == TRAIT_EFFECT.NEGATIVE)
			{
				num++;
				if (num >= 10)
				{
					CompleteTask();
					break;
				}
			}
		}
	}
}
