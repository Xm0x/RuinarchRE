using System;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;

[Serializable]
public class SaveDataPlayer
{
	private TutorialManager.Tutorial_Type[] defaultUnlockedTutorials = new TutorialManager.Tutorial_Type[14]
	{
		TutorialManager.Tutorial_Type.Unlocking_Bonus_Powers,
		TutorialManager.Tutorial_Type.Upgrading_The_Portal,
		TutorialManager.Tutorial_Type.Mana,
		TutorialManager.Tutorial_Type.Chaotic_Energy,
		TutorialManager.Tutorial_Type.Storing_Targets,
		TutorialManager.Tutorial_Type.Maraud,
		TutorialManager.Tutorial_Type.Intel,
		TutorialManager.Tutorial_Type.Spirit_Energy,
		TutorialManager.Tutorial_Type.Migration_Controls,
		TutorialManager.Tutorial_Type.Base_Building,
		TutorialManager.Tutorial_Type.Abilities,
		TutorialManager.Tutorial_Type.Resistances,
		TutorialManager.Tutorial_Type.Time_Management,
		TutorialManager.Tutorial_Type.Target_Menu
	};

	public string gameVersion;

	public List<TutorialManager.Tutorial_Type> unlockedTutorials;

	public List<TutorialManager.Tutorial_Type> readTutorials;

	public List<TIPS> unlockedTips;

	public List<Game_Alert> alertsShown;

	public LoadoutSaveData ravagerLoadoutSaveData;

	public LoadoutSaveData puppetmasterLoadoutSaveData;

	public LoadoutSaveData lichLoadoutSaveData;

	public LoadoutSaveData attainmentRavagerLoadoutSaveData;

	public LoadoutSaveData attainmentPuppetmasterLoadoutSaveData;

	public LoadoutSaveData attainmentLichLoadoutSaveData;

	public LoadoutSaveData eradicationRavagerLoadoutSaveData;

	public LoadoutSaveData eradicationPuppetmasterLoadoutSaveData;

	public LoadoutSaveData eradicationLichLoadoutSaveData;

	public bool moreLoadoutOptions;

	public void InitializeInitialData()
	{
		gameVersion = Application.version;
		unlockedTips = new List<TIPS>();
		for (int i = 0; i < PlayerSkillManager.Instance.allSkillTrees.Length; i++)
		{
			PlayerSkillTree playerSkillTree = PlayerSkillManager.Instance.allSkillTrees[i];
			for (int j = 0; j < playerSkillTree.initialLearnedSkills.Length; j++)
			{
				PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = playerSkillTree.initialLearnedSkills[j];
				if (PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(pLAYER_SKILL_TYPE) != null)
				{
					LearnSkill(pLAYER_SKILL_TYPE, playerSkillTree.nodes[pLAYER_SKILL_TYPE]);
				}
			}
		}
		ravagerLoadoutSaveData = new LoadoutSaveData();
		puppetmasterLoadoutSaveData = new LoadoutSaveData();
		lichLoadoutSaveData = new LoadoutSaveData();
		attainmentRavagerLoadoutSaveData = new LoadoutSaveData();
		attainmentPuppetmasterLoadoutSaveData = new LoadoutSaveData();
		attainmentLichLoadoutSaveData = new LoadoutSaveData();
		eradicationRavagerLoadoutSaveData = new LoadoutSaveData();
		eradicationPuppetmasterLoadoutSaveData = new LoadoutSaveData();
		eradicationLichLoadoutSaveData = new LoadoutSaveData();
		CreateInitialUnlockedTutorials();
		readTutorials = new List<TutorialManager.Tutorial_Type>();
		alertsShown = new List<Game_Alert>();
	}

	public void LearnSkill(PLAYER_SKILL_TYPE skillType, PlayerSkillTreeNode node)
	{
	}

	private void CreateInitialUnlockedTutorials()
	{
		unlockedTutorials = new List<TutorialManager.Tutorial_Type>(defaultUnlockedTutorials);
	}

	public void UnlockTutorial(TutorialManager.Tutorial_Type p_type)
	{
		if (!unlockedTutorials.Contains(p_type))
		{
			unlockedTutorials.Add(p_type);
			Messenger.Broadcast(TutorialSignals.TUTORIAL_UNLOCKED, p_type);
		}
	}

	public void SetTutorialAsRead(TutorialManager.Tutorial_Type p_type)
	{
		if (!readTutorials.Contains(p_type))
		{
			readTutorials.Add(p_type);
			Messenger.Broadcast(TutorialSignals.TUTORIAL_READ, p_type);
		}
	}

	public bool HasTutorialBeenRead(TutorialManager.Tutorial_Type p_type)
	{
		return readTutorials.Contains(p_type);
	}

	public void SetTutorialAlertAsDone(Game_Alert p_alert)
	{
		if (!alertsShown.Contains(p_alert))
		{
			alertsShown.Add(p_alert);
		}
	}

	public bool IsTutorialAlertDone(Game_Alert p_alert)
	{
		return alertsShown.Contains(p_alert);
	}

	public void ResetTutorialAlerts()
	{
		alertsShown.Clear();
	}

