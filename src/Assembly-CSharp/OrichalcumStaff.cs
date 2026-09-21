public class OrichalcumStaff : WeaponItem
{
	public OrichalcumStaff()
	{
		Initialize(TILE_OBJECT_TYPE.ORICHALCUM_STAFF, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public OrichalcumStaff(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
