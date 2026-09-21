public class IronStaff : WeaponItem
{
	public IronStaff()
	{
		Initialize(TILE_OBJECT_TYPE.IRON_STAFF, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public IronStaff(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
