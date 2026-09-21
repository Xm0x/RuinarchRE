using System;
using TMPro;
using UnityEngine;

public class SummonMinionPlayerSkillNameplateItem : SpellItem
{
	[Header("Summon/Minion PlayerSkill Nameplate Attributes")]
	[SerializeField]
	private CharacterPortrait classPortrait;

	[SerializeField]
	private TextMeshProUGUI countText;

	public override void SetObject(SkillData o)
	{
		base.SetObject(o);
		mainLbl.text = base.spellData.localizedName;
		subLbl.text = string.Empty;
		SetPortrait();
		ClearAllHoverEnterActions();
		AddHoverEnterAction(delegate(SkillData spellData)
		{
			PlayerUI.Instance.OnHoverSpell(spellData, PlayerUI.Instance.minionListHoverPosition);
		});
	}

	private void SetPortrait()
	{
		RACE rACE = RACE.NONE;
		string text = string.Empty;
		if (base.spellData is MinionPlayerSkill minionPlayerSkill)
		{
			rACE = minionPlayerSkill.race;
			text = minionPlayerSkill.className;
		}
		else if (base.spellData is SummonPlayerSkill summonPlayerSkill)
		{
			rACE = summonPlayerSkill.race;
			text = summonPlayerSkill.className;
		}
		if (rACE != RACE.NONE && text != string.Empty)
		{
			classPortrait.GeneratePortrait(CharacterManager.Instance.GeneratePortraitSettings(rACE, text));
			return;
		}
		throw new Exception("Trying to create portrait for " + base.spellData.name + " but Race or Class is None");
	}

	[Obsolete("Use UpdateData function instead")]
	public void SetCount(int count, bool useCountOnly = false)
	{
		if (!useCountOnly)
		{
			countText.text = count + "/" + base.spellData.charges;
		}
		else
		{
			countText.text = count.ToString() ?? "";
		}
	}
}
