namespace Goap.Job_Checkers;

public class CanTakeChangeClass : CanTakeJobChecker
{
	public override string key => "CanTakeChangeClass";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		if (character.isNormalCharacter && character.race != RACE.RATMAN && character.classComponent.canChangeClass && jobQueueItem is GoapPlanJob goapPlanJob)
		{
			OtherData[] otherDataSpecific = goapPlanJob.GetOtherDataSpecific(INTERACTION_TYPE.CHANGE_CLASS);
			string text = string.Empty;
			for (int i = 0; i < otherDataSpecific.Length; i++)
			{
				if (otherDataSpecific[i] is StringOtherData stringOtherData)
				{
					text = stringOtherData.str;
					break;
				}
			}
			if (text.IsSpecialCivilianClassName() && character.characterClass.className.IsSpecialCivilianClassName() && character.structureComponent.HasWorkPlaceStructure() && character.characterClass.workStructureType == character.structureComponent.workPlaceStructure.structureType)
			{
				return false;
			}
			if (character.classComponent.shouldChangeClass)
			{
				if (text == "Combatant")
				{
					if (character.faction != null && character.faction.ideologyComponent.HasIdeology(FACTION_IDEOLOGY.Mage_Guild))
					{
						if (character.characterClass.className == "Mage")
						{
							return false;
						}
					}
					else
					{
						if (!character.classComponent.HasAbleCombatantClass())
						{
							return false;
						}
						if (character.characterClass.IsCombatant())
						{
							return false;
						}
					}
				}
				else
				{
					if (character.characterClass.className == text)
					{
						return false;
					}
					if (character.faction != null && character.faction.ideologyComponent.HasIdeology(FACTION_IDEOLOGY.Mage_Guild) && text == "Mage")
					{
						if (character.characterClass.className == "Mage")
						{
							return false;
						}
						return true;
					}
					if (!character.classComponent.HasAbleClass(text))
					{
						return false;
					}
					if (CharacterManager.Instance.GetCharacterClass(text).IsCombatant() && character.characterClass.IsCombatant())
					{
						return false;
					}
				}
				return true;
			}
			if (character.characterClass.className == text)
			{
				return false;
			}
			if (character.faction != null && character.faction.ideologyComponent.HasIdeology(FACTION_IDEOLOGY.Mage_Guild) && text == "Mage")
			{
				if (character.characterClass.className == "Mage")
				{
					return false;
				}
				return true;
			}
			if (!character.classComponent.HasAbleClass(text))
			{
				return false;
			}
			if (character.characterClass.className.IsFoodProducerClassName())
			{
				int foodProducerClassTier = CharacterManager.Instance.GetFoodProducerClassTier(character.characterClass.className);
				if (CharacterManager.Instance.GetFoodProducerClassTier(text) > foodProducerClassTier)
				{
					return true;
				}
			}
			else if (character.characterClass.className.IsResourceProducerClassName())
			{
				int resourceProducerClassTier = CharacterManager.Instance.GetResourceProducerClassTier(character.characterClass.className);
				if (CharacterManager.Instance.GetResourceProducerClassTier(text) > resourceProducerClassTier)
				{
					return true;
				}
			}
		}
		return false;
	}
}
