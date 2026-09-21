using System.Linq;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettlementInfoUI : InfoUIBase
{
	[Space(10f)]
	[Header("Basic Info")]
	[SerializeField]
	private TextMeshProUGUI nameLbl;

	[SerializeField]
	private TextMeshProUGUI typeLbl;

	[SerializeField]
	private LocationPortrait locationPortrait;

	[Space(10f)]
	[Header("Migration Meter")]
	[SerializeField]
	private Image meterImg;

	public BaseSettlement activeSettlement { get; private set; }

	public override void CloseMenu()
	{
		base.CloseMenu();
		if (activeSettlement != null)
		{
			Selector.Instance.Deselect();
			GameObject gameObject = null;
			LocationStructure locationStructure = activeSettlement.allStructures.FirstOrDefault();
			if (locationStructure is ManMadeStructure manMadeStructure)
			{
				gameObject = manMadeStructure.structureObj.gameObject;
			}
			else if (locationStructure is DemonicStructure demonicStructure)
			{
				gameObject = demonicStructure.structureObj.gameObject;
			}
			if (gameObject != null && InnerMapCameraMove.Instance.target == gameObject.transform)
			{
				InnerMapCameraMove.Instance.CenterCameraOn(null);
			}
		}
		activeSettlement = null;
	}

	public override void OpenMenu()
	{
		activeSettlement = _data as BaseSettlement;
		LocationStructure locationStructure = activeSettlement.allStructures.FirstOrDefault();
		locationStructure?.CenterOnStructure();
		base.OpenMenu();
		locationStructure?.ShowSelectorOnStructure();
		UpdateSettlementInfoUI();
	}

	public void UpdateSettlementInfoUI()
	{
		if (activeSettlement != null)
		{
			UpdateBasicInfo();
			UpdateInfo();
		}
	}

	private void UpdateBasicInfo()
	{
		nameLbl.text = activeSettlement.name ?? "";
		if (activeSettlement is NPCSettlement { settlementType: not null } nPCSettlement)
		{
			string text = string.Empty;
			if (nPCSettlement.owner != null)
			{
				text = LocalizationManager.Instance.GetLocalizedValue("Faction_Table", nPCSettlement.owner.factionType.type.ToStringEnum());
			}
			typeLbl.text = text;
		}
		locationPortrait.SetLocation(activeSettlement);
		LocationStructure locationStructure = activeSettlement.allStructures.FirstOrDefault();
		if (locationStructure != null)
		{
			locationPortrait.SetPortrait(locationStructure.structureType);
		}
	}

	private void UpdateInfo()
	{
		if (activeSettlement is NPCSettlement nPCSettlement)
		{
			meterImg.fillAmount = nPCSettlement.migrationComponent.GetNormalizedMigrationMeterValue();
		}
		else
		{
			meterImg.fillAmount = 0f;
		}
	}

	public void OnHoverEnterMigrationMeter()
	{
		if (activeSettlement is NPCSettlement nPCSettlement)
		{
			string hoverTextOfMigrationMeter = nPCSettlement.migrationComponent.GetHoverTextOfMigrationMeter();
			if (!string.IsNullOrEmpty(hoverTextOfMigrationMeter))
			{
				UIManager.Instance.ShowSmallInfo(hoverTextOfMigrationMeter);
			}
		}
	}

	public void OnHoverExitMigrationMeter()
	{
		UIManager.Instance.HideSmallInfo();
	}
}
