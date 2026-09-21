public class CopperStaff : WeaponItem
{
	public CopperStaff()
	{
		Initialize(TILE_OBJECT_TYPE.COPPER_STAFF, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public CopperStaff(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
