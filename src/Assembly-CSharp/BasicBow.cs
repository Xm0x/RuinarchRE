public class BasicBow : WeaponItem
{
	public BasicBow()
	{
		Initialize(TILE_OBJECT_TYPE.BASIC_BOW, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public BasicBow(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
