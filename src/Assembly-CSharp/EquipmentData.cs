using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment Data", menuName = "Scriptable Objects/Equipments/EquipmentData")]
public class EquipmentData : ScriptableObject
{
	[HideInInspector]
	[Header("if filled = any of type")]
	[Tooltip("if filled ignore specific resources")]
	public RESOURCE resourceType = RESOURCE.NONE;

	[HideInInspector]
	[Tooltip("if resources is not NONE, this one will be ignored")]
	public List<CONCRETE_RESOURCES> specificResource = new List<CONCRETE_RESOURCES>();

	public int resourceAmount;

	public int purchaseCost;

	public int tier;

	[HideInInspector]
	public string description;

	public Sprite imgIcon;

	public bool isLegendary;

	public EquipmentUpgradeData equipmentUpgradeData;

	[ContextMenu("Set 20 Cost resources")]
	public void SetResourceCostTo20()
	{
		resourceAmount = 20;
	}
}
