using System.Collections.Generic;
using System.Text;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Maccima_Games.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class PlayerSkillDetailsTooltip : MonoBehaviour
{
	public RectTransform thisRect;

	public TextMeshProUGUI titleText;

	public TextMeshProUGUI levelText;

	public RuinarchText descriptionText;

	public TextMeshProUGUI currenciesText;

	public TextMeshProUGUI bonusesText;

	public TextMeshProUGUI additionalText;

	public TextMeshProUGUI remainingChaosOrbsText;

	public GameObject additionalTextBG;

	public UIHoverPosition defaultPosition;

	public GameObject imageContainer;

	public Image image;

	public void ShowPlayerSkillDetails(SkillData spellData, UIHoverPosition position = null, bool p_dontShowAdditionalText = false, IPlayerActionTarget p_target = null, bool p_showImage = false, Vector2 p_spriteSize = default(Vector2))
	{
		UpdateData(spellData, p_dontShowAdditionalText, p_target, p_showImage, p_spriteSize);
		UpdatePosition(position);
	}

	public void ShowPlayerSkillDetails(PlayerSkillData skillData, int level = 0, UIHoverPosition position = null)
	{
		UpdateData(skillData, level);
		UpdatePosition(position);
	}

	public void ShowPlayerSkillDetails(string title, string description, UIHoverPosition position = null, bool autoReplaceText = true, string additionalText = "")
	{
		UpdateData(title, description, autoReplaceText, additionalText, null, Vector2.zero);
		UpdatePosition(position);
	}

	public void ShowPlayerSkillDetails(string title, string description, Sprite p_sprite, UIHoverPosition position = null, bool autoReplaceText = true, string additionalText = "", Vector2 p_spriteSize = default(Vector2))
	{
		UpdateData(title, description, autoReplaceText, additionalText, p_sprite, p_spriteSize);
		UpdatePosition(position);
	}

	public void ShowPlayerSkillWithLevelUpDetails(SkillData spellData, UIHoverPosition position = null, bool p_isChaoticEnergyEnough = true)
	{
		UpdateDataWithLevelUpDetails(spellData, p_isChaoticEnergyEnough);
		UpdatePosition(position);
	}

	public void HidePlayerSkillDetails()
	{
		base.gameObject.SetActive(value: false);
	}

	private void UpdatePosition(UIHoverPosition position)
	{
		UIHoverPosition uIHoverPosition = position;
		if (uIHoverPosition == null)
		{
			uIHoverPosition = defaultPosition;
		}
		thisRect.SetParent(uIHoverPosition.transform);
		thisRect.pivot = uIHoverPosition.pivot;
		Utilities.GetAnchorMinMax(uIHoverPosition.anchor, out var anchorMin, out var anchorMax);
		thisRect.anchorMin = anchorMin;
		thisRect.anchorMax = anchorMax;
		thisRect.anchoredPosition = Vector2.zero;
		thisRect.sizeDelta = new Vector2(thisRect.sizeDelta.x, 264f);
		base.gameObject.SetActive(value: true);
	}

	private void UpdateData(PlayerSkillData p_playerSkillData, int level)
	{
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(p_playerSkillData.skill);
		imageContainer.SetActive(value: false);
		titleText.SetText(skillData.localizedName);
		descriptionText.SetTextAndReplaceWithIcons(skillData.localizedDescription);
		levelText.text = $"{LocalizationManager.Level} {level + 1}";
		currenciesText.text = skillData.GetCurrencyUIText();
		additionalText.text = string.Empty;
		bonusesText.text = string.Empty;
		additionalTextBG.SetActive(value: false);
		string bonusUIText = skillData.GetBonusUIText();
		bonusUIText = bonusUIText + "\n" + p_playerSkillData.GetUpgradeBonusUIText(level);
		bonusUIText = bonusUIText + "\n" + p_playerSkillData.GetChargesBonusUIText();
		bonusUIText = bonusUIText + "\n" + p_playerSkillData.GetResistanceBonusUIText();
		if (p_playerSkillData.isAffliction)
		{
			string afflictionBonusUIText = p_playerSkillData.GetAfflictionBonusUIText(level);
			if (!string.IsNullOrEmpty(afflictionBonusUIText))
			{
				bonusUIText = bonusUIText + "\n" + afflictionBonusUIText;
			}
		}
		bonusUIText = bonusUIText + "\n" + p_playerSkillData.GetPowerAddedEffectsUIText(level);
		bonusesText.text = bonusUIText;
		UpdateRemainingChaosOrbsText(skillData);
	}

	private void UpdateData(string title, string description, bool autoReplaceText, string p_additionalText, Sprite p_sprite, Vector2 p_spriteSize)
	{
		titleText.text = title;
		image.sprite = p_sprite;
		if (p_spriteSize != Vector2.zero)
		{
			image.rectTransform.sizeDelta = p_spriteSize;
		}
		imageContainer.SetActive(p_sprite != null);
		if (autoReplaceText)
		{
			descriptionText.SetTextAndReplaceWithIcons(description);
		}
		else
		{
			descriptionText.text = description;
		}
		currenciesText.text = string.Empty;
		if (string.IsNullOrEmpty(p_additionalText))
		{
			additionalText.text = string.Empty;
			additionalTextBG.SetActive(value: false);
		}
		else
		{
			additionalText.text = p_additionalText;
			additionalTextBG.SetActive(value: true);
		}
		bonusesText.text = string.Empty;
		levelText.text = string.Empty;
		UpdateRemainingChaosOrbsText(null);
	}

	private void UpdateData(SkillData skillData, bool p_dontShowAdditionalText = false, IPlayerActionTarget p_target = null, bool p_showImage = false, Vector2 p_spriteSize = default(Vector2))
	{
		PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(skillData.type);
		titleText.text = skillData.localizedName;
		string text = skillData.localizedDescription;
		if (skillData is BrainwashData)
		{
			Character character = p_target as Character;
			if (p_target is Character character2)
			{
				character = character2;
			}
			else if (p_target is TortureChambers tortureChambers && tortureChambers.rooms.Length != 0 && tortureChambers.rooms[0] is PrisonCell prisonCell && prisonCell.HasCharacterInRoom())
			{
				character = prisonCell.GetFirstAliveCharacterInRoom();
			}
			if (character != null)
			{
				text = text + "\n\n<b>" + character.name + " " + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Brainwash_Success_Rate") + " " + PrisonCell.GetBrainwashSuccessRate(character).ToString("N0") + "%</b>";
			}
		}
		descriptionText.SetTextAndReplaceWithIcons(text);
		if (p_showImage)
		{
			image.sprite = scriptableObjPlayerSkillData.tooltipImage;
			image.rectTransform.sizeDelta = p_spriteSize;
			imageContainer.SetActive(image.sprite != null);
		}
		else
		{
			imageContainer.SetActive(value: false);
		}
		levelText.text = (scriptableObjPlayerSkillData.isNonUpgradeable ? string.Empty : $"{LocalizationManager.Level} {skillData.levelForDisplay}");
		currenciesText.text = skillData.GetCurrencyUIText();
		StringBuilder stringBuilder = new StringBuilder();
		string upgradeBonusUIText = scriptableObjPlayerSkillData.GetUpgradeBonusUIText(skillData.currentLevel);
		string maxChargesUIText = skillData.GetMaxChargesUIText();
		string resistanceBonusUIText = scriptableObjPlayerSkillData.GetResistanceBonusUIText();
		if (!string.IsNullOrEmpty(upgradeBonusUIText))
		{
			stringBuilder.AppendLine(upgradeBonusUIText);
		}
		if (!string.IsNullOrEmpty(maxChargesUIText))
		{
			stringBuilder.AppendLine(maxChargesUIText);
		}
		if (!string.IsNullOrEmpty(resistanceBonusUIText))
		{
			stringBuilder.AppendLine(resistanceBonusUIText);
		}
		if (scriptableObjPlayerSkillData.isAffliction)
		{
			string afflictionBonusUIText = scriptableObjPlayerSkillData.GetAfflictionBonusUIText(skillData.currentLevel);
			if (!string.IsNullOrEmpty(afflictionBonusUIText))
			{
				stringBuilder.AppendLine(afflictionBonusUIText);
			}
		}
		string bonusUIText = skillData.GetBonusUIText();
		if (!string.IsNullOrEmpty(bonusUIText))
		{
			stringBuilder.AppendLine(bonusUIText);
		}
		string powerAddedEffectsUIText = scriptableObjPlayerSkillData.GetPowerAddedEffectsUIText(skillData.currentLevel);
		if (!string.IsNullOrEmpty(powerAddedEffectsUIText))
		{
			stringBuilder.Append(powerAddedEffectsUIText);
		}
		bonusesText.text = stringBuilder.ToString().TrimEnd();
		if (p_dontShowAdditionalText)
		{
			additionalText.text = string.Empty;
			additionalTextBG.SetActive(value: false);
		}
		else
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			AdditionalInvalidInfoStringBuilder(skillData, p_target, stringBuilder2);
			if (skillData.type == PLAYER_SKILL_TYPE.AGITATE && p_target is Summon summon)
			{
				bool canBeAgitated = true;
				string agitateToolTipDescription = summon.GetAgitateToolTipDescription(ref canBeAgitated);
				if (!string.IsNullOrEmpty(agitateToolTipDescription))
				{
					if (!canBeAgitated || summon.traitContainer.HasTrait("Agitated"))
					{
						stringBuilder2.AppendLine(Utilities.ColorizeInvalidText(agitateToolTipDescription));
					}
					else
					{
						stringBuilder2.AppendLine(agitateToolTipDescription);
					}
				}
			}
			additionalText.text = stringBuilder2.ToString().TrimEnd();
			additionalTextBG.SetActive(!string.IsNullOrEmpty(additionalText.text));
		}
		UpdateRemainingChaosOrbsText(skillData);
	}

	private void UpdateDataWithLevelUpDetails(SkillData spellData, bool p_isChaoticEnergyEnough = true)
	{
		PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(spellData.type);
		imageContainer.SetActive(value: false);
		titleText.text = spellData.localizedName;
		descriptionText.text = spellData.localizedDescription;
		levelText.text = (scriptableObjPlayerSkillData.isNonUpgradeable ? string.Empty : $"{LocalizationManager.Level} {spellData.levelForDisplay} {Utilities.UpgradeArrowIcon()} {Utilities.ColorizeUpgradeText($"{LocalizationManager.Level} {spellData.levelForDisplay + 1}")}");
		currenciesText.text = spellData.GetCurrencyLevelUpUIText(scriptableObjPlayerSkillData);
		StringBuilder stringBuilder = new StringBuilder();
		string upgradeBonusLevelUpUIText = scriptableObjPlayerSkillData.GetUpgradeBonusLevelUpUIText(spellData.currentLevel);
		if (!string.IsNullOrEmpty(upgradeBonusLevelUpUIText))
		{
			stringBuilder.AppendLine(upgradeBonusLevelUpUIText);
		}
		string maxChargesBonusLevelUpUIText = spellData.GetMaxChargesBonusLevelUpUIText(scriptableObjPlayerSkillData);
		if (!string.IsNullOrEmpty(maxChargesBonusLevelUpUIText))
		{
			stringBuilder.AppendLine(maxChargesBonusLevelUpUIText);
		}
		string resistanceBonusUIText = scriptableObjPlayerSkillData.GetResistanceBonusUIText();
		if (!string.IsNullOrEmpty(resistanceBonusUIText))
		{
			stringBuilder.AppendLine(resistanceBonusUIText);
		}
		if (scriptableObjPlayerSkillData.isAffliction)
		{
			string afflictionBonusesWithLevelUpDetailsString = scriptableObjPlayerSkillData.GetAfflictionBonusesWithLevelUpDetailsString(spellData.currentLevel);
			if (!string.IsNullOrEmpty(afflictionBonusesWithLevelUpDetailsString))
			{
				stringBuilder.AppendLine(afflictionBonusesWithLevelUpDetailsString);
			}
		}
		string bonusLevelUpUIText = spellData.GetBonusLevelUpUIText();
		if (!string.IsNullOrEmpty(bonusLevelUpUIText))
		{
			stringBuilder.AppendLine(bonusLevelUpUIText);
		}
		string powerAddedEffectsWithLevelUpUIText = scriptableObjPlayerSkillData.GetPowerAddedEffectsWithLevelUpUIText(spellData.currentLevel);
		if (!string.IsNullOrEmpty(powerAddedEffectsWithLevelUpUIText))
		{
			stringBuilder.Append(powerAddedEffectsWithLevelUpUIText);
		}
		bonusesText.text = stringBuilder.ToString().TrimEnd();
		additionalText.text = GetAdditionalInfoForSpire(p_isChaoticEnergyEnough);
		additionalTextBG.SetActive(!string.IsNullOrEmpty(additionalText.text));
		UpdateRemainingChaosOrbsText(spellData);
	}

	private void UpdateRemainingChaosOrbsText(SkillData p_skillData)
	{
		if (!(remainingChaosOrbsText != null))
		{
			return;
		}
		if (p_skillData == null)
		{
			remainingChaosOrbsText.text = string.Empty;
			return;
		}
		if (string.IsNullOrEmpty(currenciesText.text) || p_skillData.type == PLAYER_SKILL_TYPE.BRAINWASH)
		{
			remainingChaosOrbsText.text = string.Empty;
			remainingChaosOrbsText.gameObject.SetActive(value: false);
			return;
		}
		if (p_skillData.hasUnliChaosOrbs)
		{
			remainingChaosOrbsText.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Unlimited_Chaos_Orbs");
		}
		else if (p_skillData.hasRemainingChaosOrbs)
		{
			Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
			dictionary.Add("chaosOrbs", p_skillData.remainingChaosOrbs.ToString());
			remainingChaosOrbsText.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Remaining_Chaos_Orbs", dictionary);
			MaccimaDictionaryPool<string, string>.Release(dictionary);
		}
		else
		{
			remainingChaosOrbsText.text = string.Empty;
		}
		if (p_skillData.hasRemainingOrUnliChaosOrbs)
		{
			remainingChaosOrbsText.color = PlayerSkillManager.Instance.withChaosOrbsTextColor;
		}
		else
		{
			remainingChaosOrbsText.color = PlayerSkillManager.Instance.withoutChaosOrbsTextColor;
		}
		remainingChaosOrbsText.gameObject.SetActive(value: true);
	}

	private string GetBonusDifferenceString(UPGRADE_BONUS bonus, PlayerSkillData p_data, int p_level, int p_nextLevel)
	{
		string result = string.Empty;
		switch (bonus)
		{
		case UPGRADE_BONUS.Damage:
		{
			int mentalResistanceBaseOnLevel = p_data.skillUpgradeData.GetAdditionalDamageBaseOnLevel(p_level);
			int mentalResistanceBaseOnLevel2 = p_data.skillUpgradeData.GetAdditionalDamageBaseOnLevel(p_nextLevel);
			if (mentalResistanceBaseOnLevel2 - mentalResistanceBaseOnLevel > 0)
			{
				result = mentalResistanceBaseOnLevel2.ToString() ?? "";
			}
			break;
		}
		case UPGRADE_BONUS.Pierce:
		{
			float additionalPiercePerLevelBaseOnLevel = p_data.GetAdditionalPiercePerLevelBaseOnLevel(p_level);
			float additionalPiercePerLevelBaseOnLevel2 = p_data.GetAdditionalPiercePerLevelBaseOnLevel(p_nextLevel);
			if (additionalPiercePerLevelBaseOnLevel2 - additionalPiercePerLevelBaseOnLevel > 0f)
			{
				result = additionalPiercePerLevelBaseOnLevel2.ToString() ?? "";
			}
			break;
		}
		case UPGRADE_BONUS.Duration:
		{
			int mentalResistanceBaseOnLevel = p_data.skillUpgradeData.GetDurationBonusPerLevel(p_level);
			int mentalResistanceBaseOnLevel2 = p_data.skillUpgradeData.GetDurationBonusPerLevel(p_nextLevel);
			if (mentalResistanceBaseOnLevel != mentalResistanceBaseOnLevel2)
			{
				string empty = string.Empty;
				empty = ((!p_data.isAffliction || mentalResistanceBaseOnLevel != 0) ? GameManager.ConvertTicksToWholeTime(mentalResistanceBaseOnLevel) : LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Permanent"));
				result = empty ?? "";
			}
			break;
		}
		case UPGRADE_BONUS.Tile_Range:
		{
			int mentalResistanceBaseOnLevel = p_data.skillUpgradeData.GetTileRangeBonusPerLevel(p_level);
			int mentalResistanceBaseOnLevel2 = p_data.skillUpgradeData.GetTileRangeBonusPerLevel(p_nextLevel);
			if (mentalResistanceBaseOnLevel2 - mentalResistanceBaseOnLevel > 0)
			{
				result = GetTileRangeDisplay(mentalResistanceBaseOnLevel2) ?? "";
			}
			break;
		}
		case UPGRADE_BONUS.Movement_Speed:
		{
			int mentalResistanceBaseOnLevel = p_data.skillUpgradeData.GetSkillMovementSpeedPerLevel(p_level);
			int mentalResistanceBaseOnLevel2 = p_data.skillUpgradeData.GetSkillMovementSpeedPerLevel(p_nextLevel);
			if (mentalResistanceBaseOnLevel2 - mentalResistanceBaseOnLevel > 0)
			{
				result = mentalResistanceBaseOnLevel2.ToString() ?? "";
			}
			break;
		}
		case UPGRADE_BONUS.Cooldown:
		{
			int mentalResistanceBaseOnLevel = p_data.GetCoolDownBaseOnLevel(p_level);
			int mentalResistanceBaseOnLevel2 = p_data.GetCoolDownBaseOnLevel(p_nextLevel);
			if (mentalResistanceBaseOnLevel != mentalResistanceBaseOnLevel2)
			{
				result = mentalResistanceBaseOnLevel2 switch
				{
					0 => LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Instant"), 
					-1 => LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "No Cooldown"), 
					_ => GameManager.ConvertTicksToWholeTime(mentalResistanceBaseOnLevel2) ?? "", 
				};
			}
			break;
		}
		case UPGRADE_BONUS.Atk_Percentage:
		{
			float additionalPiercePerLevelBaseOnLevel = p_data.skillUpgradeData.GetAdditionalAttackPercentagePerLevelBaseOnLevel(p_level);
			float additionalPiercePerLevelBaseOnLevel2 = p_data.skillUpgradeData.GetAdditionalAttackPercentagePerLevelBaseOnLevel(p_nextLevel);
			if (additionalPiercePerLevelBaseOnLevel2 - additionalPiercePerLevelBaseOnLevel > 0f)
			{
				result = additionalPiercePerLevelBaseOnLevel2 + "%";
			}
			break;
		}
		case UPGRADE_BONUS.Physical_Resistance:
		{
			int mentalResistanceBaseOnLevel = p_data.skillUpgradeData.GetPhysicalResistanceBaseOnLevel(p_level);
			int mentalResistanceBaseOnLevel2 = p_data.skillUpgradeData.GetPhysicalResistanceBaseOnLevel(p_nextLevel);
			if (mentalResistanceBaseOnLevel != mentalResistanceBaseOnLevel2)
			{
				result = mentalResistanceBaseOnLevel2.ToString();
			}
			break;
		}
		case UPGRADE_BONUS.Elemental_Resistance:
		{
			int mentalResistanceBaseOnLevel = p_data.skillUpgradeData.GetElementalResistanceBaseOnLevel(p_level);
			int mentalResistanceBaseOnLevel2 = p_data.skillUpgradeData.GetElementalResistanceBaseOnLevel(p_nextLevel);
			if (mentalResistanceBaseOnLevel != mentalResistanceBaseOnLevel2)
			{
				result = mentalResistanceBaseOnLevel2.ToString();
			}
			break;
		}
		case UPGRADE_BONUS.Secondary_Resistance:
		{
			int mentalResistanceBaseOnLevel = p_data.skillUpgradeData.GetSecondaryResistanceBaseOnLevel(p_level);
			int mentalResistanceBaseOnLevel2 = p_data.skillUpgradeData.GetSecondaryResistanceBaseOnLevel(p_nextLevel);
			if (mentalResistanceBaseOnLevel != mentalResistanceBaseOnLevel2)
			{
				result = mentalResistanceBaseOnLevel2.ToString();
			}
			break;
		}
		case UPGRADE_BONUS.Mental_Resistance:
		{
			int mentalResistanceBaseOnLevel = p_data.skillUpgradeData.GetMentalResistanceBaseOnLevel(p_level);
			int mentalResistanceBaseOnLevel2 = p_data.skillUpgradeData.GetMentalResistanceBaseOnLevel(p_nextLevel);
			if (mentalResistanceBaseOnLevel != mentalResistanceBaseOnLevel2)
			{
				result = mentalResistanceBaseOnLevel2.ToString();
			}
			break;
		}
		default:
			result = string.Empty;
			break;
		}
		return result;
	}

	private void AdditionalInvalidInfoStringBuilder(SkillData spellData, IPlayerActionTarget p_target, StringBuilder p_strBuilder)
	{
		if (spellData is PlayerAction playerAction)
		{
			object obj = p_target;
			if (obj == null)
			{
				obj = UIManager.Instance.GetCurrentlySelectedObject() ?? PlayerManager.Instance.player.currentlySelectedPlayerActionTarget;
			}
			if (obj != null)
			{
				if (obj is Character targetCharacter)
				{
					if (spellData is SchemeData && spellData.type != PLAYER_SKILL_TYPE.SCHEME)
					{
						if (!PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.SCHEME).CanPerformAbilityTowards(targetCharacter))
						{
							string reasonsWhyCannotPerformAbilityTowards = spellData.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
							if (!string.IsNullOrEmpty(reasonsWhyCannotPerformAbilityTowards))
							{
								Utilities.SplitStringIntoNewLines(reasonsWhyCannotPerformAbilityTowards, '|', p_strBuilder);
							}
						}
						else if (!spellData.CanPerformAbilityTowards(targetCharacter))
						{
							string reasonsWhyCannotPerformAbilityTowards2 = spellData.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
							if (!string.IsNullOrEmpty(reasonsWhyCannotPerformAbilityTowards2))
							{
								Utilities.SplitStringIntoNewLines(reasonsWhyCannotPerformAbilityTowards2, '|', p_strBuilder);
							}
						}
					}
					else if (!spellData.CanPerformAbilityTowards(targetCharacter))
					{
						string reasonsWhyCannotPerformAbilityTowards3 = spellData.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
						if (!string.IsNullOrEmpty(reasonsWhyCannotPerformAbilityTowards3))
						{
							Utilities.SplitStringIntoNewLines(reasonsWhyCannotPerformAbilityTowards3, '|', p_strBuilder);
						}
					}
				}
				else if (obj is TileObject tileObject)
				{
					if (tileObject is AnkhOfAnubis { isActivated: not false } && spellData.type == PLAYER_SKILL_TYPE.SEIZE_OBJECT)
					{
						p_strBuilder.AppendLine(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Ankh_No_Seize"));
					}
					else if (tileObject is Tombstone { character: not null } tombstone)
					{
						if (!spellData.CanPerformAbilityTowards(tombstone.character))
						{
							string reasonsWhyCannotPerformAbilityTowards4 = spellData.GetReasonsWhyCannotPerformAbilityTowards(tombstone.character);
							if (!string.IsNullOrEmpty(reasonsWhyCannotPerformAbilityTowards4))
							{
								Utilities.SplitStringIntoNewLines(reasonsWhyCannotPerformAbilityTowards4, '|', p_strBuilder);
							}
						}
					}
					else
					{
						string reasonsWhyCannotPerformAbilityTowards5 = spellData.GetReasonsWhyCannotPerformAbilityTowards(tileObject);
						if (!string.IsNullOrEmpty(reasonsWhyCannotPerformAbilityTowards5))
						{
							Utilities.SplitStringIntoNewLines(reasonsWhyCannotPerformAbilityTowards5, '|', p_strBuilder);
						}
					}
				}
				else if (obj is BaseSettlement baseSettlement)
				{
					if (!spellData.CanPerformAbilityTowards(baseSettlement))
					{
						string reasonsWhyCannotPerformAbilityTowards6 = spellData.GetReasonsWhyCannotPerformAbilityTowards(baseSettlement);
						if (!string.IsNullOrEmpty(reasonsWhyCannotPerformAbilityTowards6))
						{
							Utilities.SplitStringIntoNewLines(reasonsWhyCannotPerformAbilityTowards6, '|', p_strBuilder);
						}
					}
				}
				else if (obj is LocationStructure locationStructure && !spellData.CanPerformAbilityTowards(locationStructure))
				{
					string reasonsWhyCannotPerformAbilityTowards7 = spellData.GetReasonsWhyCannotPerformAbilityTowards(locationStructure);
					if (!string.IsNullOrEmpty(reasonsWhyCannotPerformAbilityTowards7))
					{
						Utilities.SplitStringIntoNewLines(reasonsWhyCannotPerformAbilityTowards7, '|', p_strBuilder);
					}
				}
			}
			if (playerAction.type == PLAYER_SKILL_TYPE.AGITATE && p_target is Summon summon && summon.limiterComponent.IsIncapacitated())
			{
				p_strBuilder.AppendLine(summon.GetAgitateIncapacitatedMessage());
			}
		}
		else
		{
			string reasonsWhyInvalid = spellData.GetReasonsWhyInvalid();
			if (!string.IsNullOrEmpty(reasonsWhyInvalid))
			{
				Utilities.SplitStringIntoNewLines(reasonsWhyInvalid, '|', p_strBuilder);
			}
		}
		if (!HasEnoughMana(spellData))
		{
			p_strBuilder.AppendLine(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "No_Mana"));
		}
		if (!HasEnoughCharges(spellData))
		{
			if (spellData is SchemeData && spellData.type == PLAYER_SKILL_TYPE.SCHEME)
			{
				if (PlayerManager.Instance.player.playerSkillComponent.schemeInCooldown != PLAYER_SKILL_TYPE.NONE)
				{
					p_strBuilder.AppendLine(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Recharging"));
				}
				else
				{
					p_strBuilder.AppendLine(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "No_Charges"));
				}
			}
			else if (spellData.hasCooldown)
			{
				p_strBuilder.AppendLine(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Recharging"));
			}
			else
			{
				p_strBuilder.AppendLine(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "No_Charges"));
			}
		}
		if (p_strBuilder.Length > 0)
		{
			p_strBuilder.Insert(0, "<color=#FE3E83>");
			p_strBuilder.Append("</color>");
		}
	}

	private string GetAdditionalInfoForSpire(bool p_isChaoticEnergyEnough = true)
	{
		string text = string.Empty;
		if (!p_isChaoticEnergyEnough)
		{
			text = text + Utilities.ColorizeInvalidText(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "No_Chaotic_Energy")) + "\n";
		}
		return text;
	}

	private string GetTileRangeDisplay(int radius)
	{
		int num = radius * 2 + 1;
		return $"{num}x{num}";
	}

	private bool HasEnoughMana(SkillData spellData)
	{
		if (spellData is SchemeData)
		{
			SkillData playerActionData = PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.SCHEME);
			if (playerActionData.hasManaCost)
			{
				if (PlayerManager.Instance.player.currenciesComponent.mana >= playerActionData.manaCost)
				{
					return true;
				}
				return false;
			}
		}
		else if (spellData.hasManaCost)
		{
			if (PlayerManager.Instance.player.currenciesComponent.mana >= spellData.manaCost)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	private bool HasEnoughCharges(SkillData spellData)
	{
		if (spellData is SchemeData)
		{
			SkillData playerActionData = PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.SCHEME);
			if (playerActionData.hasCharges)
			{
				if (playerActionData.charges > 0)
				{
					return true;
				}
				return false;
			}
		}
		else if (spellData.hasCharges)
		{
			if (spellData.charges > 0)
			{
				return true;
			}
			return false;
		}
		return true;
	}
}
