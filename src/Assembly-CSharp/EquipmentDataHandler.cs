using System.Collections.Generic;
using UnityEngine;

public class EquipmentDataHandler : MonoBehaviour
{
	public static EquipmentDataHandler Instance;

	public List<EquipmentData> allEquipmentsData = new List<EquipmentData>();

	private Dictionary<string, EquipmentData> _equipmentDataDictionary = new Dictionary<string, EquipmentData>();

	private void OnEnable()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	private void OnDisable()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void Awake()
	{
		for (int i = 0; i < allEquipmentsData.Count; i++)
		{
			EquipmentData equipmentData = allEquipmentsData[i];
			_equipmentDataDictionary.Add(equipmentData.name, equipmentData);
		}
	}

	public EquipmentData GetEquipmentDataBaseOnName(string p_name)
	{
		if (_equipmentDataDictionary.ContainsKey(p_name))
		{
			return _equipmentDataDictionary[p_name];
		}
		return null;
	}

	public List<CONCRETE_RESOURCES> GetResourcesNeeded(TILE_OBJECT_TYPE p_equipment)
	{
		string p_name = p_equipment.ToStringEnumWithSpace();
		return GetEquipmentDataBaseOnName(p_name).specificResource;
	}

	public RESOURCE GetGeneralResourcesNeeded(TILE_OBJECT_TYPE p_equipment)
	{
		string p_name = p_equipment.ToStringEnumWithSpace();
		return GetEquipmentDataBaseOnName(p_name).resourceType;
	}

	public int GetResourcesNeededAmount(TILE_OBJECT_TYPE p_equipment)
	{
		string p_name = p_equipment.ToStringEnumWithSpace();
		return GetEquipmentDataBaseOnName(p_name).resourceAmount;
	}
}
