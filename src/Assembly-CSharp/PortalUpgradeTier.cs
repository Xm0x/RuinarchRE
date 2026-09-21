using System;
using System.Collections.Generic;
using System.Linq;
using Quests;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI.Extensions;
using UtilityScripts;

[Serializable]
public class PortalUpgradeTier
{
	[ReadOnly]
	public string name;

	[ReadOnly]
	public int level;

	[FormerlySerializedAs("_isForAscendanceWin")]
	[SerializeField]
	private bool _isForProgressionWin;

	[FormerlySerializedAs("upgradeCost")]
	[SerializeField]
	private Cost[] _upgradeCost;

	[FormerlySerializedAs("upgradeTime")]
	[SerializeField]
	private int _upgradeTime;

	[FormerlySerializedAs("_upgradeItems")]
	[SerializeField]
	private PortalUpgradeItem[] _defaultUpgradeItems;

	[SerializeField]
	private PLAYER_SKILL_TYPE[] _optionalSkillsInTier;

	public Cost[] upgradeCost => _upgradeCost;

	public int upgradeTime
	{
		get
		{
			if (!ConsoleBase.fasterPortalUpgrade)
			{
				return _upgradeTime;
			}
			return 20;
		}
	}

	public PortalUpgradeItem[] defaultUpgradeItems => GetValidDefaultUpgradeItems();

	public PLAYER_SKILL_TYPE[] optionalSkillsInTier => _optionalSkillsInTier;

	public bool isForProgressionWin => _isForProgressionWin;

	public void PopulateValidOptionalSkillsForOptionalSelf(List<PLAYER_SKILL_TYPE> p_skills, IntRange p_range)
	{
		for (int i = p_range.lowerBound; i <= p_range.upperBound; i++)
		{
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = _optionalSkillsInTier[i];
			SkillData skillData = PlayerSkillManager.Instance.GetSkillData(pLAYER_SKILL_TYPE);
			bool flag = true;
			if (!p_skills.Contains(pLAYER_SKILL_TYPE) && skillData.category != PLAYER_SKILL_CATEGORY.MINION)
			{
				p_skills.Add(pLAYER_SKILL_TYPE);
			}
		}
	}

	public void PopulateUnlearnedEssentialAndOptionalSkills(List<PLAYER_SKILL_TYPE> p_skills)
	{
		for (int i = 0; i < _optionalSkillsInTier.Length; i++)
		{
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = _optionalSkillsInTier[i];
			SkillData skillData = PlayerSkillManager.Instance.GetSkillData(pLAYER_SKILL_TYPE);
			if (!skillData.isInUse && !PlayerSkillLoadout.All_Common_Structures.Contains(pLAYER_SKILL_TYPE) && skillData.category != PLAYER_SKILL_CATEGORY.MINION && !p_skills.Contains(pLAYER_SKILL_TYPE))
			{
				p_skills.Add(pLAYER_SKILL_TYPE);
			}
		}
		for (int j = 0; j < _defaultUpgradeItems.Length; j++)
		{
			PortalUpgradeItem portalUpgradeItem = _defaultUpgradeItems[j];
			if (portalUpgradeItem.portalUpgradeType == Portal_Upgrade_Type.Essential)
			{
				SkillData skillData2 = PlayerSkillManager.Instance.GetSkillData(portalUpgradeItem.essentialSkillToUnlock);
				if (!skillData2.isInUse && !PlayerSkillLoadout.All_Common_Structures.Contains(portalUpgradeItem.essentialSkillToUnlock) && skillData2.category != PLAYER_SKILL_CATEGORY.MINION && !p_skills.Contains(portalUpgradeItem.essentialSkillToUnlock))
				{
					p_skills.Add(portalUpgradeItem.essentialSkillToUnlock);
				}
			}
		}
	}

