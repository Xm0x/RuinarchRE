using System.Collections.Generic;
using Character_Talents;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class CharacterTalentComponent : CharacterComponent
{
	private Dictionary<CHARACTER_TALENT, CharacterTalent> _talentDictionary;

	public List<CharacterTalent> allTalents { get; private set; }

	public CharacterTalentComponent()
	{
	}

	public CharacterTalentComponent(SaveDataCharacterTalentComponent data)
	{
		LoadAllTalents(data);
	}

	public void ConstructAllTalents()
	{
		allTalents = new List<CharacterTalent>();
		_talentDictionary = new Dictionary<CHARACTER_TALENT, CharacterTalent>();
		for (int i = 0; i < CharacterManager.Instance.talentManager.allTalentEnums.Length; i++)
		{
			CHARACTER_TALENT cHARACTER_TALENT = CharacterManager.Instance.talentManager.allTalentEnums[i];
			CharacterTalent characterTalent = new CharacterTalent();
			characterTalent.SetTalentType(cHARACTER_TALENT);
			characterTalent.SetExperience(0);
			characterTalent.SetLevel(1, base.owner);
			allTalents.Add(characterTalent);
			_talentDictionary.Add(cHARACTER_TALENT, characterTalent);
		}
	}

	public void RandomizeInitialTalents(Character p_character)
	{
		List<CHARACTER_TALENT> list = RuinarchListPool<CHARACTER_TALENT>.Claim();
		list.AddRange(CharacterManager.Instance.talentManager.allTalentEnums);
		CHARACTER_TALENT randomElement = CollectionUtilities.GetRandomElement(list);
		CharacterTalent talent = GetTalent(randomElement);
		for (int i = 0; i < 2; i++)
		{
			if (GameManager.Instance.gameHasStarted)
			{
				talent.LevelUp(p_character);
			}
			else
			{
				talent.LevelUpForInitialVillagersInWorldGen(p_character);
			}
		}
		list.Remove(randomElement);
		randomElement = CollectionUtilities.GetRandomElement(list);
		talent = GetTalent(randomElement);
		if (GameManager.Instance.gameHasStarted)
		{
			talent.LevelUp(p_character);
		}
		else
		{
			talent.LevelUpForInitialVillagersInWorldGen(p_character);
		}
		if (!p_character.race.IsSapient())
		{
			return;
		}
		int bonusTalentPoints = PortalBonusDataHandler.Instance.portalBonus.migrationBonusPerLevel[0].bonusTalentPoints;
		if (GameManager.Instance.gameHasStarted)
		{
			ThePortal thePortal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
			int num = ((WorldSettings.Instance.worldSettingsData.playerSkillSettings.omnipotentMode == OMNIPOTENT_MODE.Enabled) ? 4 : thePortal.level);
			bonusTalentPoints = PortalBonusDataHandler.Instance.portalBonus.migrationBonusPerLevel[num - 1].bonusTalentPoints;
			PlayerSkillManager.Instance.GetSelectedLoadout();
		}
		List<CHARACTER_TALENT> upgradeableTalents = RuinarchListPool<CHARACTER_TALENT>.Claim();
		for (int j = 0; j < bonusTalentPoints; j++)
		{
			upgradeableTalents.Clear();
			list.ForEach(delegate(CHARACTER_TALENT eachTalent)
			{
				if (GetTalent(eachTalent).level < 5)
				{
					upgradeableTalents.Add(eachTalent);
				}
			});
			if (upgradeableTalents.Count <= 0)
			{
				break;
			}
			randomElement = CollectionUtilities.GetRandomElement(upgradeableTalents);
			GetTalent(randomElement).LevelUp(p_character);
		}
		RuinarchListPool<CHARACTER_TALENT>.Release(upgradeableTalents);
		RuinarchListPool<CHARACTER_TALENT>.Release(list);
	}

	private void LoadAllTalents(SaveDataCharacterTalentComponent data)
	{
		allTalents = new List<CharacterTalent>();
		_talentDictionary = new Dictionary<CHARACTER_TALENT, CharacterTalent>();
		for (int i = 0; i < data.allTalents.Count; i++)
		{
			CharacterTalent characterTalent = data.allTalents[i].Load();
			allTalents.Add(characterTalent);
			_talentDictionary.Add(characterTalent.talentType, characterTalent);
		}
	}

	public void ReevaluateAllTalents()
	{
		for (int i = 0; i < allTalents.Count; i++)
		{
			allTalents[i].ReevaluateTalent(base.owner);
		}
	}

	public CharacterTalent GetTalent(CHARACTER_TALENT p_talentType)
	{
		if (_talentDictionary.ContainsKey(p_talentType))
		{
			return _talentDictionary[p_talentType];
		}
		return null;
	}

	public string GetTalentSummary()
	{
		string text = string.Empty;
		for (int i = 0; i < allTalents.Count; i++)
		{
			if (i > 0)
			{
				text += "\n";
			}
			CharacterTalent characterTalent = allTalents[i];
			CharacterTalentData orCreateCharacterTalentData = CharacterManager.Instance.talentManager.GetOrCreateCharacterTalentData(characterTalent.talentType);
			text = text + "Lvl." + characterTalent.level + " " + orCreateCharacterTalentData.localizedName + ": " + characterTalent.experience + "/" + 100;
		}
		return text;
	}

	public void PopulateHighestAbleCombatantClasses(List<string> classes)
	{
		CharacterTalent talent = GetTalent(CHARACTER_TALENT.Martial_Arts);
		(CharacterManager.Instance.talentManager.GetOrCreateCharacterTalentData(talent.talentType) as MartialArtsData).PopulateHighestClasses(classes, talent.level);
		CharacterTalent talent2 = GetTalent(CHARACTER_TALENT.Combat_Magic);
		(CharacterManager.Instance.talentManager.GetOrCreateCharacterTalentData(talent2.talentType) as CombatMagicData).PopulateHighestClasses(classes, talent2.level);
		if (base.owner.traitContainer.HasTrait("Demon Cultist"))
		{
			classes.Remove("Stalker");
		}
	}

	public void LoadReferences(SaveDataCharacterTalentComponent data)
	{
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}
}
