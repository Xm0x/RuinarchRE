public class MithrilStaff : WeaponItem
{
	public MithrilStaff()
	{
		Initialize(TILE_OBJECT_TYPE.MITHRIL_STAFF, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public MithrilStaff(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
