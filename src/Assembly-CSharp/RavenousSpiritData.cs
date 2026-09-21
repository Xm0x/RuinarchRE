using Inner_Maps;
using UtilityScripts;

public class RavenousSpiritData : SkillData
{
	private string _bonusUIText = string.Empty;

	private string _bonusLevelUpUIText = string.Empty;

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.RAVENOUS_SPIRIT;

	public override string name => "Ravenous Spirit";

	public override string description => "This Spell summons a Ravenous Spirit that will drain Fullness from a nearby Villager.\nA Villager produces a Chaos Orb whenever it becomes Starving.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public RavenousSpiritData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		RavenousSpirit ravenousSpirit = InnerMapManager.Instance.CreateNewTileObject<RavenousSpirit>(TILE_OBJECT_TYPE.RAVENOUS_SPIRIT);
		ravenousSpirit.SetGridTileLocation(targetTile);
		ravenousSpirit.OnPlacePOI();
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
			_bonusUIText = string.Format("{0} {1}", Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Fullness_Drained") + ":"), increaseStatsPercentagePerLevel);
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
				_bonusLevelUpUIText = string.Format("{0} {1}", Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Fullness_Drained") + ":"), increaseStatsPercentagePerLevel);
			}
			else
			{
				float increaseStatsPercentagePerLevel2 = PlayerSkillManager.Instance.GetIncreaseStatsPercentagePerLevel(this, base.currentLevel + 1);
				_bonusLevelUpUIText = string.Format("{0} {1} {2} {3}", Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Fullness_Drained") + ":"), increaseStatsPercentagePerLevel, Utilities.UpgradeArrowIcon(), Utilities.ColorizeUpgradeText($"{increaseStatsPercentagePerLevel2}"));
			}
		}
		return _bonusLevelUpUIText;
	}
}
