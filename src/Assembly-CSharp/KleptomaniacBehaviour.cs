using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class KleptomaniacBehaviour : CharacterBehaviour
{
	public KleptomaniacBehaviour()
	{
		base.priority = 20;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.homeSettlement == null)
		{
			producedJob = null;
			return false;
		}
		PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = PLAYER_SKILL_TYPE.KLEPTOMANIA;
		if (character.afflictionsSkillsInflictedByPlayer.Contains(pLAYER_SKILL_TYPE) && !character.IsInventoryAtFullCapacity())
		{
			PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(pLAYER_SKILL_TYPE);
			SkillData skillData = PlayerSkillManager.Instance.GetSkillData(pLAYER_SKILL_TYPE);
			bool flag = skillData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Rob_From_House);
			bool flag2 = skillData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Rob_Any_Place);
			if ((flag || flag2) && ChanceData.RollChance(flag2 ? CHANCE_TYPE.Kleptomania_Rob_Any_Place : CHANCE_TYPE.Kleptomania_Rob_Other_House))
			{
				List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
				if (flag2)
				{
					list.AddRange(character.homeSettlement.allStructures);
					if (character.homeStructure != null)
					{
						list.Remove(character.homeStructure);
					}
				}
				else
				{
					List<LocationStructure> structuresOfType = character.homeSettlement.GetStructuresOfType(STRUCTURE_TYPE.DWELLING);
					if (structuresOfType != null)
					{
						list.AddRange(structuresOfType);
					}
					if (character.homeStructure != null)
					{
						list.Remove(character.homeStructure);
					}
				}
				if (list.Count > 0)
				{
					LocationStructure randomElement = CollectionUtilities.GetRandomElement(list);
					return character.jobComponent.TriggerRobLocation(randomElement, INTERACTION_TYPE.STEAL_ANYTHING, out producedJob);
				}
				RuinarchListPool<LocationStructure>.Release(list);
			}
		}
		producedJob = null;
		return false;
	}
}
