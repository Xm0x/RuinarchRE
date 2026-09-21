using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class ResearchAbilityUI : PopupMenuBase
{
	[Header("General")]
	public Button okBtn;

	public ToggleGroup toggleGroup;

	[Header("Ability 1")]
	public Toggle ability1Toggle;

	public Image ability1Icon;

	public TextMeshProUGUI ability1Text;

	[Header("Ability 2")]
	public Toggle ability2Toggle;

	public Image ability2Icon;

	public TextMeshProUGUI ability2Text;

	[Header("Ability 3")]
	public Toggle ability3Toggle;

	public Image ability3Icon;

	public TextMeshProUGUI ability3Text;

	public void ShowResearchUI()
	{
		if (PlayerUI.Instance.IsMajorUIShowing())
		{
			PlayerUI.Instance.AddPendingUI(delegate
			{
				ShowResearchUI();
			});
			return;
		}
		if (!GameManager.Instance.isPaused)
		{
			UIManager.Instance.Pause();
			UIManager.Instance.SetSpeedTogglesState(state: false);
		}
		ability1Toggle.isOn = false;
		ability2Toggle.isOn = false;
		ability3Toggle.isOn = false;
		okBtn.interactable = false;
		base.Open();
	}

	public void SetAbility1(PLAYER_SKILL_TYPE ability)
	{
		string text = Utilities.NormalizeStringUpperCaseFirstLetters(ability.ToStringEnum());
		ability1Icon.sprite = PlayerManager.Instance.GetJobActionSprite(text);
		string text2 = text;
		text2 = text2 + "\n" + PlayerSkillManager.Instance.allSpellsData[ability].localizedDescription;
		ability1Text.text = text2;
	}

	public void SetAbility2(PLAYER_SKILL_TYPE ability)
	{
		string text = Utilities.NormalizeStringUpperCaseFirstLetters(ability.ToStringEnum());
		ability2Icon.sprite = PlayerManager.Instance.GetJobActionSprite(text);
		string text2 = text;
		text2 = text2 + "\n" + PlayerSkillManager.Instance.allSpellsData[ability].localizedDescription;
		ability2Text.text = text2;
	}

	public void SetAbility3(PLAYER_SKILL_TYPE ability)
	{
		string text = Utilities.NormalizeStringUpperCaseFirstLetters(ability.ToStringEnum());
		ability3Icon.sprite = PlayerManager.Instance.GetJobActionSprite(text);
		string text2 = text;
		text2 = text2 + "\n" + PlayerSkillManager.Instance.allSpellsData[ability].localizedDescription;
		ability3Text.text = text2;
	}

	public void OnClickAbility1(bool state)
	{
		if (state)
		{
			okBtn.interactable = true;
		}
	}

	public void OnClickAbility2(bool state)
	{
		if (state)
		{
			okBtn.interactable = true;
		}
	}

	public void OnClickAbility3(bool state)
	{
		if (state)
		{
			okBtn.interactable = true;
		}
	}

	public override void Close()
	{
		base.Close();
		if (!PlayerUI.Instance.TryShowPendingUI() && !UIManager.Instance.IsObjectPickerOpen())
		{
			UIManager.Instance.ResumeLastProgressionSpeed();
		}
	}
}
