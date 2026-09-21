using System.Collections.Generic;
using Traits;
using UtilityScripts;

namespace Interrupts;

public class BeingTortured : Interrupt
{
	private readonly string[] _negativeTraits = new string[9] { "Agoraphobic", "Pyrophobic", "Coward", "Hothead", "Alcoholic", "Glutton", "Suspicious", "Music Hater", "Evil" };

	private readonly string[] _negativeStatus = new string[4] { "Injured", "Traumatized", "Spooked", "Unconscious" };

	public BeingTortured()
		: base(INTERRUPT.Being_Tortured)
	{
		base.duration = 20;
		base.doesStopCurrentAction = true;
		base.doesDropCurrentJob = true;
		base.interruptIconString = GoapActionStateDB.Injured_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Life_Changes,
			LOG_TAG.Player
		};
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		interruptHolder.actor.classComponent.OnCharacterStartedBeingTortured();
		return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
	}

	public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder)
	{
		string randomValidNegativeTrait = GetRandomValidNegativeTrait(interruptHolder.actor);
		string randomValidNegativeStatus = GetRandomValidNegativeStatus(interruptHolder.actor);
		if (string.IsNullOrEmpty(randomValidNegativeTrait) || string.IsNullOrEmpty(randomValidNegativeStatus))
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " cannot_torture", base.logTags);
			log.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			interruptHolder.actor.logComponent.RegisterLog(log, releaseAfter: true);
		}
		else
		{
			int num = 1;
			bool flag = true;
			List<string> list = RuinarchListPool<string>.Claim();
			while (flag)
			{
				string text = $"{base.name} torture_text_{num}";
				if (!string.IsNullOrEmpty(LocalizationManager.Instance.GetLocalizedValue("Interrupts_Table", text)))
				{
					list.Add(text);
					num++;
					continue;
				}
				flag = false;
				break;
			}
			string randomElement = CollectionUtilities.GetRandomElement(list);
			RuinarchListPool<string>.Release(list);
			Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", randomElement, base.logTags);
			log2.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			Log log3 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " full_text", base.logTags);
			log3.AddToFillers(null, log2.unreplacedText, LOG_IDENTIFIER.APPEND);
			log3.AddToFillers(log2.fillers);
			log3.AddToFillers(null, randomValidNegativeStatus, LOG_IDENTIFIER.STRING_1);
			log3.AddToFillers(null, randomValidNegativeTrait, LOG_IDENTIFIER.STRING_2);
			interruptHolder.actor.logComponent.RegisterLog(log3, releaseAfter: true);
			interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, randomValidNegativeStatus);
			interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, randomValidNegativeTrait);
			if (randomValidNegativeTrait == "Evil")
			{
				PlayerManager.Instance.player.goalComponent.CompleteSubGoal(SUB_GOAL.GOAL_MAKE_VILLAGER_EVIL);
			}
			interruptHolder.actor.AddAfflictionByPlayer(randomValidNegativeStatus);
			interruptHolder.actor.AddAfflictionByPlayer(randomValidNegativeTrait);
		}
		return base.ExecuteInterruptEndEffect(interruptHolder);
	}

	private string GetRandomValidNegativeTrait(Character target)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < _negativeTraits.Length; i++)
		{
			string text = _negativeTraits[i];
			if (TraitValidator.CanAddTrait(target, text, target.traitContainer))
			{
				list.Add(text);
			}
		}
		if (list.Count > 0)
		{
			return CollectionUtilities.GetRandomElement(list);
		}
		return string.Empty;
	}

	private string GetRandomValidNegativeStatus(Character target)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < _negativeStatus.Length; i++)
		{
			string text = _negativeStatus[i];
			if (TraitValidator.CanAddTrait(target, text, target.traitContainer))
			{
				list.Add(text);
			}
		}
		if (list.Count > 0)
		{
			return CollectionUtilities.GetRandomElement(list);
		}
		return string.Empty;
	}
}
