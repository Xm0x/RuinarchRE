using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FactionItem : PooledObject
{
	public GameObject selectedGO;

	public Image emblem;

	public TextMeshProUGUI nameLbl;

	public TextMeshProUGUI typeLbl;

	public Material grayscaleMat;

	private Color origColorName;

	private Color origColorType = new Color(0.80784315f, 0.7137255f, 0.4862745f);

	private bool isInGrayscale;

	public Faction faction { get; private set; }

	public void SetFaction(Faction faction)
	{
		origColorName = FactionManager.Instance.factionNameColor;
		this.faction = faction;
		emblem.sprite = faction.emblem;
		nameLbl.text = faction.name;
		TextMeshProUGUI textMeshProUGUI = nameLbl;
		textMeshProUGUI.text = textMeshProUGUI.text + " (" + faction.GetAliveNonEphemeralMembersCount() + ")";
		typeLbl.text = faction.factionType.displayName;
	}

	public void UpdateFaction()
	{
		if (faction != null)
		{
			emblem.sprite = faction.emblem;
			nameLbl.text = faction.name;
			TextMeshProUGUI textMeshProUGUI = nameLbl;
			textMeshProUGUI.text = textMeshProUGUI.text + " (" + faction.GetAliveNonEphemeralMembersCount() + ")";
			typeLbl.text = faction.factionType.displayName;
		}
	}

	public void SetSelected(bool state)
	{
		selectedGO.SetActive(state);
		SetGrayscale(!state);
	}

	private void SetGrayscale(bool state)
	{
		isInGrayscale = state;
		if (isInGrayscale)
		{
			emblem.material = grayscaleMat;
			nameLbl.color = Color.gray;
			typeLbl.color = Color.gray;
		}
		else
		{
			emblem.material = null;
			nameLbl.color = origColorName;
			typeLbl.color = origColorType;
		}
	}

	public override void Reset()
	{
		base.Reset();
		faction = null;
		selectedGO.SetActive(value: false);
		isInGrayscale = false;
		emblem.material = null;
		nameLbl.color = origColorName;
		typeLbl.color = origColorType;
	}
}
