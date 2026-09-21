using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI.Extensions;
using UtilityScripts;

[Serializable]
public class PortalUpgradeItem
{
	public const int Optional_Power_Choice_Count = 3;

	[ReadOnly]
	public string name;

	[ReadOnly]
	public int tier;

	[ReadOnly]
	public int itemIndex;

	public Portal_Upgrade_Type portalUpgradeType;

	[Header("Essential")]
	public PLAYER_SKILL_TYPE essentialSkillToUnlock;

	[Header("Optional self")]
	[Tooltip("Index to use for optional powers, default value is 0-2 since by default there are only 3 optional powers.")]
	public IntRange optionalSelfTierRange;

	[Header("Optional other")]
	public int otherArchetypeTier;

	[Header("Common Structures")]
	public PLAYER_SKILL_TYPE[] commonStructureChoices;

	[HideInInspector]
	[SerializeField]
	private PLAYER_SKILL_TYPE _chosenPowerForUpgrade;

	[HideInInspector]
	[SerializeField]
	private PLAYER_SKILL_TYPE[] _powerChoices;

	public PLAYER_SKILL_TYPE chosenPowerForUpgrade => _chosenPowerForUpgrade;

	public PLAYER_SKILL_TYPE[] powerChoices => _powerChoices;

	public PortalUpgradeItem(PortalUpgradeItem p_item, PortalUpgradeTier p_parentTier)
	{
		name = p_item.name;
		tier = p_item.tier;
		itemIndex = p_item.itemIndex;
		portalUpgradeType = p_item.portalUpgradeType;
		essentialSkillToUnlock = p_item.essentialSkillToUnlock;
		otherArchetypeTier = p_item.otherArchetypeTier;
		optionalSelfTierRange = p_item.optionalSelfTierRange;
		_powerChoices = new PLAYER_SKILL_TYPE[3];
		commonStructureChoices = p_item.commonStructureChoices;
		PrePopulateChosenPowerForUpgrade();
		PrePopulatePowerChoices(p_parentTier);
	}

	public PortalUpgradeItem(Portal_Upgrade_Type p_upgradeType)
	{
		portalUpgradeType = p_upgradeType;
	}

	private void PrePopulateChosenPowerForUpgrade()
	{
		if (portalUpgradeType == Portal_Upgrade_Type.Essential)
		{
			SetChosenPowerForUpgrade(essentialSkillToUnlock);
		}
	}

	private void PrePopulatePowerChoices(PortalUpgradeTier p_parentTier)
	{
		if (portalUpgradeType == Portal_Upgrade_Type.Optional_Self)
		{
			int num = optionalSelfTierRange.lowerBound;
			for (int i = 0; i < powerChoices.Length; i++)
			{
				powerChoices[i] = p_parentTier.optionalSkillsInTier[num];
				num++;
			}
		}
	}

	public void SetChosenPowerForUpgrade(PLAYER_SKILL_TYPE p_type)
	{
		_chosenPowerForUpgrade = p_type;
	}

	public void PreselectOptionalPowers(PortalUpgradeTier p_parentTier, int p_currentLevel, List<PLAYER_SKILL_TYPE> chosenWildcardPowers)
	{
		List<PLAYER_SKILL_TYPE> list = RuinarchListPool<PLAYER_SKILL_TYPE>.Claim();
		PopulateRandomChoicesForUpgradeItem(p_parentTier, list, p_currentLevel, chosenWildcardPowers);
		if (list.Count > 0)
		{
			for (int i = 0; i < powerChoices.Length; i++)
			{
				PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = list.ElementAtOrDefault(i);
				powerChoices[i] = pLAYER_SKILL_TYPE;
				if (portalUpgradeType == Portal_Upgrade_Type.Wildcard)
				{
					chosenWildcardPowers.Add(pLAYER_SKILL_TYPE);
				}
			}
		}
		RuinarchListPool<PLAYER_SKILL_TYPE>.Release(list);
	}

