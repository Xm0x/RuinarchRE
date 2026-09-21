using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FactionEmblem : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private Faction faction;

	[SerializeField]
	private Image emblemImage;

	[SerializeField]
	private bool alwaysShowEmblem;

	public void SetFaction(Faction faction)
	{
		this.faction = faction;
		UpdateEmblem();
	}

	public void ShowFactionInfo()
	{
		if (faction == null)
		{
			return;
		}
		string text = faction.nameWithColor + "\n" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Relationship_Summary_Title");
		foreach (KeyValuePair<Faction, FactionRelationship> relationship in faction.relationships)
		{
			text = text + "\n" + relationship.Key.nameWithColor + " - " + relationship.Value.localizedRelationshipStatus;
		}
		UIManager.Instance.ShowSmallInfo(text, "", autoReplaceText: false);
	}

	public void HideSmallInfo()
	{
		if (faction != null)
		{
			UIManager.Instance.HideSmallInfo();
		}
	}

	private void UpdateEmblem()
	{
		if (alwaysShowEmblem)
		{
			if (faction == null)
			{
				base.gameObject.SetActive(value: false);
				return;
			}
			base.gameObject.SetActive(value: true);
			emblemImage.sprite = faction.emblem;
		}
		else if (faction == null || !faction.factionType.shouldShowFactionEmblem)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			base.gameObject.SetActive(value: true);
			emblemImage.sprite = faction.emblem;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (faction != null)
		{
			UIManager.Instance.ShowFactionInfo(faction);
		}
	}
}
