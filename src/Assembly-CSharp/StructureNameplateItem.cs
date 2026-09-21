using Inner_Maps.Location_Structures;
using UnityEngine;

public class StructureNameplateItem : NameplateItem<LocationStructure>
{
	[Header("Settlement Attributes")]
	[SerializeField]
	private LocationPortrait portrait;

	[Space(10f)]
	[Header("Store Target")]
	[SerializeField]
	private StoreTargetButton btnStoreTarget;

	private LocationStructure _structure;

	public override void SetObject(LocationStructure o)
	{
		base.SetObject(o);
		_structure = o;
		btnStoreTarget.SetTarget(_structure);
		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		mainLbl.text = _structure.name;
		subLbl.text = string.Empty;
		portrait.SetPortrait(_structure.structureType);
	}

	public void SetPosition(UIHoverPosition position)
	{
		UIManager.Instance.PositionTooltip(position, base.gameObject, base.transform as RectTransform);
	}

	public override void Reset()
	{
		base.Reset();
		_structure = null;
	}
}