	private void PopulateRandomChoicesForUpgradeItem(PortalUpgradeTier p_tier, List<PLAYER_SKILL_TYPE> p_choices, int p_currentLevel, List<PLAYER_SKILL_TYPE> p_chosenWildcardPowers)
	{
		switch (portalUpgradeType)
		{
		case Portal_Upgrade_Type.Optional_Other:
		{
			List<PlayerSkillLoadout> list5 = RuinarchListPool<PlayerSkillLoadout>.Claim();
			PlayerSkillManager.Instance.GetDifferentArchetypeLoadouts(list5);
			List<PLAYER_SKILL_TYPE> list6 = RuinarchListPool<PLAYER_SKILL_TYPE>.Claim();
			for (int m = 0; m < list5.Count; m++)
			{
				list5[m].GetUpgradeForTier(otherArchetypeTier).PopulateUnlearnedEssentialAndOptionalSkills(list6);
			}
			for (int n = 0; n < 3; n++)
			{
				if (list6.Count > 0)
				{
					PLAYER_SKILL_TYPE randomElement4 = CollectionUtilities.GetRandomElement(list6);
					p_choices.Add(randomElement4);
					list6.Remove(randomElement4);
				}
			}
			RuinarchListPool<PlayerSkillLoadout>.Release(list5);
			RuinarchListPool<PLAYER_SKILL_TYPE>.Release(list6);
			break;
		}
		case Portal_Upgrade_Type.Optional_Self:
		{
			List<PLAYER_SKILL_TYPE> list3 = RuinarchListPool<PLAYER_SKILL_TYPE>.Claim();
			p_tier.PopulateValidOptionalSkillsForOptionalSelf(list3, optionalSelfTierRange);
			for (int k = 0; k < 3; k++)
			{
				if (list3.Count > 0)
				{
					PLAYER_SKILL_TYPE randomElement2 = CollectionUtilities.GetRandomElement(list3);
					p_choices.Add(randomElement2);
					list3.Remove(randomElement2);
				}
			}
			RuinarchListPool<PLAYER_SKILL_TYPE>.Release(list3);
			break;
		}
		case Portal_Upgrade_Type.Common_Structures:
		{
			List<PLAYER_SKILL_TYPE> list4 = RuinarchListPool<PLAYER_SKILL_TYPE>.Claim();
			list4.AddRange(commonStructureChoices);
			for (int l = 0; l < 3; l++)
			{
				if (list4.Count > 0)
				{
					PLAYER_SKILL_TYPE randomElement3 = CollectionUtilities.GetRandomElement(list4);
					p_choices.Add(randomElement3);
					list4.Remove(randomElement3);
				}
			}
			break;
		}
		case Portal_Upgrade_Type.Lesser_Demon:
		{
			List<PLAYER_SKILL_TYPE> list7 = RuinarchListPool<PLAYER_SKILL_TYPE>.Claim();
			for (int num = 0; num < PlayerSkillManager.Instance.allMinionPlayerSkills.Length; num++)
			{
				PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = PlayerSkillManager.Instance.allMinionPlayerSkills[num];
				if (!PlayerSkillManager.Instance.GetSkillData(pLAYER_SKILL_TYPE).isInUse)
				{
					list7.Add(pLAYER_SKILL_TYPE);
				}
			}
			for (int num2 = 0; num2 < 3; num2++)
			{
				if (list7.Count > 0)
				{
					PLAYER_SKILL_TYPE randomElement5 = CollectionUtilities.GetRandomElement(list7);
					p_choices.Add(randomElement5);
					list7.Remove(randomElement5);
				}
			}
			RuinarchListPool<PLAYER_SKILL_TYPE>.Release(list7);
			break;
		}
		case Portal_Upgrade_Type.Wildcard:
		{
			List<PlayerSkillLoadout> list = RuinarchListPool<PlayerSkillLoadout>.Claim();
			PlayerSkillManager.Instance.GetAllMainArchetypeLoadouts(list);
			List<PLAYER_SKILL_TYPE> list2 = RuinarchListPool<PLAYER_SKILL_TYPE>.Claim();
			for (int i = 0; i < list.Count; i++)
			{
				list[i].PopulateValidSkillsForWildcard(list2, p_currentLevel, p_chosenWildcardPowers);
			}
			for (int j = 0; j < 3; j++)
			{
				if (list2.Count > 0)
				{
					PLAYER_SKILL_TYPE randomElement = CollectionUtilities.GetRandomElement(list2);
					p_choices.Add(randomElement);
					list2.Remove(randomElement);
				}
			}
			break;
		}
		}
	}

	public override string ToString()
	{
		string text = portalUpgradeType.ToStringEnum();
		switch (portalUpgradeType)
		{
		case Portal_Upgrade_Type.Essential:
			text = text + " - " + essentialSkillToUnlock.ToStringEnum();
			break;
		case Portal_Upgrade_Type.Optional_Other:
			text = text + " - Tier " + otherArchetypeTier;
			break;
		}
		return text;
	}
}
