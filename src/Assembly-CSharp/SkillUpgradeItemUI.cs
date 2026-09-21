using System;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.UI;

public class SkillUpgradeItemUI : MonoBehaviour
{
	public static Action<PLAYER_SKILL_TYPE> onHoverOverUpgradeItem;

	public static Action<PLAYER_SKILL_TYPE> onHoverOutUpgradeItem;

	public Action<PLAYER_SKILL_TYPE> onButtonClick;

	public RuinarchButton btnSkill;

	public RuinarchText txtSkillName;

	public RuinarchText txtUpgrade;

	public RuinarchText txtCost;

	public Image spiritIcon;

	public RuinarchText txtPlus;

	public HoverHandler hoverHandler;

	private PLAYER_SKILL_TYPE m_skillType;

	private void OnEnable()
	{
		btnSkill.onClick.AddListener(SkillClicked);
		hoverHandler.AddOnHoverOverAction(OnHoverOverItem);
		hoverHandler.AddOnHoverOutAction(OnHoverOutItem);
	}

	private void OnDisable()
	{
		btnSkill.onClick.RemoveListener(SkillClicked);
		hoverHandler.RemoveOnHoverOverAction(OnHoverOverItem);
		hoverHandler.RemoveOnHoverOutAction(OnHoverOutItem);
	}

	public void InitItem(PLAYER_SKILL_TYPE p_type, int p_spiritCount)
	{
		PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(p_type);
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(p_type);
		if (skillData.isMaxLevel)
		{
			txtCost.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "MAX");
			spiritIcon.gameObject.SetActive(value: false);
			btnSkill.gameObject.SetActive(value: false);
		}
		else
		{
			btnSkill.gameObject.SetActive(value: true);
			spiritIcon.gameObject.SetActive(value: true);
			txtCost.text = scriptableObjPlayerSkillData.skillUpgradeData.GetUpgradeCostBaseOnLevel(skillData.currentLevel).ToString();
			if (p_spiritCount < scriptableObjPlayerSkillData.skillUpgradeData.GetUpgradeCostBaseOnLevel(skillData.currentLevel))
			{
				btnSkill.interactable = false;
				txtPlus.color = new Color32(128, 128, 128, 128);
			}
			else
			{
				btnSkill.interactable = true;
				txtPlus.color = new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue);
			}
		}
		txtSkillName.text = skillData.localizedName;
		m_skillType = p_type;
	}

	private void SkillClicked()
	{
		onButtonClick?.Invoke(m_skillType);
		OnHoverOverItem();
	}

	private void OnHoverOverItem()
	{
		onHoverOverUpgradeItem?.Invoke(m_skillType);
	}

	private void OnHoverOutItem()
	{
		onHoverOutUpgradeItem?.Invoke(m_skillType);
	}
}
