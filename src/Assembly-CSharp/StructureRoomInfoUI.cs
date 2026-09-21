using System.Linq;
using Inner_Maps.Location_Structures;
using TMPro;
using UnityEngine;

public class StructureRoomInfoUI : InfoUIBase
{
	[Space(10f)]
	[Header("Basic Info")]
	[SerializeField]
	private TextMeshProUGUI nameLbl;

	[SerializeField]
	private LocationPortrait locationPortrait;

	public StructureRoom activeRoom { get; private set; }

	internal override void Initialize()
	{
		base.Initialize();
		Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
		ListenToPlayerActionSignals();
	}

	public override void CloseMenu()
	{
		base.CloseMenu();
		if (activeRoom != null)
		{
			Selector.Instance.Deselect();
		}
		activeRoom = null;
	}

	public override void OpenMenu()
	{
		activeRoom = _data as StructureRoom;
		base.OpenMenu();
		Selector.Instance.Select(activeRoom);
		UpdateInfo();
		LoadActions(activeRoom);
	}

	private void OnStructureDestroyed(LocationStructure p_structure)
	{
		if (isShowing && activeRoom != null && p_structure.rooms.Contains(activeRoom))
		{
			CloseMenu();
		}
	}

	public void UpdateInfo()
	{
		if (activeRoom != null)
		{
			UpdateBasicInfo();
		}
	}

	private void UpdateBasicInfo()
	{
		nameLbl.text = activeRoom.name ?? "";
		if (activeRoom.parentStructure != null)
		{
			locationPortrait.SetPortrait(activeRoom.parentStructure.structureType);
		}
	}
}
