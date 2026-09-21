public class CopperSword : WeaponItem
{
	public CopperSword()
	{
		Initialize(TILE_OBJECT_TYPE.COPPER_SWORD, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public CopperSword(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
