using Object_Pools;
using UnityEngine.Localization.Settings;
using UtilityScripts;

namespace Interrupts;

public class HunterSpecialization : Interrupt
{
	public HunterSpecialization()
		: base(INTERRUPT.Hunter_Specialization)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.No_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Combat };
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Character actor = interruptHolder.actor;
		if (interruptHolder.target is Character character)
		{
			CHARACTER_CATEGORY category = RaceManager.Instance.GetRaceData(character.race).category;
			actor.classComponent.SetHunterKillingSpecialization(category);
			if (overrideEffectLog != null)
			{
				LogPool.Release(overrideEffectLog);
			}
			overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " specialized", LOG_TAG.Combat);
			overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			if (LocalizationSettings.SelectedLocale.LocaleName.Equals("Spanish (Latin America) (es)") || LocalizationSettings.SelectedLocale.LocaleName.Equals("English (en)"))
			{
				string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", category.ToStringEnum());
				overrideEffectLog.AddToFillers(null, Utilities.ForcePluralizeString(localizedValue), LOG_IDENTIFIER.STRING_1);
			}
			else
			{
				string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", category.ToStringEnum() + "_Plural");
				if (string.IsNullOrEmpty(localizedValue2))
				{
					localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", category.ToStringEnum());
				}
				overrideEffectLog.AddToFillers(null, localizedValue2, LOG_IDENTIFIER.STRING_1);
			}
		}
		return true;
	}
}