	public LoadoutSaveData GetLoadout(PLAYER_ARCHETYPE archetype)
	{
		return archetype switch
		{
			PLAYER_ARCHETYPE.Progression_Ravager => ravagerLoadoutSaveData, 
			PLAYER_ARCHETYPE.Progression_Puppet_Master => puppetmasterLoadoutSaveData, 
			PLAYER_ARCHETYPE.Progression_Lich => lichLoadoutSaveData, 
			PLAYER_ARCHETYPE.Attainment_Lich => attainmentLichLoadoutSaveData, 
			PLAYER_ARCHETYPE.Attainment_Puppet_Master => attainmentPuppetmasterLoadoutSaveData, 
			PLAYER_ARCHETYPE.Attainment_Ravager => attainmentRavagerLoadoutSaveData, 
			PLAYER_ARCHETYPE.Eradication_Lich => eradicationLichLoadoutSaveData, 
			PLAYER_ARCHETYPE.Eradication_Puppet_Master => eradicationPuppetmasterLoadoutSaveData, 
			PLAYER_ARCHETYPE.Eradication_Ravager => eradicationRavagerLoadoutSaveData, 
			_ => null, 
		};
	}

	public void SaveLoadoutExtraSpells(PLAYER_ARCHETYPE archetype, List<SkillSlotItem> slotItems)
	{
		LoadoutSaveData loadout = GetLoadout(archetype);
		if (loadout == null)
		{
			return;
		}
		for (int i = 0; i < slotItems.Count; i++)
		{
			SkillSlotItem skillSlotItem = slotItems[i];
			if (skillSlotItem.skillData != null)
			{
				loadout.AddExtraSpell(skillSlotItem.skillData.skill);
			}
		}
	}

	public void SaveLoadoutExtraAfflictions(PLAYER_ARCHETYPE archetype, List<SkillSlotItem> slotItems)
	{
		LoadoutSaveData loadout = GetLoadout(archetype);
		if (loadout == null)
		{
			return;
		}
		for (int i = 0; i < slotItems.Count; i++)
		{
			SkillSlotItem skillSlotItem = slotItems[i];
			if (skillSlotItem.skillData != null)
			{
				loadout.AddExtraAffliction(skillSlotItem.skillData.skill);
			}
		}
	}

	public void SaveLoadoutExtraMinions(PLAYER_ARCHETYPE archetype, List<SkillSlotItem> slotItems)
	{
		LoadoutSaveData loadout = GetLoadout(archetype);
		if (loadout == null)
		{
			return;
		}
		for (int i = 0; i < slotItems.Count; i++)
		{
			SkillSlotItem skillSlotItem = slotItems[i];
			if (skillSlotItem.skillData != null)
			{
				loadout.AddExtraMinion(skillSlotItem.skillData.skill);
			}
		}
	}

	public void SaveLoadoutExtraStructures(PLAYER_ARCHETYPE archetype, List<SkillSlotItem> slotItems)
	{
		LoadoutSaveData loadout = GetLoadout(archetype);
		if (loadout == null)
		{
			return;
		}
		for (int i = 0; i < slotItems.Count; i++)
		{
			SkillSlotItem skillSlotItem = slotItems[i];
			if (skillSlotItem.skillData != null)
			{
				loadout.AddExtraStructure(skillSlotItem.skillData.skill);
			}
		}
	}

	public void SaveLoadoutExtraMiscs(PLAYER_ARCHETYPE archetype, List<SkillSlotItem> slotItems)
	{
		LoadoutSaveData loadout = GetLoadout(archetype);
		if (loadout == null)
		{
			return;
		}
		for (int i = 0; i < slotItems.Count; i++)
		{
			SkillSlotItem skillSlotItem = slotItems[i];
			if (skillSlotItem.skillData != null)
			{
				loadout.AddExtraMisc(skillSlotItem.skillData.skill);
			}
		}
	}

	public List<PLAYER_SKILL_TYPE> GetLoadoutExtraSpells(PLAYER_ARCHETYPE archetype)
	{
		return GetLoadout(archetype)?.extraSpells;
	}

	public List<PLAYER_SKILL_TYPE> GetLoadoutExtraAfflictions(PLAYER_ARCHETYPE archetype)
	{
		return GetLoadout(archetype)?.extraAfflictions;
	}

	public List<PLAYER_SKILL_TYPE> GetLoadoutExtraMinions(PLAYER_ARCHETYPE archetype)
	{
		return GetLoadout(archetype)?.extraMinions;
	}

	public List<PLAYER_SKILL_TYPE> GetLoadoutExtraStructures(PLAYER_ARCHETYPE archetype)
	{
		return GetLoadout(archetype)?.extraStructures;
	}

	public List<PLAYER_SKILL_TYPE> GetLoadoutExtraMiscs(PLAYER_ARCHETYPE archetype)
	{
		return GetLoadout(archetype)?.extraMiscs;
	}

	public void ClearLoadoutSaveData(PLAYER_ARCHETYPE archetype)
	{
		LoadoutSaveData loadout = GetLoadout(archetype);
		if (loadout != null)
		{
			loadout.ClearExtraSpells();
			loadout.ClearExtraAfflictions();
			loadout.ClearExtraMinions();
			loadout.ClearExtraStructures();
			loadout.ClearExtraMiscs();
		}
	}

	public void SetMoreLoadoutOptions(bool state)
	{
		moreLoadoutOptions = state;
	}

	public void ProcessOnLoad()
	{
		gameVersion = Application.version;
		if (unlockedTutorials == null)
		{
			CreateInitialUnlockedTutorials();
		}
		else
		{
			for (int i = 0; i < defaultUnlockedTutorials.Length; i++)
			{
				TutorialManager.Tutorial_Type item = defaultUnlockedTutorials[i];
				if (!unlockedTutorials.Contains(item))
				{
					unlockedTutorials.Add(item);
				}
			}
		}
		if (readTutorials == null)
		{
			readTutorials = new List<TutorialManager.Tutorial_Type>();
		}
		if (unlockedTips == null)
		{
			unlockedTips = new List<TIPS>();
		}
		if (alertsShown == null)
		{
			alertsShown = new List<Game_Alert>();
		}
	}
}
