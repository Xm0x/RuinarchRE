public class IronAxe : WeaponItem
{
	public IronAxe()
	{
		Initialize(TILE_OBJECT_TYPE.IRON_AXE, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public IronAxe(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
