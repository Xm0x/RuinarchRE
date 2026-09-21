public class IronBow : WeaponItem
{
	public IronBow()
	{
		Initialize(TILE_OBJECT_TYPE.IRON_BOW, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public IronBow(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
