using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class SummonSkeletonsData : SkillData
{
	private string _bonusUIText = string.Empty;

	private string _bonusLevelUpUIText = string.Empty;

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SUMMON_SKELETONS;

	public override string name => "Summon Skeletons";

	public override string description => "This Spell will spawn Skeletons on the target ground. The number of Skeletons spawned increases as you upgrade this Spell.\nSkeletons summoned by this Spell can only survive for a short time.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public override int radius => 1;

	public SummonSkeletonsData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		targetTile.PopulateTilesInRadius(list, radius, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
		int num = GameUtilities.RandomBetweenTwoNumbers(GetMinimumNumberOfSkeletons(base.currentLevel), GetMaximumNumberOfSkeletons(base.currentLevel));
		int durationBonusPerLevel = PlayerSkillManager.Instance.GetDurationBonusPerLevel(type);
		GameObject gameObject = null;
		for (int i = 0; i < num; i++)
		{
			LocationGridTile randomElement = CollectionUtilities.GetRandomElement(list);
			Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Skeleton, PlayerManager.Instance.player.playerFaction, null, randomElement.parentMap.region, null, "", bypassIdeologyChecking: true);
			summon.traitContainer.AddTrait(summon, "Ephemeral", null, bypassElementalChance: false, durationBonusPerLevel);
			CharacterManager.Instance.PlaceSummonInitially(summon, randomElement);
			if (gameObject == null && summon.hasMarker)
			{
				gameObject = summon.marker.gameObject;
			}
		}
		AkSoundEngine.PostEvent("Play_Summon_Skeletons", gameObject);
		RuinarchListPool<LocationGridTile>.Release(list);
		base.ActivateAbility(targetTile);
	}

	public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason)
	{
		bool flag = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
		if (flag)
		{
			List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
			targetTile.PopulateTilesInRadius(list, radius, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
			for (int i = 0; i < list.Count; i++)
			{
				LocationGridTile locationGridTile = list[i];
				if (!locationGridTile.IsPassable() || locationGridTile.structure.structureType == STRUCTURE_TYPE.KENNEL || locationGridTile.structure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS)
				{
					flag = false;
					break;
				}
			}
		}
		return flag;
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(radius, tile);
	}

	public override string GetBonusUIText()
	{
		if (string.IsNullOrEmpty(_bonusUIText))
		{
			int minimumNumberOfSkeletons = GetMinimumNumberOfSkeletons(base.currentLevel);
			int maximumNumberOfSkeletons = GetMaximumNumberOfSkeletons(base.currentLevel);
			_bonusUIText = string.Format("{0} {1}-{2}", Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Skeletons") + ":"), minimumNumberOfSkeletons, maximumNumberOfSkeletons);
		}
		return _bonusUIText;
	}

	public override string GetBonusLevelUpUIText()
	{
		if (string.IsNullOrEmpty(_bonusLevelUpUIText))
		{
			int minimumNumberOfSkeletons = GetMinimumNumberOfSkeletons(base.currentLevel);
			int maximumNumberOfSkeletons = GetMaximumNumberOfSkeletons(base.currentLevel);
			if (base.isMaxLevel)
			{
				_bonusLevelUpUIText = string.Format("{0} {1}-{2}", Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Skeletons") + ":"), minimumNumberOfSkeletons, maximumNumberOfSkeletons);
			}
			else
			{
				int minimumNumberOfSkeletons2 = GetMinimumNumberOfSkeletons(base.currentLevel + 1);
				int maximumNumberOfSkeletons2 = GetMaximumNumberOfSkeletons(base.currentLevel + 1);
				_bonusLevelUpUIText = string.Format("{0} {1}-{2} {3} {4}", Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Skeletons") + ":"), minimumNumberOfSkeletons, maximumNumberOfSkeletons, Utilities.UpgradeArrowIcon(), Utilities.ColorizeUpgradeText($"{minimumNumberOfSkeletons2}-{maximumNumberOfSkeletons2}"));
			}
		}
		return _bonusLevelUpUIText;
	}

	protected override void OnLevelUp()
	{
		base.OnLevelUp();
		ResetBonusUIText();
		ResetBonusLevelUpUIText();
	}

	private void ResetBonusUIText()
	{
		_bonusUIText = string.Empty;
	}

	private void ResetBonusLevelUpUIText()
	{
		_bonusLevelUpUIText = string.Empty;
	}

	public int GetMinimumNumberOfSkeletons(int p_level)
	{
		int result = 1;
		switch (p_level)
		{
		case 1:
			result = 1;
			break;
		case 2:
			result = 2;
			break;
		case 3:
			result = 2;
			break;
		}
		return result;
	}

	public int GetMaximumNumberOfSkeletons(int p_level)
	{
		int result = 3;
		switch (p_level)
		{
		case 1:
			result = 4;
			break;
		case 2:
			result = 5;
			break;
		case 3:
			result = 6;
			break;
		}
		return result;
	}
}
