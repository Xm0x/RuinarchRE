using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class StringNameplateItem : NameplateItem<string>
{
	[SerializeField]
	private LocationPortrait _locationPortrait;

	[SerializeField]
	private TextMeshProUGUI additionalText;

	public Image img;

	public string identifier { get; private set; }

	public string str { get; private set; }

	public override void SetObject(string o)
	{
		base.SetObject(o);
		str = o;
		base.name = str;
		button.name = str;
		base.toggle.name = str;
		identifier = string.Empty;
		mainLbl.text = str;
		additionalText.text = string.Empty;
	}

	public void SetIdentifier(string id)
	{
		identifier = id;
		_locationPortrait.gameObject.SetActive(value: false);
		img.gameObject.SetActive(value: false);
		if (identifier == "Landmark")
		{
			_locationPortrait.gameObject.SetActive(value: true);
			string text = str.Replace(' ', '_');
			LANDMARK_TYPE landmarkType = (LANDMARK_TYPE)Enum.Parse(typeof(LANDMARK_TYPE), text.ToUpper());
			_locationPortrait.SetPortrait(landmarkType.GetStructureType());
			_locationPortrait.disableInteraction = true;
		}
		else if (identifier == "Intervention Ability")
		{
			img.gameObject.SetActive(value: true);
			img.sprite = PlayerManager.Instance.GetJobActionSprite(str);
		}
		else if (identifier == "player skill")
		{
			img.gameObject.SetActive(value: true);
			img.sprite = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>((PLAYER_SKILL_TYPE)Enum.Parse(typeof(PLAYER_SKILL_TYPE), Utilities.NotNormalizedConversionStringToEnum(str).ToUpper())).buttonSprite;
		}
	}

	public override void Reset()
	{
		base.Reset();
		button.name = "Button";
		base.toggle.name = "Toggle";
	}
}
