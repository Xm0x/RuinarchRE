public class IronSword : WeaponItem
{
	public IronSword()
	{
		Initialize(TILE_OBJECT_TYPE.IRON_SWORD, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public IronSword(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
