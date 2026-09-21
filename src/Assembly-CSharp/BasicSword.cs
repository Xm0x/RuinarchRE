public class BasicSword : WeaponItem
{
	public BasicSword()
	{
		Initialize(TILE_OBJECT_TYPE.BASIC_SWORD, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public BasicSword(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
