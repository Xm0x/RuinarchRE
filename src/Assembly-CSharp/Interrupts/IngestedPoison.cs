using System.Collections.Generic;
using Traits;
using UtilityScripts;

namespace Interrupts;

public class IngestedPoison : Interrupt
{
	public IngestedPoison()
		: base(INTERRUPT.Ingested_Poison)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.Sick_Icon;
		base.isIntel = true;
		base.logTags = new LOG_TAG[1];
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		if (GameUtilities.RollChance(20))
		{
			interruptHolder.actor.Death("poisoned");
		}
		else
		{
			int stacks = interruptHolder.target.traitContainer.GetStacks("Poisoned");
			if (stacks > 0)
			{
				for (int i = 0; i < stacks; i++)
				{
					interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Poisoned", null, bypassElementalChance: true, -1, 0f, ELEMENTAL_TYPE.Poison);
				}
				Poisoned traitOrStatus = interruptHolder.actor.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned");
				if (traitOrStatus != null)
				{
					Poisoned traitOrStatus2 = interruptHolder.target.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned");
					if (traitOrStatus2.responsibleCharacters != null)
					{
						for (int j = 0; j < traitOrStatus2.responsibleCharacters.Count; j++)
						{
							traitOrStatus.AddCharacterResponsibleForTrait(traitOrStatus2.responsibleCharacters[j]);
						}
					}
					traitOrStatus.SetIsPlayerSource(traitOrStatus2.isPlayerSource);
					overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " sick", base.logTags);
					overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				}
			}
		}
		return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
	}

	public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
		base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);
		reactions.Add(EMOTION.Shock);
		if (!witness.relationshipContainer.IsEnemiesWith(actor))
		{
			reactions.Add(EMOTION.Concern);
		}
	}
}
