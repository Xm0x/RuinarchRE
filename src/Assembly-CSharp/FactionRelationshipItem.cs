using TMPro;
using UnityEngine;
using UtilityScripts;

public class FactionRelationshipItem : MonoBehaviour
{
	[SerializeField]
	private FactionEmblem emblem;

	[SerializeField]
	private TextMeshProUGUI nameLbl;

	[SerializeField]
	private TextMeshProUGUI statusLbl;

	public void SetData(Faction faction, FactionRelationship rel)
	{
		emblem.SetFaction(faction);
		nameLbl.text = faction.nameWithColor;
		statusLbl.text = "<color=" + rel.relationshipStatus.FactionRelationshipColor() + ">" + Utilities.NormalizeStringUpperCaseFirstLetterOnly(rel.localizedRelationshipStatus) + "</color>";
	}
}