	public void PopulateUnlearnedSkillsForWildcard(List<PLAYER_SKILL_TYPE> p_skills, List<PLAYER_SKILL_TYPE> p_chosenWildcardPowers)
	{
		for (int i = 0; i < _optionalSkillsInTier.Length; i++)
		{
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = _optionalSkillsInTier[i];
			SkillData skillData = PlayerSkillManager.Instance.GetSkillData(pLAYER_SKILL_TYPE);
			if (!skillData.isInUse && IsSkillValidCategoryForWildcard(skillData) && !p_skills.Contains(pLAYER_SKILL_TYPE) && !p_chosenWildcardPowers.Contains(pLAYER_SKILL_TYPE))
			{
				p_skills.Add(pLAYER_SKILL_TYPE);
			}
		}
		for (int j = 0; j < _defaultUpgradeItems.Length; j++)
		{
			PortalUpgradeItem portalUpgradeItem = _defaultUpgradeItems[j];
			if (portalUpgradeItem.portalUpgradeType == Portal_Upgrade_Type.Essential)
			{
				SkillData skillData2 = PlayerSkillManager.Instance.GetSkillData(portalUpgradeItem.essentialSkillToUnlock);
				if (!skillData2.isInUse && IsSkillValidCategoryForWildcard(skillData2) && !p_skills.Contains(portalUpgradeItem.essentialSkillToUnlock) && !p_chosenWildcardPowers.Contains(portalUpgradeItem.essentialSkillToUnlock))
				{
					p_skills.Add(portalUpgradeItem.essentialSkillToUnlock);
				}
			}
		}
	}

	private bool IsSkillValidCategoryForWildcard(SkillData p_skillData)
	{
		if (p_skillData.category != PLAYER_SKILL_CATEGORY.AFFLICTION && p_skillData.category != PLAYER_SKILL_CATEGORY.SPELL && p_skillData.category != PLAYER_SKILL_CATEGORY.PLAYER_ACTION)
		{
			return p_skillData.category == PLAYER_SKILL_CATEGORY.SCHEME;
		}
		return true;
	}

	public string GetUpgradeCostString()
	{
		string text = string.Empty;
		for (int i = 0; i < upgradeCost.Length; i++)
		{
			Cost cost = upgradeCost[i];
			string currencyTextSprite = cost.currency.GetCurrencyTextSprite();
			text = text + " " + cost.processedAmount + currencyTextSprite;
			if (!upgradeCost.IsLastIndex(i))
			{
				text += ",";
			}
		}
		return text;
	}

	private PortalUpgradeItem[] GetValidDefaultUpgradeItems()
	{
		if (PlayerSkillManager.Instance != null && PlayerSkillManager.Instance.unlockAllSkills)
		{
			List<PortalUpgradeItem> list = RuinarchListPool<PortalUpgradeItem>.Claim();
			for (int i = 0; i < _defaultUpgradeItems.Length; i++)
			{
				PortalUpgradeItem portalUpgradeItem = _defaultUpgradeItems[i];
				if (portalUpgradeItem.portalUpgradeType != Portal_Upgrade_Type.Optional_Other && portalUpgradeItem.portalUpgradeType != Portal_Upgrade_Type.Optional_Self && portalUpgradeItem.portalUpgradeType != Portal_Upgrade_Type.Wildcard && portalUpgradeItem.portalUpgradeType != Portal_Upgrade_Type.Lesser_Demon)
				{
					list.Add(portalUpgradeItem);
				}
			}
			PortalUpgradeItem[] result = list.ToArray();
			RuinarchListPool<PortalUpgradeItem>.Release(list);
			return result;
		}
		return _defaultUpgradeItems;
	}

	public bool ShouldTierBeIncludedGivenVictoryCondition(VICTORY_CONDITION p_victoryCondition)
	{
		if (_isForProgressionWin)
		{
			if (QuestManager.Instance != null)
			{
				return p_victoryCondition == VICTORY_CONDITION.Progression;
			}
			return false;
		}
		return true;
	}

	public void ResetDataBeforeGameStart()
	{
	}

	public void SetUpgradeItems(PortalUpgradeItem[] p_items)
	{
		_defaultUpgradeItems = p_items;
	}
}
