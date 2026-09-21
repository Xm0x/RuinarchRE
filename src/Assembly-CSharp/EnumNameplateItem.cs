using System;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class EnumNameplateItem : NameplateItem<Enum>
{
	[SerializeField]
	private Image portrait;

	public override void SetObject(Enum o)
	{
		base.SetObject(o);
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", o.ToString());
		string text = (base.name = ((!(o is TILE_OBJECT_TYPE)) ? Utilities.NormalizeStringUpperCaseFirstLetters(o.ToString()) : ((TILE_OBJECT_TYPE)(object)o).ToStringEnumWithSpace()));
		button.name = text;
		base.toggle.name = text;
		mainLbl.text = localizedValue;
	}

	public void SetPortrait(Sprite sprite)
	{
		portrait.sprite = sprite;
		portrait.gameObject.SetActive(portrait.sprite != null);
	}

	public override void Reset()
	{
		base.Reset();
		base.name = "Nameplate Item";
	}
}
