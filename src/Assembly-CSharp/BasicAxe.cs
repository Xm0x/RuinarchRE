public class BasicAxe : WeaponItem
{
	public BasicAxe()
	{
		Initialize(TILE_OBJECT_TYPE.BASIC_AXE, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public BasicAxe(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
