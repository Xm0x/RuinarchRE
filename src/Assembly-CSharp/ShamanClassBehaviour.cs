using System.Collections.Generic;
using UtilityScripts;

public class ShamanClassBehaviour : CharacterClassBehaviour
{
	public override bool TryDoBehaviour(Character p_character, ref JobQueueItem p_producedJob, ref string log)
	{
		if (p_character.dailyScheduleComponent.schedule.GetScheduleType(GameManager.Instance.currentTick) != DAILY_SCHEDULE.Work && ChanceData.RollChance(CHANCE_TYPE.Shaman_Ritual, ref log))
		{
			string randomRitual = GetRandomRitual(p_character);
			if (!string.IsNullOrEmpty(randomRitual))
			{
				INTERACTION_TYPE p_actionType = INTERACTION_TYPE.RECONCILIATION_RITUAL;
				switch (randomRitual)
				{
				case "Reconciliation":
					p_actionType = INTERACTION_TYPE.RECONCILIATION_RITUAL;
					break;
				case "Guardians":
					p_actionType = INTERACTION_TYPE.GUARDIAN_RITUAL;
					break;
				case "Gods":
					p_actionType = INTERACTION_TYPE.GOD_DAY_RITUAL;
					break;
				}
				if (p_character.jobComponent.TryCreateShamanRitualJob(p_actionType, out p_producedJob) && p_producedJob != null)
				{
					return true;
				}
			}
		}
		if (MagicUserBehaviour(p_character, ref log, ref p_producedJob))
		{
			return true;
		}
		return false;
	}

	private string GetRandomRitual(Character p_character)
	{
		List<string> list = RuinarchListPool<string>.Claim();
		string result = string.Empty;
		int num = p_character.TryGetTalentLevel(CHARACTER_TALENT.Combat_Magic);
		if (num == 5)
		{
			list.Add("Gods");
		}
		if (num >= 3)
		{
			list.Add("Guardians");
		}
		if (num >= 1 && p_character.homeSettlement != null && ReconciliationRitual.HasTwoCharactersThatAreEnemiesOrRivalInHomeSettlementForReconciliation(p_character.homeSettlement, p_character))
		{
			list.Add("Reconciliation");
		}
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<string>.Release(list);
		return result;
	}
}
