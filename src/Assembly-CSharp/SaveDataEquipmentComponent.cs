using System;

[Serializable]
public class SaveDataEquipmentComponent : SaveData<EquipmentComponent>
{
	public string currentWeaponID = string.Empty;

	public string currentArmorID = string.Empty;

	public string currentAccessoryID = string.Empty;

	public override void Save(EquipmentComponent data)
	{
		if (data.currentWeapon != null)
		{
			currentWeaponID = data.currentWeapon.persistentID;
		}
		if (data.currentArmor != null)
		{
			currentArmorID = data.currentArmor.persistentID;
		}
		if (data.currentAccessory != null)
		{
			currentAccessoryID = data.currentAccessory.persistentID;
		}
	}

	public override EquipmentComponent Load()
	{
		base.Load();
		EquipmentComponent equipmentComponent = new EquipmentComponent();
		if (!string.IsNullOrEmpty(currentWeaponID))
		{
			equipmentComponent.currentWeapon = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(currentWeaponID) as EquipmentItem;
		}
		if (!string.IsNullOrEmpty(currentArmorID))
		{
			equipmentComponent.currentArmor = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(currentArmorID) as EquipmentItem;
		}
		if (!string.IsNullOrEmpty(currentAccessoryID))
		{
			equipmentComponent.currentAccessory = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(currentAccessoryID) as EquipmentItem;
		}
		if (equipmentComponent.currentWeapon != null)
		{
			equipmentComponent.allEquipments.Add(equipmentComponent.currentWeapon);
		}
		if (equipmentComponent.currentArmor != null)
		{
			equipmentComponent.allEquipments.Add(equipmentComponent.currentArmor);
		}
		if (equipmentComponent.currentAccessory != null)
		{
			equipmentComponent.allEquipments.Add(equipmentComponent.currentAccessory);
		}
		return equipmentComponent;
	}
}
