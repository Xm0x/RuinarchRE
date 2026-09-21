public class BasicStaff : WeaponItem
{
	public BasicStaff()
	{
		Initialize(TILE_OBJECT_TYPE.BASIC_STAFF, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public BasicStaff(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
