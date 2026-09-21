using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Scriptable Objects/Equipments/WeaponData")]
public class WeaponData : EquipmentData
{
	[Header("Weapon Specific")]
	public WEAPON_TYPE weaponType;
}
