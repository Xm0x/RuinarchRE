using Inner_Maps.Location_Structures;
using TMPro;
using UnityEngine;

public class UnbuiltStructureInfoUI : InfoUIBase
{
	[Space(10f)]
	[Header("Basic Info")]
	[SerializeField]
	private TextMeshProUGUI nameLbl;

	[SerializeField]
	private TextMeshProUGUI subLbl;

	[SerializeField]
	private LocationPortrait locationPortrait;

	public LocationStructureObject activeStructureObject { get; private set; }

	public void OnBuiltStructure(LocationStructure p_structure)
	{
		if (isShowing)
		{
			if (p_structure is DemonicStructure demonicStructure && demonicStructure.structureObj == activeStructureObject)
			{
				CloseMenu();
				UIManager.Instance.ShowStructureInfo(p_structure, centerOnStructure: false);
			}
			else if (p_structure is ManMadeStructure manMadeStructure && manMadeStructure.structureObj == activeStructureObject)
			{
				CloseMenu();
				UIManager.Instance.ShowStructureInfo(p_structure, centerOnStructure: false);
			}
		}
	}

	public override void CloseMenu()
	{
		base.CloseMenu();
		if (activeStructureObject != null)
		{
			Selector.Instance.Deselect();
			GameObject gameObject = activeStructureObject.gameObject;
			if (InnerMapCameraMove.Instance.target == gameObject.transform)
			{
				InnerMapCameraMove.Instance.CenterCameraOn(null);
			}
		}
		activeStructureObject = null;
	}

	public override void OpenMenu()
	{
		activeStructureObject = _data as LocationStructureObject;
		InnerMapCameraMove.Instance.CenterCameraOn(activeStructureObject.gameObject);
		base.OpenMenu();
		Selector.Instance.Select(activeStructureObject);
		UpdateUnbuiltStructureInfoUI();
	}

	public void UpdateUnbuiltStructureInfoUI()
	{
		if (!(activeStructureObject == null))
		{
			UpdateBasicInfo();
		}
	}

	private void UpdateBasicInfo()
	{
		int tickDifference = GridMap.Instance.mainRegion.innerMap.GetTileFromWorldPosition(activeStructureObject.worldPosition).tileObjectComponent.genericTileObject.selfBuildingStructureDueDate.GetTickDifference(GameManager.Instance.Today());
		nameLbl.text = activeStructureObject.structureType.LocalizedStructureName();
		subLbl.text = "(" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Ticks_Remaining") + " " + tickDifference + ")";
		locationPortrait.SetPortrait(activeStructureObject.structureType);
	}
}
