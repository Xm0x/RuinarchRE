using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;
using UtilityScripts;

public class ProtectionData : SkillData
{
	private string _bonusUIText = string.Empty;

	private string _bonusLevelUpUIText = string.Empty;

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.PROTECTION;

	public override string name => "Protection";

	public override string description => "This Spell will apply Protection to all non-hostile units within its area of effect - significantly reducing damage they receive.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public override int radius => PlayerSkillManager.Instance.GetTileRangeBonusPerLevel(PLAYER_SKILL_TYPE.PROTECTION);

	public ProtectionData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		int num = radius;
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		targetTile.PopulateTilesInRadius(list, num, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
		GameObject in_gameObjectID = GameManager.Instance.CreateParticleEffectAtWithScale(targetTile, PARTICLE_EFFECT.Protection, 3f + (float)num);
		AkSoundEngine.PostEvent("Play_Protection", in_gameObjectID);
		for (int i = 0; i < list.Count; i++)
		{
			LocationGridTile locationGridTile = list[i];
			for (int j = 0; j < locationGridTile.charactersHere.Count; j++)
			{
				Character character = locationGridTile.charactersHere[j];
				if ((!(locationGridTile.tileObjectComponent.objHere is Tombstone tombstone) || tombstone.character != character) && character.isNotHostileWithPlayer)
				{
					character.traitContainer.AddTrait(character, "Protection");
				}
			}
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		base.ActivateAbility(targetTile);
	}

	public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason)
	{
		bool flag = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
		if (flag)
		{
			return targetTile.structure != null;
		}
		return flag;
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(radius, tile);
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
			_bonusUIText = string.Format("{0} {1}", Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Resistances") + ":"), increaseStatsPercentagePerLevel);
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
				_bonusLevelUpUIText = string.Format("{0} {1}", Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Resistances") + ":"), increaseStatsPercentagePerLevel);
			}
			else
			{
				float increaseStatsPercentagePerLevel2 = PlayerSkillManager.Instance.GetIncreaseStatsPercentagePerLevel(this, base.currentLevel + 1);
				_bonusLevelUpUIText = string.Format("{0} {1} {2} {3}", Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Resistances") + ":"), increaseStatsPercentagePerLevel, Utilities.UpgradeArrowIcon(), Utilities.ColorizeUpgradeText($"{increaseStatsPercentagePerLevel2}"));
			}
		}
		return _bonusLevelUpUIText;
	}

	private Status GetStatusByLevel(int p_level)
	{
		string p_traitName = "Gloomy1";
		switch (p_level)
		{
		case 0:
			p_traitName = "Gloomy1";
			break;
		case 1:
			p_traitName = "Gloomy2";
			break;
		case 2:
			p_traitName = "Gloomy3";
			break;
		case 3:
			p_traitName = "Gloomy4";
			break;
		}
		if (TraitManager.Instance != null)
		{
			return TraitManager.Instance.GetTrait(p_traitName) as Status;
		}
		return null;
	}
}
