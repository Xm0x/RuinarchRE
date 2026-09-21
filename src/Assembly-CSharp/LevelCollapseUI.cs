using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelCollapseUI : MonoBehaviour
{
	public Text workAroundText;

	public Text lvlText;

	public Dropdown skillsOptions;

	public Image arrowImg;

	public Transform skillsContentTransform;

	public GameObject skillsGO;

	public GameObject skillsPerLevelBtnGO;

	private bool _isShowing;

	[NonSerialized]
	public SkillsPerLevelButton currentSelectedButton;

	private List<string> _skills;

	public List<string> skills => _skills;

	private void Awake()
	{
		if (_skills == null)
		{
			_skills = new List<string>();
		}
	}

	private void Start()
	{
		UpdateSkillList();
	}

	public void ToggleCollapse()
	{
		_isShowing = !_isShowing;
		if (_isShowing)
		{
			UpdateSkillList();
			arrowImg.rectTransform.localEulerAngles = new Vector3(0f, 0f, 0f);
			skillsGO.SetActive(value: true);
			workAroundText.text = "eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeerrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrr";
		}
		else
		{
			arrowImg.rectTransform.localEulerAngles = new Vector3(0f, 0f, 90f);
			skillsGO.SetActive(value: false);
			workAroundText.text = "eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeereeeeeeee";
		}
	}

	public void UpdateSkillList()
	{
		skillsOptions.ClearOptions();
		skillsOptions.AddOptions(SkillPanelUI.Instance.allSkills);
	}

	private IEnumerator UpdateSkillsSrollView()
	{
		yield return null;
		Vector2 sizeDelta = (base.transform as RectTransform).sizeDelta;
		if (_isShowing)
		{
			sizeDelta.y = 200f;
		}
		else
		{
			sizeDelta.y = 30f;
		}
		(base.transform as RectTransform).sizeDelta = sizeDelta;
		Canvas.ForceUpdateCanvases();
	}

	public void SetSkills(List<string> skills)
	{
		_skills = new List<string>(skills);
		for (int i = 0; i < _skills.Count; i++)
		{
			GameObject obj = UnityEngine.Object.Instantiate(skillsPerLevelBtnGO, skillsContentTransform);
			obj.GetComponent<SkillsPerLevelButton>().buttonText.text = _skills[i];
			obj.GetComponent<SkillsPerLevelButton>().collapseUI = this;
		}
	}

	public void OnClickAdd()
	{
		string text = skillsOptions.options[skillsOptions.value].text;
		if (!_skills.Contains(text))
		{
			_skills.Add(text);
			GameObject obj = UnityEngine.Object.Instantiate(skillsPerLevelBtnGO, skillsContentTransform);
			obj.GetComponent<SkillsPerLevelButton>().buttonText.text = text;
			obj.GetComponent<SkillsPerLevelButton>().collapseUI = this;
		}
	}

	public void OnClickRemove()
	{
		if (currentSelectedButton != null)
		{
			string text = currentSelectedButton.buttonText.text;
			if (_skills.Remove(text))
			{
				UnityEngine.Object.Destroy(currentSelectedButton.gameObject);
				currentSelectedButton = null;
			}
		}
	}
}
