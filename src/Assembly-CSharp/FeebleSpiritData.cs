using Inner_Maps;
using UtilityScripts;

public class FeebleSpiritData : SkillData
{
	private string _bonusUIText = string.Empty;

	private string _bonusLevelUpUIText = string.Empty;

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.FEEBLE_SPIRIT;

	public override string name => "Feeble Spirit";

	public override string description => "This Spell summons a Feeble Spirit that will drain Energy from a nearby Villager.\nA Villager produces a Chaos Orb whenever it becomes Exhausted.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public FeebleSpiritData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		FeebleSpirit feebleSpirit = InnerMapManager.Instance.CreateNewTileObject<FeebleSpirit>(TILE_OBJECT_TYPE.FEEBLE_SPIRIT);
		feebleSpirit.SetGridTileLocation(targetTile);
		feebleSpirit.OnPlacePOI();
		base.ActivateAbility(targetTile);
	}

	public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason)
	{
		bool flag = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
		if (flag)
		{
			if (targetTile.structure != null)
			{
				return targetTile.tileObjectComponent.objHere == null;
			}
			return false;
		}
		return flag;
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(0, tile);
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

	public override string GetBonusUIText()
	{
		if (string.IsNullOrEmpty(_bonusUIText))
		{
			float increaseStatsPercentagePerLevel = PlayerSkillManager.Instance.GetIncreaseStatsPercentagePerLevel(this);
			_bonusUIText = string.Format("{0} {1}", Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Energy_Drained") + ":"), increaseStatsPercentagePerLevel);
		}
		return _bonusUIText;
	}

	public override string GetBonusLevelUpUIText()
	{
		if (string.IsNullOrEmpty(_bonusLevelUpUIText))
		{
			float increaseStatsPercentagePerLevel = PlayerSkillManager.Instance.GetIncreaseStatsPercentagePerLevel(this);
			if (base.isMaxLevel)
			{
				_bonusLevelUpUIText = string.Format("{0} {1}", Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Energy_Drained") + ":"), increaseStatsPercentagePerLevel);
			}
			else
			{
				float increaseStatsPercentagePerLevel2 = PlayerSkillManager.Instance.GetIncreaseStatsPercentagePerLevel(this, base.currentLevel + 1);
				_bonusLevelUpUIText = string.Format("{0} {1} {2} {3}", Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Energy_Drained") + ":"), increaseStatsPercentagePerLevel, Utilities.UpgradeArrowIcon(), Utilities.ColorizeUpgradeText($"{increaseStatsPercentagePerLevel2}"));
			}
		}
		return _bonusLevelUpUIText;
	}
}
