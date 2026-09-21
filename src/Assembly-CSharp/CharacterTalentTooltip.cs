using Character_Talents;
using TMPro;
using UnityEngine;
using UtilityScripts;

public class CharacterTalentTooltip : MonoBehaviour
{
	public RectTransform thisRect;

	public TextMeshProUGUI titleText;

	public TextMeshProUGUI levelText;

	public RuinarchText descriptionText;

	public TextMeshProUGUI experienceText;

	public TextMeshProUGUI bonusesText;

	public UIHoverPosition defaultPosition;

	private void UpdatePosition(UIHoverPosition position)
	{
		base.gameObject.SetActive(value: true);
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
		thisRect.sizeDelta = new Vector2(thisRect.sizeDelta.x, 464f);
	}

	private void UpdateData(Character character, CHARACTER_TALENT type)
	{
		CharacterTalent talent = character.talentComponent.GetTalent(type);
		CharacterTalentData orCreateCharacterTalentData = CharacterManager.Instance.talentManager.GetOrCreateCharacterTalentData(talent.talentType);
		titleText.text = orCreateCharacterTalentData.localizedName;
		levelText.text = LocalizationManager.Level + " " + talent.level;
		descriptionText.text = orCreateCharacterTalentData.localizedDescription;
		if (talent.level >= 5)
		{
			experienceText.gameObject.SetActive(value: false);
		}
		else
		{
			experienceText.gameObject.SetActive(value: true);
			experienceText.text = talent.experience + " xp";
		}
		string text = orCreateCharacterTalentData.GetAdditionalBonusDescription(character, talent.level);
		if (!string.IsNullOrEmpty(text))
		{
			text = "\n" + text;
		}
		bonusesText.text = orCreateCharacterTalentData.GetBonusDescription(talent.level) + text;
	}

	public void ShowCharacterTalentData(Character character, CHARACTER_TALENT type, UIHoverPosition position = null)
	{
		UpdateData(character, type);
		UpdatePosition(position);
	}
}
