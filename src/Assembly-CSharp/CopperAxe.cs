public class CopperAxe : WeaponItem
{
	public CopperAxe()
	{
		Initialize(TILE_OBJECT_TYPE.COPPER_AXE, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public CopperAxe(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
