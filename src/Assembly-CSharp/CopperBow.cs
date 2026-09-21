public class CopperBow : WeaponItem
{
	public CopperBow()
	{
		Initialize(TILE_OBJECT_TYPE.COPPER_BOW, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public CopperBow(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
